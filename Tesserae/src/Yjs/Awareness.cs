using System;
using Transpose;
using static Transpose.Core.es5;

namespace Tesserae
{
    /// <summary>
    /// The presence of the peers editing a document (y-protocols' <c>Awareness</c>): each client's own small JSON
    /// state - by convention <c>{ user: { name, color }, cursor or selection }</c> - kept apart from the document and
    /// never persisted. A state nobody has renewed for 30 seconds is dropped; the local one is renewed every 15.
    /// </summary>
    /// <remarks>Declared, not emitted: see <see cref="Y"/> for the rules. Exists once <see cref="Yjs.LoadAsync"/> has completed.</remarks>
    [External]
    [Convention(Notation.None)]
    [Name("YProtocols.awareness.Awareness")]
    public class Awareness
    {
        public extern Awareness(Y.Doc doc);

        public extern Y.Doc doc { get; }

        /// <summary>The local client's id - the document's <see cref="Y.Doc.clientID"/>.</summary>
        public extern int clientID { get; }

        /// <summary>The local state, or null once it has been removed (which tells the other peers this one left).</summary>
        public extern AwarenessState getLocalState();

        /// <summary>Replaces the local state. Null removes it.</summary>
        public extern void setLocalState(object state);

        /// <summary>Sets one field of the local state, e.g. <c>setLocalStateField("user", new YjsUser { … })</c>.</summary>
        public extern void setLocalStateField(string field, object value);

        /// <summary>Every known client's state, by client id - the local one included.</summary>
        public extern Map<int, AwarenessState> getStates();

        /// <summary>Stops the timer that renews the local state and drops stale ones.</summary>
        public extern void destroy();

        /// <summary><c>on('change', …)</c>: a state was added, removed or changed in content (not merely renewed).</summary>
        [Template("{this}.on('change', {handler})")]
        public extern void onChange(Action<AwarenessChanges, object> handler);

        [Template("{this}.off('change', {handler})")]
        public extern void offChange(Action<AwarenessChanges, object> handler);

        /// <summary><c>on('update', …)</c>: a state was added, renewed, changed or removed - what a provider sends on.</summary>
        [Template("{this}.on('update', {handler})")]
        public extern void onUpdate(Action<AwarenessChanges, object> handler);

        [Template("{this}.off('update', {handler})")]
        public extern void offUpdate(Action<AwarenessChanges, object> handler);

        /// <summary>Encodes the states of <paramref name="clients"/> as an awareness update.</summary>
        [Template("YProtocols.awareness.encodeAwarenessUpdate({awareness}, {clients})")]
        public static extern Uint8Array encodeAwarenessUpdate(Awareness awareness, ReadOnlyArray<int> clients);

        /// <summary>Applies an awareness update received from a peer, with the origin the change event reports.</summary>
        [Template("YProtocols.awareness.applyAwarenessUpdate({awareness}, {update}, {origin})")]
        public static extern void applyAwarenessUpdate(Awareness awareness, Uint8Array update, object origin);

        /// <summary>Marks <paramref name="clients"/> as gone, which an update then tells the other peers.</summary>
        [Template("YProtocols.awareness.removeAwarenessStates({awareness}, {clients}, {origin})")]
        public static extern void removeAwarenessStates(Awareness awareness, ReadOnlyArray<int> clients, object origin);

        /// <summary>Rewrites every state in an encoded update, e.g. to strip a field before relaying it.</summary>
        [Template("YProtocols.awareness.modifyAwarenessUpdate({update}, {modify})")]
        public static extern Uint8Array modifyAwarenessUpdate(Uint8Array update, Func<object, object> modify);
    }

    /// <summary>
    /// One client's awareness state: a plain JSON object. <see cref="user"/> is the field every Yjs binding reads; any
    /// other field (a binding's <c>selection</c> or <c>cursor</c>) is read with <see cref="field"/>.
    /// </summary>
    [External]
    [Convention(Notation.None)]
    public interface AwarenessState
    {
        /// <summary>The <c>user</c> field, or null when the client has not set one.</summary>
        YjsUser user { get; }

        /// <summary>Any field of the state, or null/undefined when it is not set.</summary>
        [Template("{this}[{name}]")]
        object field(string name);
    }

    /// <summary>The client ids an awareness event is about.</summary>
    [External]
    [Convention(Notation.None)]
    public interface AwarenessChanges
    {
        ReadOnlyArray<int> added   { get; }
        ReadOnlyArray<int> updated { get; }
        ReadOnlyArray<int> removed { get; }
    }

    /// <summary>
    /// The <c>user</c> field of an awareness state, in the shape the Yjs editor bindings (y-monaco, y-codemirror,
    /// y-prosemirror) read: <c>awareness.setLocalStateField("user", new YjsUser { name = …, color = … })</c>.
    /// </summary>
    [ObjectLiteral]
    public class YjsUser
    {
        public string name;

        /// <summary>A CSS colour for the user's caret and name label.</summary>
        public string color;

        /// <summary>A translucent version of <see cref="color"/> for the user's selection.</summary>
        public string colorLight;
    }
}
