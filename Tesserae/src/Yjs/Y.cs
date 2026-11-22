using System;
using Transpose;
using static Transpose.Core.es5;

namespace Tesserae
{
    /// <summary>
    /// The global <c>Y</c> object of <see href="https://docs.yjs.dev">Yjs</see>, declared to the compiler rather than
    /// reached through script strings. Every member is an <c>[External]</c> declaration: nothing is emitted for it, a
    /// call site compiles straight to the JavaScript it names, and <c>[Convention(Notation.None)]</c> keeps the C#
    /// names identical to Yjs' own, so its documentation reads straight across.
    /// </summary>
    /// <remarks>
    /// <para>Nothing here exists until <see cref="Yjs.LoadAsync"/> has completed: the library is not part of the
    /// bundle a page starts with.</para>
    /// <para>Binary data (updates, state vectors, snapshots) is a JavaScript <see cref="Uint8Array"/>, which is what
    /// Yjs reads and writes; <see cref="YjsBinary"/> converts to and from <c>byte[]</c> and base64.</para>
    /// <para>JavaScript event registration (<c>doc.on('update', f)</c>) is declared once per event, as
    /// <c>onUpdate</c> / <c>offUpdate</c> and so on, each typed with the arguments that event passes. Two declarations
    /// of one JavaScript name on one type would get a <c>$1</c> suffix from the compiler and stop matching Yjs, which
    /// is also why nothing here is overloaded: a variant with another argument list has its own C# name and a
    /// <c>[Template]</c> naming the JavaScript call.</para>
    /// <para>Pass a handler that is kept in a field to both <c>on…</c> and <c>off…</c>: Yjs removes a listener by
    /// reference, and a lambda written twice is two functions.</para>
    /// </remarks>
    [External]
    [Convention(Notation.None)]
    [Name("Y")]
    public static class Y
    {
        /// <summary>A Yjs document: the unit that is synchronised, holding named shared types.</summary>
        [External]
        [Convention(Notation.None)]
        [Name("Y.Doc")]
        public class Doc
        {
            public extern Doc();

            public extern Doc(DocOptions options);

            /// <summary>This peer's id in the document. Random per <see cref="Doc"/>.</summary>
            public extern int clientID { get; set; }

            public extern string guid { get; }

            /// <summary>Whether deleted content is garbage-collected. Off is needed for snapshots of the past.</summary>
            public extern bool gc { get; set; }

            public extern bool isLoaded { get; }

            /// <summary>The top-level text of this name, created on first use.</summary>
            public extern Text getText(string name);

            /// <summary>The top-level array of this name, created on first use.</summary>
            [IgnoreGeneric]
            public extern Array<T> getArray<T>(string name);

            /// <summary>The top-level map of this name, created on first use.</summary>
            [IgnoreGeneric]
            public extern Map<T> getMap<T>(string name);

            /// <summary>The top-level XML fragment of this name, created on first use.</summary>
            public extern XmlFragment getXmlFragment(string name);

            /// <summary>Runs <paramref name="f"/> as one transaction: observers fire once, and one update is emitted.</summary>
            public extern void transact(Action<Transaction> f, object origin);

            /// <summary>The whole document as JSON, one key per top-level type.</summary>
            public extern object toJSON();

            public extern void destroy();

            /// <summary><c>doc.on('update', …)</c>: an update (V1 encoding) was applied, with the transaction's origin.</summary>
            [Template("{this}.on('update', {handler})")]
            public extern void onUpdate(Action<Uint8Array, object, Doc, Transaction> handler);

            [Template("{this}.off('update', {handler})")]
            public extern void offUpdate(Action<Uint8Array, object, Doc, Transaction> handler);

            /// <summary><c>doc.on('updateV2', …)</c>: the same as <see cref="onUpdate"/>, in the V2 encoding.</summary>
            [Template("{this}.on('updateV2', {handler})")]
            public extern void onUpdateV2(Action<Uint8Array, object, Doc, Transaction> handler);

            [Template("{this}.off('updateV2', {handler})")]
            public extern void offUpdateV2(Action<Uint8Array, object, Doc, Transaction> handler);

            [Template("{this}.on('beforeTransaction', {handler})")]
            public extern void onBeforeTransaction(Action<Transaction, Doc> handler);

            [Template("{this}.off('beforeTransaction', {handler})")]
            public extern void offBeforeTransaction(Action<Transaction, Doc> handler);

            [Template("{this}.on('afterTransaction', {handler})")]
            public extern void onAfterTransaction(Action<Transaction, Doc> handler);

            [Template("{this}.off('afterTransaction', {handler})")]
            public extern void offAfterTransaction(Action<Transaction, Doc> handler);

            /// <summary><c>doc.on('beforeAllTransactions', …)</c>: before the first of a batch of nested transactions - the moment to save positions an edit is about to move.</summary>
            [Template("{this}.on('beforeAllTransactions', {handler})")]
            public extern void onBeforeAllTransactions(Action<Doc> handler);

            [Template("{this}.off('beforeAllTransactions', {handler})")]
            public extern void offBeforeAllTransactions(Action<Doc> handler);

            /// <summary><c>doc.on('afterAllTransactions', …)</c>: after the last of a batch of nested transactions.</summary>
            [Template("{this}.on('afterAllTransactions', {handler})")]
            public extern void onAfterAllTransactions(Action<Doc, ReadOnlyArray<Transaction>> handler);

            [Template("{this}.off('afterAllTransactions', {handler})")]
            public extern void offAfterAllTransactions(Action<Doc, ReadOnlyArray<Transaction>> handler);

            [Template("{this}.on('destroy', {handler})")]
            public extern void onDestroy(Action<Doc> handler);

            [Template("{this}.off('destroy', {handler})")]
            public extern void offDestroy(Action<Doc> handler);
        }

        /// <summary>The options of <c>new Y.Doc(…)</c>. Only the fields that are set are passed.</summary>
        [ObjectLiteral]
        public class DocOptions
        {
            public string guid;
            public bool   gc;
            public bool   autoLoad;
            public object meta;
        }

        /// <summary>A change to the document in progress, handed to observers and to <see cref="Doc.transact"/>.</summary>
        [External]
        [Convention(Notation.None)]
        public interface Transaction
        {
            Doc    doc    { get; }
            object origin { get; }

            /// <summary>Whether the change was made on this peer rather than applied from an update.</summary>
            bool local { get; }
        }

        /// <summary>What every shared type has: its document, its parent, and deep observation.</summary>
        [External]
        [Convention(Notation.None)]
        [Name("Y.AbstractType")]
        public abstract class AbstractType
        {
            /// <summary>The document the type is in, or null while it is not integrated into one.</summary>
            public extern Doc doc { get; }

            public extern AbstractType parent { get; }

            public extern object toJSON();

            /// <summary>Called after every transaction that changed this type or anything below it.</summary>
            public extern void observeDeep(Action<ReadOnlyArray<YEvent>, Transaction> f);

            public extern void unobserveDeep(Action<ReadOnlyArray<YEvent>, Transaction> f);
        }

        /// <summary>Shared text, with optional formatting attributes - what an editor binds to.</summary>
        [External]
        [Convention(Notation.None)]
        [Name("Y.Text")]
        public class Text : AbstractType
        {
            /// <summary>A text that is not in a document yet; it becomes shared when inserted into one.</summary>
            public extern Text(string initial);

            /// <summary>The length in UTF-16 code units, the same unit as a JavaScript (and so a C#) string index.</summary>
            public extern int length { get; }

            /// <summary>The plain text. <c>toString()</c> in JavaScript.</summary>
            [Template("{this}.toString()")]
            public extern string asString();

            /// <param name="attributes">Formatting for the inserted text, or null to continue the formatting at <paramref name="index"/>.</param>
            public extern void insert(int index, string text, object attributes);

            /// <summary>Inserts with the formatting found at <paramref name="index"/>.</summary>
            [Template("{this}.insert({index}, {text})")]
            public extern void insertText(int index, string text);

            public extern void @delete(int index, int length);

            public extern void format(int index, int length, object attributes);

            /// <summary>The content as a delta: runs of <c>insert</c>, each with its attributes.</summary>
            public extern ReadOnlyArray<Delta> toDelta();

            /// <summary>Applies a Quill-style delta (<c>retain</c> / <c>insert</c> / <c>delete</c>).</summary>
            public extern void applyDelta(ReadOnlyArray<Delta> delta);

            /// <summary>Called after every transaction that changed this text.</summary>
            public extern void observe(Action<TextEvent, Transaction> f);

            public extern void unobserve(Action<TextEvent, Transaction> f);

            public extern Text clone();
        }

        /// <summary>Shared array.</summary>
        [External]
        [Convention(Notation.None)]
        [Name("Y.Array")]
        [IgnoreGeneric]
        public class Array<T> : AbstractType
        {
            public extern Array();

            public extern int length { get; }

            /// <summary>Inserts the items at <paramref name="index"/>. A C# array crosses as the JavaScript array it is.</summary>
            public extern void insert(int index, ReadOnlyArray<T> content);

            public extern void push(ReadOnlyArray<T> content);

            public extern void unshift(ReadOnlyArray<T> content);

            public extern void @delete(int index, int length);

            public extern T get(int index);

            public extern ReadOnlyArray<T> slice(int start, int end);

            public extern ReadOnlyArray<T> toArray();

            public extern void forEach(Action<T, int> f);

            public extern void observe(Action<ArrayEvent, Transaction> f);

            public extern void unobserve(Action<ArrayEvent, Transaction> f);
        }

        /// <summary>Shared map with string keys.</summary>
        [External]
        [Convention(Notation.None)]
        [Name("Y.Map")]
        [IgnoreGeneric]
        public class Map<T> : AbstractType
        {
            public extern Map();

            public extern int size { get; }

            public extern T get(string key);

            public extern T set(string key, T value);

            public extern bool has(string key);

            public extern void @delete(string key);

            public extern void clear();

            /// <summary>Calls <paramref name="f"/> with each value and its key.</summary>
            public extern void forEach(Action<T, string> f);

            /// <summary>The keys. <c>Array.from(map.keys())</c> in JavaScript.</summary>
            [Template("Array.from({this}.keys())")]
            public extern string[] keysAsArray();

            public extern void observe(Action<MapEvent, Transaction> f);

            public extern void unobserve(Action<MapEvent, Transaction> f);
        }

        /// <summary>Shared XML tree root, what rich-text editors (ProseMirror, Tiptap) bind to.</summary>
        [External]
        [Convention(Notation.None)]
        [Name("Y.XmlFragment")]
        public class XmlFragment : AbstractType
        {
            public extern XmlFragment();

            public extern int length { get; }

            public extern AbstractType firstChild { get; }

            /// <summary>Inserts elements or texts at <paramref name="index"/>.</summary>
            public extern void insert(int index, ReadOnlyArray<AbstractType> content);

            public extern void push(ReadOnlyArray<AbstractType> content);

            public extern void @delete(int index, int length);

            public extern AbstractType get(int index);

            public extern ReadOnlyArray<AbstractType> toArray();

            /// <summary>The fragment as XML markup. <c>toString()</c> in JavaScript.</summary>
            [Template("{this}.toString()")]
            public extern string asString();

            public extern void observe(Action<XmlEvent, Transaction> f);

            public extern void unobserve(Action<XmlEvent, Transaction> f);
        }

        /// <summary>An element of an XML tree: a node name, attributes and children.</summary>
        [External]
        [Convention(Notation.None)]
        [Name("Y.XmlElement")]
        public class XmlElement : XmlFragment
        {
            public extern XmlElement(string nodeName);

            public extern string nodeName { get; }

            public extern void setAttribute(string name, string value);

            public extern string getAttribute(string name);

            public extern void removeAttribute(string name);

            public extern object getAttributes();
        }

        /// <summary>Text inside an XML tree.</summary>
        [External]
        [Convention(Notation.None)]
        [Name("Y.XmlText")]
        public class XmlText : Text
        {
            public extern XmlText(string initial);
        }

        /// <summary>One run of a delta: an insert (text or embed), a retain or a delete, with its attributes.</summary>
        [ObjectLiteral]
        public class Delta
        {
            /// <summary>The inserted text, or the embed (any value) for a non-text insert. Undefined when this is no insert.</summary>
            public object insert;

            /// <summary>How many units to skip (keeping or formatting them). Undefined when this is no retain.</summary>
            public int? retain;

            /// <summary>How many units to delete. Undefined when this is no delete.</summary>
            public int? @delete;

            public object attributes;
        }

        /// <summary>What changed, as reported to an observer after a transaction.</summary>
        [External]
        [Convention(Notation.None)]
        public interface YEvent
        {
            /// <summary>The type that changed.</summary>
            AbstractType target { get; }

            /// <summary>The type the observer was registered on (differs from <see cref="target"/> in deep observers).</summary>
            AbstractType currentTarget { get; }

            Transaction transaction { get; }

            /// <summary>The keys and indexes from <see cref="currentTarget"/> down to <see cref="target"/>.</summary>
            ReadOnlyArray<object> path { get; }

            /// <summary>The change as a delta - for text, runs of <c>retain</c> / <c>insert</c> / <c>delete</c>.</summary>
            ReadOnlyArray<Delta> delta { get; }
        }

        [External]
        [Convention(Notation.None)]
        public interface TextEvent : YEvent
        {
            new Text target { get; }
        }

        [External]
        [Convention(Notation.None)]
        public interface ArrayEvent : YEvent
        {
        }

        [External]
        [Convention(Notation.None)]
        public interface MapEvent : YEvent
        {
            /// <summary>The keys that changed, as an array. <c>Array.from(event.keysChanged)</c> in JavaScript.</summary>
            [Template("Array.from({this}.keysChanged)")]
            string[] keysChangedAsArray();
        }

        [External]
        [Convention(Notation.None)]
        public interface XmlEvent : YEvent
        {
            /// <summary>Whether the children changed (rather than only attributes).</summary>
            bool childListChanged { get; }

            /// <summary>The attribute names that changed, as an array.</summary>
            [Template("Array.from({this}.attributesChanged)")]
            string[] attributesChangedAsArray();
        }

        /// <summary>
        /// Undo and redo over one or more shared types, limited to the transactions whose origin it tracks - which is
        /// what keeps Ctrl+Z to the edits this user made rather than the last edit anyone made.
        /// </summary>
        [External]
        [Convention(Notation.None)]
        [Name("Y.UndoManager")]
        public class UndoManager
        {
            public extern UndoManager(AbstractType scope);

            public extern UndoManager(AbstractType scope, UndoManagerOptions options);

            /// <summary>Only transactions with an origin in this set are captured. Starts as <c>{ null }</c>.</summary>
            public extern void addTrackedOrigin(object origin);

            public extern void removeTrackedOrigin(object origin);

            public extern void addToScope(AbstractType type);

            /// <summary>Undoes the last captured change. Returns the stack item, or null when there was nothing to undo.</summary>
            public extern object undo();

            public extern object redo();

            public extern bool canUndo();

            public extern bool canRedo();

            /// <summary>Makes the next change start a new undo step instead of merging into the current one.</summary>
            public extern void stopCapturing();

            public extern void clear();

            public extern void destroy();

            /// <summary>Changes made within this many milliseconds of each other undo as one step. Defaults to 500.</summary>
            public extern int captureTimeout { get; set; }

            public extern bool undoing { get; }

            public extern bool redoing { get; }

            /// <summary><c>on('stack-item-added', …)</c>, e.g. to store a selection with the step.</summary>
            [Template("{this}.on('stack-item-added', {handler})")]
            public extern void onStackItemAdded(Action<UndoStackEvent, UndoManager> handler);

            [Template("{this}.off('stack-item-added', {handler})")]
            public extern void offStackItemAdded(Action<UndoStackEvent, UndoManager> handler);

            /// <summary><c>on('stack-item-popped', …)</c>, e.g. to restore the selection stored with the step.</summary>
            [Template("{this}.on('stack-item-popped', {handler})")]
            public extern void onStackItemPopped(Action<UndoStackEvent, UndoManager> handler);

            [Template("{this}.off('stack-item-popped', {handler})")]
            public extern void offStackItemPopped(Action<UndoStackEvent, UndoManager> handler);
        }

        /// <summary>The options of <c>new Y.UndoManager(…)</c>.</summary>
        [ObjectLiteral]
        public class UndoManagerOptions
        {
            public int captureTimeout;

            /// <summary>Whether a remote change to a map key the user also set should be ignored when undoing.</summary>
            public bool ignoreRemoteMapChanges;
        }

        [External]
        [Convention(Notation.None)]
        public interface UndoStackEvent
        {
            /// <summary>The step that was added or popped.</summary>
            StackItem stackItem { get; }

            object origin { get; }

            /// <summary><c>"undo"</c> or <c>"redo"</c>.</summary>
            string type { get; }
        }

        /// <summary>One undo or redo step of an <see cref="UndoManager"/>.</summary>
        [External]
        [Convention(Notation.None)]
        public interface StackItem
        {
            /// <summary>A map the host may keep its own data in, such as the selection to restore when the step is undone.</summary>
            Transpose.Core.es5.Map<string, object> meta { get; }
        }

        /// <summary>
        /// A position that survives concurrent edits: it is anchored to an item in the document rather than to an
        /// index. Opaque; turn it into an index with <see cref="createAbsolutePositionFromRelativePosition"/>.
        /// </summary>
        [External]
        [Convention(Notation.None)]
        public interface RelativePosition
        {
        }

        /// <summary>A <see cref="RelativePosition"/> resolved against a document.</summary>
        [External]
        [Convention(Notation.None)]
        public interface AbsolutePosition
        {
            AbstractType type  { get; }
            int          index { get; }
            int          assoc { get; }
        }

        /// <summary>The state of a document at one moment: what it contained, without the content itself.</summary>
        [External]
        [Convention(Notation.None)]
        public interface Snapshot
        {
        }

        /// <param name="assoc">Which side the position sticks to: 0 or more to the character after it, negative to the one before.</param>
        public static extern RelativePosition createRelativePositionFromTypeIndex(AbstractType type, int index, int assoc);

        /// <summary>Resolves <paramref name="rpos"/>, or returns null when what it was anchored to is not in <paramref name="doc"/>.</summary>
        public static extern AbsolutePosition createAbsolutePositionFromRelativePosition(RelativePosition rpos, Doc doc);

        public static extern Uint8Array encodeRelativePosition(RelativePosition rpos);

        public static extern RelativePosition decodeRelativePosition(Uint8Array encoded);

        /// <summary>A plain JSON object for the position - the shape y-monaco and the other bindings put in awareness.</summary>
        public static extern object relativePositionToJSON(RelativePosition rpos);

        public static extern RelativePosition createRelativePositionFromJSON(object json);

        public static extern bool compareRelativePositions(RelativePosition a, RelativePosition b);

        /// <param name="transactionOrigin">The <see cref="Transaction.origin"/> the update is applied with, which is how a provider recognises (and does not echo) what it applied.</param>
        public static extern void applyUpdate(Doc doc, Uint8Array update, object transactionOrigin);

        public static extern void applyUpdateV2(Doc doc, Uint8Array update, object transactionOrigin);

        /// <summary>The whole document as one update.</summary>
        public static extern Uint8Array encodeStateAsUpdate(Doc doc);

        /// <summary>What <paramref name="doc"/> has that a peer with <paramref name="encodedTargetStateVector"/> does not.</summary>
        [Template("Y.encodeStateAsUpdate({doc}, {encodedTargetStateVector})")]
        public static extern Uint8Array encodeStateAsUpdateSince(Doc doc, Uint8Array encodedTargetStateVector);

        public static extern Uint8Array encodeStateAsUpdateV2(Doc doc);

        [Template("Y.encodeStateAsUpdateV2({doc}, {encodedTargetStateVector})")]
        public static extern Uint8Array encodeStateAsUpdateV2Since(Doc doc, Uint8Array encodedTargetStateVector);

        /// <summary>Which changes of each client the document has - what a peer sends to be told what it is missing.</summary>
        public static extern Uint8Array encodeStateVector(Doc doc);

        public static extern Uint8Array encodeStateVectorFromUpdate(Uint8Array update);

        public static extern Uint8Array encodeStateVectorFromUpdateV2(Uint8Array update);

        /// <summary>Merges several updates into one, without a document.</summary>
        public static extern Uint8Array mergeUpdates(ReadOnlyArray<Uint8Array> updates);

        public static extern Uint8Array mergeUpdatesV2(ReadOnlyArray<Uint8Array> updates);

        /// <summary>The part of <paramref name="update"/> a peer with <paramref name="stateVector"/> does not have.</summary>
        public static extern Uint8Array diffUpdate(Uint8Array update, Uint8Array stateVector);

        public static extern Uint8Array diffUpdateV2(Uint8Array update, Uint8Array stateVector);

        public static extern Uint8Array convertUpdateFormatV1ToV2(Uint8Array update);

        public static extern Uint8Array convertUpdateFormatV2ToV1(Uint8Array update);

        /// <summary>The state of <paramref name="doc"/> now. Requires <see cref="Doc.gc"/> off to be restorable later.</summary>
        public static extern Snapshot snapshot(Doc doc);

        public static extern Uint8Array encodeSnapshot(Snapshot snapshot);

        public static extern Snapshot decodeSnapshot(Uint8Array encoded);

        public static extern Uint8Array encodeSnapshotV2(Snapshot snapshot);

        public static extern Snapshot decodeSnapshotV2(Uint8Array encoded);

        public static extern bool equalSnapshots(Snapshot a, Snapshot b);

        /// <summary>A new document holding <paramref name="originDoc"/> as it was at <paramref name="snapshot"/>.</summary>
        public static extern Doc createDocFromSnapshot(Doc originDoc, Snapshot snapshot);

        public static extern Snapshot emptySnapshot();
    }
}
