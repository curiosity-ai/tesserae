using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Transpose.Core.es5;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Utilities, Order = 70, Icon = UIcons.UsersAlt, Description = "Real-time collaborative editing with Yjs")]
    public class YjsSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public YjsSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(YjsSample), UIcons.UsersAlt, "Shared documents that converge, with Yjs")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Yjs is a CRDT: every peer edits its own copy of a document, and copies that have seen the same changes hold the same content, whatever order the changes arrived in. Tesserae loads it on demand (`await Yjs.LoadAsync()`, a separate script that is not part of the page's bundle) and declares it as `Y` (`Y.Doc`, `Y.Text`, `Y.Array`, `Y.Map`, `Y.UndoManager`, relative positions, updates and snapshots) and `Awareness` for presence."),
                        TextBlock("`YjsSyncProvider` speaks the y-websocket message protocol over any transport: it hands each outgoing message to a callback, takes incoming ones through `Receive`, sends sync step 1 and the local presence on `ConnectAsync`, and broadcasts its departure on `Dispose`. `TextArea.BindYjs(text)` binds a plain field to a shared text.")
                    )).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleDo("Set the awareness `user` field with `YjsUserColors.UserFor(name)`, so every peer shows a person in the same colour."),
                        SampleDo("Call `ConnectAsync` again after the transport reconnects: the step 1 / step 2 exchange carries what either side missed."),
                        SampleDo("Have the peer send its own sync step 1 when a client connects, as the y-websocket server does, so it learns what the client has."),
                        SampleDont("Load a second copy of Yjs on the page: its type checks fail between the two copies.")
                    )).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(Defer(async () =>
                    {
                        await Yjs.LoadAsync();
                        return new TwoPeers().Content;
                    }, TextBlock("Loading Yjs…"))).SetTitle("Two documents, one page")));
        }

        public HTMLElement Render() => _content.Render();

        /// <summary>Two documents on one page, each with its own provider, connected through an in-memory transport.</summary>
        private sealed class TwoPeers
        {
            private readonly Peer        _ada;
            private readonly Peer        _grace;
            private readonly SampleRandom _random = new SampleRandom(1307);
            private readonly TextBlock   _convergence;

            private int  _latency = 200;
            private bool _offline;

            public IComponent Content { get; }

            public TwoPeers()
            {
                _ada   = new Peer("Ada",   this);
                _grace = new Peer("Grace", this);

                _ada.Connect(_grace);
                _grace.Connect(_ada);

                _convergence = TextBlock().Small();

                var latencyLabel = TextBlock($"Latency: {_latency} ms").Small();

                var latency = Slider(val: _latency, min: 0, max: 1500, step: 50).W(200).OnInput((s, e) =>
                {
                    _latency = s.Value;
                    latencyLabel.Text = $"Latency: {_latency} ms";
                });

                var offline = Toggle("Offline").OnChange((t, e) => SetOfflineAsync(t.IsChecked).FireAndForget());

                var concurrent = Button("Concurrent edits").SetIcon(UIcons.Shuffle).OnClick(() =>
                {
                    // Both peers edit at once, before either has heard of the other's change.
                    _ada.RandomEdit(_random);
                    _grace.RandomEdit(_random);
                });

                _ada.Text.observe((e, tr) => UpdateConvergence());
                _grace.Text.observe((e, tr) => UpdateConvergence());

                _ada.Doc.transact(_ => _ada.Text.insert(0, "Type in either box: the other one follows. Turn on Offline, edit both, then turn it off again.", null), "seed");

                Content = VStack().WS().Children(
                    HStack().WS().AlignItemsCenter().Wrap().Gap(16.px()).Children(latencyLabel, latency, offline, concurrent),
                    HStack().WS().Wrap().Gap(16.px()).Children(_ada.Content.Grow().W(10), _grace.Content.Grow().W(10)),
                    _convergence.PT(8))
                   .WhenRemoved(() =>
                    {
                        _ada.Dispose();
                        _grace.Dispose();
                    });

                ConnectAsync().FireAndForget();
            }

            internal int Latency => _latency;

            internal bool Offline => _offline;

            private async Task ConnectAsync()
            {
                await _ada.Provider.ConnectAsync();
                await _grace.Provider.ConnectAsync();
            }

            private async Task SetOfflineAsync(bool offline)
            {
                _offline = offline;

                if (offline)
                {
                    _ada.Provider.Disconnect();
                    _grace.Provider.Disconnect();
                }
                else
                {
                    await ConnectAsync();
                }

                UpdateConvergence();
            }

            private void UpdateConvergence()
            {
                var same = _ada.Text.asString() == _grace.Text.asString();

                _convergence.Text = same
                    ? "Both documents hold the same text."
                    : _offline ? "The documents differ: they are offline." : "The documents differ: updates are in flight.";

                _convergence.Foreground(same ? Theme.Success.Background : Theme.Secondary.Foreground);
            }
        }

        /// <summary>One peer: its document, presence, provider, bound field and presence list.</summary>
        private sealed class Peer : IDisposable
        {
            private readonly TwoPeers        _owner;
            private readonly YjsInputBinding _binding;
            private readonly Stack           _presence;
            private readonly Badge           _synced;
            private readonly Action<AwarenessChanges, object> _onPresence;

            public Y.Doc            Doc       { get; }
            public Y.Text           Text      { get; }
            public Awareness        Awareness { get; }
            public YjsSyncProvider  Provider  { get; private set; }
            public IComponent       Content   { get; }

            public Peer(string name, TwoPeers owner)
            {
                _owner    = owner;
                Doc       = new Y.Doc();
                Text      = Doc.getText("content");
                Awareness = new Awareness(Doc);
                Awareness.setLocalStateField("user", YjsUserColors.UserFor(name));

                var field = TextArea().WS().H(140);
                _binding  = field.BindYjs(Text);
                _presence = HStack().Wrap().Gap(8.px()).AlignItemsCenter();
                _synced   = Badge("Connecting");

                _onPresence = (changes, origin) => RenderPresence();
                Awareness.onChange(_onPresence);

                Content = VStack().Gap(8.px()).Children(
                    HStack().AlignItemsCenter().Gap(8.px()).Children(
                        Raw(Swatch(YjsUserColors.ColorFor(name))),
                        TextBlock(name + "'s document").SemiBold(),
                        _synced),
                    field,
                    _presence);

                RenderPresence();
            }

            public void Connect(Peer other)
            {
                Provider = YjsSyncProvider.WithUint8ArraySender(Doc, Awareness, message =>
                {
                    // An in-memory network: delivered after the latency unless the link is down by then.
                    window.setTimeout(_ =>
                    {
                        if (!_owner.Offline) other.Provider.Receive(message);
                    }, _owner.Latency);

                    return Task.FromResult(0);
                });

                Provider.Synced.Observe(synced => _synced.SetText(synced ? "Synced" : (_owner.Offline ? "Offline" : "Connecting")));
            }

            public void RandomEdit(SampleRandom random)
            {
                var words = new[] { " apple", " pear", " plum", " fig", " kiwi" };
                var at    = random.Next(Text.length + 1);
                Doc.transact(_ => Text.insert(at, words[random.Next(words.Length)], null), null);
            }

            private void RenderPresence()
            {
                _presence.Clear();
                _presence.Add(TextBlock("Here:").Small().Secondary());

                var states = new List<(int client, YjsUser user)>();

                Awareness.getStates().forEach((state, client, map) =>
                {
                    if (state.user != null) states.Add((client, state.user));
                });

                foreach (var (client, user) in states)
                {
                    var label = client == Doc.clientID ? user.name + " (you)" : user.name;
                    _presence.Add(HStack().AlignItemsCenter().Gap(4.px()).Children(Raw(Swatch(user.color)), TextBlock(label).Small()));
                }
            }

            private static HTMLElement Swatch(string color)
            {
                var swatch = Div(Att(styles: s =>
                {
                    s.width        = "10px";
                    s.height       = "10px";
                    s.borderRadius = "50%";
                    s.background   = color;
                }));

                return swatch;
            }

            public void Dispose()
            {
                Awareness.offChange(_onPresence);
                _binding.Dispose();
                Provider.Dispose();
                Awareness.destroy();
                Doc.destroy();
            }
        }
    }
}
