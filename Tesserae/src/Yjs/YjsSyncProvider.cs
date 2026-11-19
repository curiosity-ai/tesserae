using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;
using static Transpose.Core.es5;

namespace Tesserae
{
    /// <summary>
    /// Keeps a <see cref="Y.Doc"/> and its <see cref="Awareness"/> in sync with a peer (usually a server relaying to
    /// the other clients) over a transport the host owns. It speaks the y-websocket message protocol - a varint
    /// message type, then the payload - but sends through a callback and receives through <see cref="Receive(Uint8Array)"/>,
    /// so the transport can be a WebSocket, HTTP posts with server-sent events back, or anything else that moves bytes
    /// in order.
    /// </summary>
    /// <remarks>
    /// <para>Message types: <see cref="MessageSync"/> (y-protocols sync: step 1, step 2, update),
    /// <see cref="MessageAwareness"/>, <see cref="MessageAuth"/> (permission denied) and
    /// <see cref="MessageQueryAwareness"/> (asks for every awareness state).</para>
    /// <para><see cref="ConnectAsync"/> sends sync step 1 (this document's state vector) and the local awareness state.
    /// The peer answers with sync step 2 - what this document is missing - which marks the provider
    /// <see cref="IsSynced"/>. For the peer to learn what <i>it</i> is missing, it sends its own step 1 on a new
    /// connection, as the y-websocket server does; this provider answers any step 1 it receives.</para>
    /// <para>Local changes (any transaction whose origin is not this provider) are sent as sync updates while
    /// connected. Updates made in one burst - one frame - are merged into one message. Changes made while
    /// disconnected are not queued: the step 1 / step 2 exchange of the next <see cref="ConnectAsync"/> carries them.</para>
    /// <para>Messages are sent one after another: a send starts once the previous one has completed. A failed send is
    /// reported through <see cref="OnSendFailed"/> (or to the console when nothing listens) and clears
    /// <see cref="IsSynced"/>, because the peer may now be missing an update; connect again to resync.</para>
    /// </remarks>
    public sealed class YjsSyncProvider : IDisposable
    {
        /// <summary>A y-protocols sync message follows: step 1, step 2 or an update.</summary>
        public const int MessageSync = 0;

        /// <summary>An awareness update follows.</summary>
        public const int MessageAwareness = 1;

        /// <summary>An auth message follows; type 0 is "permission denied" with a reason.</summary>
        public const int MessageAuth = 2;

        /// <summary>The peer asks for every awareness state this side knows.</summary>
        public const int MessageQueryAwareness = 3;

        /// <summary>The transaction origin of the awareness removal <see cref="Dispose"/> broadcasts.</summary>
        public const string DisposeOrigin = "YjsSyncProvider.Dispose";

        private readonly Y.Doc                    _doc;
        private readonly Awareness                _awareness;
        private readonly SettableObservable<bool> _synced = new SettableObservable<bool>(false);
        private readonly List<Uint8Array>         _pendingUpdates = new List<Uint8Array>();

        private readonly Action<Uint8Array, object, Y.Doc, Y.Transaction> _onDocUpdate;
        private readonly Action<AwarenessChanges, object>                 _onAwarenessUpdate;

        private Func<Uint8Array, Task> _send;
        private Task                   _lastSend = Task.FromResult(0);
        private bool                   _connected;
        private bool                   _disposed;
        private uint                   _flushFrame;
        private double                 _flushTimeout;

        /// <summary>
        /// A provider that sends each message as a C# <c>byte[]</c>. Use <see cref="WithUint8ArraySender"/> or
        /// <see cref="WithBase64Sender"/> to skip the copy into a <c>byte[]</c>.
        /// </summary>
        /// <param name="doc">The document to keep in sync.</param>
        /// <param name="awareness">Its presence, or null to sync the document alone.</param>
        /// <param name="send">Delivers one message to the peer. Completes once it is sent.</param>
        public YjsSyncProvider(Y.Doc doc, Awareness awareness, Func<byte[], Task> send) : this(doc, awareness)
        {
            if (send is null) throw new ArgumentNullException(nameof(send));
            _send = message => send(YjsBinary.ToBytes(message));
        }

        private YjsSyncProvider(Y.Doc doc, Awareness awareness)
        {
            if (!Yjs.IsLoaded) throw new InvalidOperationException("Yjs is not loaded yet: await Yjs.LoadAsync() first.");

            _doc               = doc ?? throw new ArgumentNullException(nameof(doc));
            _awareness         = awareness;
            _onDocUpdate       = (update, origin, d, transaction) => OnDocUpdate(update, origin);
            _onAwarenessUpdate = (changes, origin) => OnAwarenessUpdate(changes, origin);

            _doc.onUpdate(_onDocUpdate);
            if (_awareness != null) _awareness.onUpdate(_onAwarenessUpdate);
        }

        /// <summary>A provider that sends each message as the <see cref="Uint8Array"/> Yjs produced, with no copy.</summary>
        public static YjsSyncProvider WithUint8ArraySender(Y.Doc doc, Awareness awareness, Func<Uint8Array, Task> send)
        {
            if (send is null) throw new ArgumentNullException(nameof(send));
            return new YjsSyncProvider(doc, awareness) { _send = send };
        }

        /// <summary>A provider that sends each message base64-encoded, for a text transport (server-sent events, JSON).</summary>
        public static YjsSyncProvider WithBase64Sender(Y.Doc doc, Awareness awareness, Func<string, Task> send)
        {
            if (send is null) throw new ArgumentNullException(nameof(send));
            return new YjsSyncProvider(doc, awareness) { _send = message => send(YjsBinary.ToBase64(message)) };
        }

        public Y.Doc Doc => _doc;

        public Awareness Awareness => _awareness;

        /// <summary>True once the peer has answered sync step 1 since the last <see cref="ConnectAsync"/>.</summary>
        public IObservable<bool> Synced => _synced;

        /// <summary>Whether the peer has answered sync step 1 since the last <see cref="ConnectAsync"/>.</summary>
        public bool IsSynced => _synced.Value;

        /// <summary>Whether messages are being sent: between <see cref="ConnectAsync"/> and <see cref="Disconnect"/>.</summary>
        public bool IsConnected => _connected;

        /// <summary>
        /// Whether local updates made in one burst are merged and sent once per frame (the default) rather than one
        /// message per transaction.
        /// </summary>
        public bool BatchUpdates { get; set; } = true;

        /// <summary>
        /// Whether <see cref="ConnectAsync"/> also asks the peer for every awareness state it knows
        /// (<see cref="MessageQueryAwareness"/>), so presence shows at once rather than at each peer's next renewal.
        /// On by default; a y-websocket server ignores the message and pushes the states itself.
        /// </summary>
        public bool QueryAwarenessOnConnect { get; set; } = true;

        /// <summary>Raised when the transport failed to send a message. <see cref="IsSynced"/> is false from then on.</summary>
        public event Action<Exception> OnSendFailed;

        /// <summary>Raised when the peer refused access (an auth message of type 0), with its reason.</summary>
        public event Action<string> OnPermissionDenied;

        /// <summary>
        /// Starts sending: sync step 1 (asking the peer for what this document is missing), the local awareness state,
        /// and - with <see cref="QueryAwarenessOnConnect"/> - a request for the peer's awareness states. Completes once
        /// those are sent; <see cref="WhenSyncedAsync"/> completes once the peer has answered. Calling it again
        /// (after a reconnect of the transport) resyncs.
        /// </summary>
        public async Task ConnectAsync()
        {
            ThrowIfDisposed();

            _connected = true;
            _synced.Value = false;

            var encoder = YProtocols.encoding.createEncoder();
            YProtocols.encoding.writeVarUint(encoder, MessageSync);
            YProtocols.sync.writeSyncStep1(encoder, _doc);
            Send(YProtocols.encoding.toUint8Array(encoder));

            if (_awareness != null && _awareness.getLocalState() != null)
            {
                Send(EncodeAwareness(new[] { _doc.clientID }));
            }

            if (_awareness != null && QueryAwarenessOnConnect)
            {
                var query = YProtocols.encoding.createEncoder();
                YProtocols.encoding.writeVarUint(query, MessageQueryAwareness);
                Send(YProtocols.encoding.toUint8Array(query));
            }

            await _lastSend;
        }

        /// <summary>Completes once the provider is synced (at once, if it already is).</summary>
        public Task WhenSyncedAsync()
        {
            if (_synced.Value) return Task.FromResult(0);

            var tcs = new TaskCompletionSource<int>();
            ObservableEvent.ValueChanged<bool> handler = null;

            handler = synced =>
            {
                if (!synced) return;
                _synced.StopObserving(handler);
                tcs.TrySetResult(0);
            };

            _synced.ObserveFutureChanges(handler);
            return tcs.Task;
        }

        /// <summary>
        /// Stops sending, for when the transport went away (or to simulate being offline). Local changes keep going
        /// into the document and are exchanged by the next <see cref="ConnectAsync"/>. The other peers' awareness
        /// states are removed locally, since nothing will renew them; the peer drops this client's state the same
        /// way once it stops hearing from it.
        /// </summary>
        public void Disconnect()
        {
            if (_disposed || !_connected) return;

            CancelFlush();
            _pendingUpdates.Clear();
            _connected    = false;
            _synced.Value = false;

            RemoveRemoteAwarenessStates();
        }

        /// <summary>Handles one message from the peer.</summary>
        public void Receive(byte[] message) => Receive(YjsBinary.FromBytes(message));

        /// <summary>Handles one base64-encoded message from the peer.</summary>
        public void ReceiveBase64(string message) => Receive(YjsBinary.FromBase64(message));

        /// <summary>
        /// Handles one message from the peer: applies a sync message or an awareness update (with this provider as the
        /// transaction origin, so it is not sent back), and answers a sync step 1 or an awareness query while
        /// connected. Throws for a message type the protocol does not have. A message that arrives after
        /// <see cref="Dispose"/> (one the transport still had in flight) is ignored.
        /// </summary>
        public void Receive(Uint8Array message)
        {
            if (_disposed) return;

            var decoder     = YProtocols.decoding.createDecoder(message);
            var messageType = YProtocols.decoding.readVarUint(decoder);

            switch (messageType)
            {
                case MessageSync:
                {
                    var encoder = YProtocols.encoding.createEncoder();
                    YProtocols.encoding.writeVarUint(encoder, MessageSync);

                    var syncType = YProtocols.sync.readSyncMessage(decoder, encoder, _doc, this);

                    if (YProtocols.encoding.length(encoder) > 1) Send(YProtocols.encoding.toUint8Array(encoder));
                    if (syncType == YProtocols.sync.messageYjsSyncStep2 && _connected && !_synced.Value) _synced.Value = true;
                    break;
                }

                case MessageAwareness:
                {
                    if (_awareness != null) Awareness.applyAwarenessUpdate(_awareness, YProtocols.decoding.readVarUint8Array(decoder), this);
                    break;
                }

                case MessageQueryAwareness:
                {
                    if (_awareness != null) Send(EncodeAwareness(ClientsOf(_awareness)));
                    break;
                }

                case MessageAuth:
                {
                    if (YProtocols.decoding.readVarUint(decoder) == 0) OnPermissionDenied?.Invoke(YProtocols.decoding.readVarString(decoder));
                    break;
                }

                default: throw new ArgumentException("Unknown Yjs message type " + messageType + ".", nameof(message));
            }
        }

        /// <summary>
        /// Sends what is still waiting to be batched, tells the peer this client left (removes the local awareness
        /// state) and stops listening to the document and the awareness. Neither of them is destroyed: they belong to
        /// the host.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            Flush();

            if (_awareness != null)
            {
                Awareness.removeAwarenessStates(_awareness, new[] { _doc.clientID }, DisposeOrigin);
                _awareness.offUpdate(_onAwarenessUpdate);
                RemoveRemoteAwarenessStates();
            }

            _doc.offUpdate(_onDocUpdate);
            _connected = false;
            _disposed  = true;
            _synced.Value = false;
        }

        private void OnDocUpdate(Uint8Array update, object origin)
        {
            if (origin == (object)this || !_connected) return;

            _pendingUpdates.Add(update);

            if (!BatchUpdates)
            {
                Flush();
                return;
            }

            if (_flushFrame == 0 && _flushTimeout == 0)
            {
                // A frame in a visible tab, and a timeout so a hidden tab (where frames stop) still sends.
                _flushFrame   = window.requestAnimationFrame(_ => Flush());
                _flushTimeout = window.setTimeout(_ => Flush(), 100);
            }
        }

        private void Flush()
        {
            CancelFlush();

            if (_pendingUpdates.Count == 0) return;

            var update = _pendingUpdates.Count == 1 ? _pendingUpdates[0] : Y.mergeUpdates(_pendingUpdates.ToArray());
            _pendingUpdates.Clear();

            var encoder = YProtocols.encoding.createEncoder();
            YProtocols.encoding.writeVarUint(encoder, MessageSync);
            YProtocols.sync.writeUpdate(encoder, update);
            Send(YProtocols.encoding.toUint8Array(encoder));
        }

        private void CancelFlush()
        {
            if (_flushFrame != 0) window.cancelAnimationFrame(_flushFrame);
            if (_flushTimeout != 0) window.clearTimeout(_flushTimeout);

            _flushFrame   = 0;
            _flushTimeout = 0;
        }

        private void OnAwarenessUpdate(AwarenessChanges changes, object origin)
        {
            // What this provider applied came from the peer; everything else - the local state, a removal - goes out.
            if (origin == (object)this) return;

            var clients = new List<int>();
            for (var i = 0; i < changes.added.Length;   i++) clients.Add(changes.added[i]);
            for (var i = 0; i < changes.updated.Length; i++) clients.Add(changes.updated[i]);
            for (var i = 0; i < changes.removed.Length; i++) clients.Add(changes.removed[i]);

            Send(EncodeAwareness(clients.ToArray()));
        }

        private Uint8Array EncodeAwareness(int[] clients)
        {
            var encoder = YProtocols.encoding.createEncoder();
            YProtocols.encoding.writeVarUint(encoder, MessageAwareness);
            YProtocols.encoding.writeVarUint8Array(encoder, Awareness.encodeAwarenessUpdate(_awareness, clients));
            return YProtocols.encoding.toUint8Array(encoder);
        }

        private static int[] ClientsOf(Awareness awareness)
        {
            var clients = new List<int>();
            awareness.getStates().forEach((state, client, map) => clients.Add(client));
            return clients.ToArray();
        }

        private void RemoveRemoteAwarenessStates()
        {
            var remote = new List<int>();

            foreach (var client in ClientsOf(_awareness))
            {
                if (client != _doc.clientID) remote.Add(client);
            }

            if (remote.Count > 0) Awareness.removeAwarenessStates(_awareness, remote.ToArray(), this);
        }

        private void Send(Uint8Array message)
        {
            if (!_connected || _disposed) return;

            _lastSend = SendAfterAsync(_lastSend, message);
        }

        private async Task SendAfterAsync(Task previous, Uint8Array message)
        {
            // The previous send reported its own failure; it only decides when this one may start.
            await previous;

            try
            {
                await _send(message);
            }
            catch (Exception e)
            {
                _synced.Value = false;

                if (OnSendFailed != null) OnSendFailed(e);
                else console.error("YjsSyncProvider: sending a message failed; the peer may be missing an update until the next ConnectAsync.", e);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(YjsSyncProvider));
        }
    }
}
