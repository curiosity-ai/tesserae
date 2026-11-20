using System;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;
using Range = Transpose.Core.dom.Range;

namespace Tesserae
{
    /// <summary>
    /// Loads <see href="https://github.com/fastmail/Squire">Squire</see>, the rich text editor behind
    /// <see cref="SquireEditor"/>. Squire is not part of the bundle a page starts with: it ships as its own file
    /// (<c>tss-squire.min.js</c>, the unmodified <c>dist/squire.js</c> of the <c>squire-rte</c> npm package) and is
    /// fetched the first time an editor mounts or something calls <see cref="LoadAsync"/>.
    /// </summary>
    public static class SquireLibrary
    {
        /// <summary>Where the compiler puts the unlinked <c>tss-squire.min.js</c> resource of <c>tps.json</c>.</summary>
        public const string ScriptUrl = "assets/js/tss-squire.min.js";

        private static Task _loading;

        /// <summary>Whether Squire is on the page.</summary>
        public static bool IsLoaded { get; private set; }

        /// <summary>Loads the library once. Every caller shares the one fetch, and a failed load is forgotten so the next call tries again.</summary>
        public static async Task LoadAsync()
        {
            if (IsLoaded) return;

            if (_loading is null) _loading = global::Transpose.Require.RequireAsync(global::Transpose.RequireKind.Script, ScriptUrl);

            try
            {
                await _loading;
                IsLoaded = true;
            }
            catch
            {
                _loading = null;
                throw;
            }
        }
    }

    /// <summary>
    /// A Squire 2 editor instance (<c>new Squire(root, config)</c>), declared to the compiler. Every member is an
    /// <c>[External]</c> declaration: nothing is emitted for it, and a call site compiles straight to the JavaScript
    /// it names. Nothing here exists until <see cref="SquireLibrary.LoadAsync"/> has completed.
    /// </summary>
    /// <remarks>
    /// Events are declared once per event (<c>onInput</c> for <c>addEventListener('input', f)</c>) and nothing is
    /// overloaded, because two declarations of one JavaScript name would get a <c>$1</c> suffix from the compiler.
    /// </remarks>
    [External]
    [Convention(Notation.None)]
    [Name("Squire")]
    public class Squire
    {
        public extern Squire(HTMLElement root, SquireConfig config);

        public extern void destroy();

        public extern Squire focus();

        public extern Squire blur();

        public extern HTMLElement getRoot();

        public extern string getHTML();

        public extern Squire setHTML(string html);

        public extern Squire insertHTML(string html, bool isPaste);

        public extern HTMLImageElement insertImage(string src, object attributes);

        public extern string getSelectedText();

        /// <summary>The selection as a range. Squire keeps the last one while the editor is not focused.</summary>
        public extern Range getSelection();

        public extern Squire setSelection(Range range);

        public extern string getPath();

        public extern bool hasFormat(string tag);

        public extern Squire bold();

        public extern Squire italic();

        public extern Squire underline();

        public extern Squire strikethrough();

        public extern Squire removeBold();

        public extern Squire removeItalic();

        public extern Squire removeUnderline();

        public extern Squire removeStrikethrough();

        public extern Squire makeLink(string url);

        public extern Squire removeLink();

        public extern Squire setFontFace(string name);

        public extern Squire setFontSize(string size);

        public extern Squire setTextColor(string color);

        public extern Squire setHighlightColor(string color);

        public extern Squire setTextAlignment(string alignment);

        public extern Squire increaseQuoteLevel();

        public extern Squire decreaseQuoteLevel();

        public extern Squire makeUnorderedList();

        public extern Squire makeOrderedList();

        public extern Squire removeList();

        public extern Squire increaseListLevel();

        public extern Squire decreaseListLevel();

        public extern Squire code();

        public extern Squire removeCode();

        public extern Squire toggleCode();

        public extern Squire removeAllFormatting();

        public extern Squire moveCursorToStart();

        public extern Squire moveCursorToEnd();

        public extern Squire undo();

        public extern Squire redo();

        public extern Squire saveUndoState();

        /// <summary>Replaces the selected blocks with what <paramref name="modify"/> returns for them, as one undo step.</summary>
        public extern Squire modifyBlocks(Func<DocumentFragment, Node> modify);

        /// <summary>Runs <paramref name="modify"/> without Squire treating the DOM changes it makes as an edit: no <c>input</c> event, no undo state.</summary>
        public extern Squire modifyDocument(Action modify);

        /// <summary>A new empty block of the configured block tag (a <c>DIV</c>), with the cursor fix-up Squire gives empty blocks.</summary>
        public extern HTMLElement createDefaultBlock();

        /// <summary>Sets the handler of a key combination (<c>Ctrl-u</c>, <c>Meta-Shift-5</c>, …); null removes it.</summary>
        public extern Squire setKeyHandler(string key, object handler);

        /// <summary>The handler of a key combination, or undefined.</summary>
        [Template("{this}._keyHandlers[{key}]")]
        public extern object getKeyHandler(string key);

        /// <summary><c>addEventListener('input', …)</c>: the document changed through an edit.</summary>
        [Template("{this}.addEventListener('input', {handler})")]
        public extern void onInput(Action<Event> handler);

        [Template("{this}.removeEventListener('input', {handler})")]
        public extern void offInput(Action<Event> handler);

        /// <summary><c>addEventListener('pathChange', …)</c>: the formatting path at the cursor changed; <c>detail.path</c> holds it.</summary>
        [Template("{this}.addEventListener('pathChange', {handler})")]
        public extern void onPathChange(Action<Event> handler);

        /// <summary><c>addEventListener('select', …)</c> and <c>'cursor'</c>: the selection moved.</summary>
        [Template("({this}.addEventListener('select', {handler}), {this}.addEventListener('cursor', {handler}))")]
        public extern void onSelectionChange(Action<Event> handler);

        [Template("({this}.removeEventListener('select', {handler}), {this}.removeEventListener('cursor', {handler}))")]
        public extern void offSelectionChange(Action<Event> handler);

        [Template("{this}.addEventListener('keydown', {handler})")]
        public extern void onKeyDown(Action<KeyboardEvent> handler);

        [Template("{this}.addEventListener('keyup', {handler})")]
        public extern void onKeyUp(Action<KeyboardEvent> handler);

        [Template("{this}.addEventListener('keypress', {handler})")]
        public extern void onKeyPress(Action<KeyboardEvent> handler);

        [Template("{this}.addEventListener('focus', {handler})")]
        public extern void onFocus(Action<Event> handler);

        [Template("{this}.addEventListener('blur', {handler})")]
        public extern void onBlur(Action<Event> handler);

        /// <summary>The configuration the editor was created with (after Squire filled in its defaults).</summary>
        [Template("{this}._config")]
        public extern SquireConfig getConfig();
    }

    /// <summary>The configuration object of <c>new Squire(root, config)</c>. Only the members that are assigned are emitted.</summary>
    [ObjectLiteral]
    public class SquireConfig
    {
        /// <summary>The tag of a new block, <c>DIV</c> by default.</summary>
        public string blockTag;

        /// <summary>Attributes put on every new block.</summary>
        public object blockAttributes;

        /// <summary>Whether URLs typed or pasted are turned into links.</summary>
        public bool addLinks;

        /// <summary>Turns HTML (from <c>setHTML</c>, <c>insertHTML</c> or a paste) into the fragment that is inserted.</summary>
        public Func<string, DocumentFragment> sanitizeToDOMFragment;

        /// <summary>Called with an error thrown inside one of Squire's event handlers.</summary>
        public Action<object> didError;
    }
}
