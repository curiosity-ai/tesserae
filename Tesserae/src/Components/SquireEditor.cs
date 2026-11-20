using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A rich text (WYSIWYG) editor built on <see href="https://github.com/fastmail/Squire">Squire</see>, with a
    /// formatting toolbar. It reads and writes HTML (<see cref="HTML"/>) and Markdown (<see cref="Markdown"/>), and
    /// can be bound to a shared <see cref="Y.Text"/> holding Markdown for real-time collaboration
    /// (<see cref="SquireYjsBinding"/>), with any other editor of the same Markdown - a code editor, another
    /// <see cref="SquireEditor"/> - on the other end.
    /// </summary>
    /// <remarks>
    /// <para>The editor edits a <c>contenteditable</c> element in the page, themed by Tesserae's tokens. Squire is
    /// loaded on demand (<see cref="SquireLibrary"/>); content set before it has loaded is applied once it has, and
    /// <see cref="Ready"/> completes then.</para>
    /// <para>HTML that goes in (set, inserted or pasted) is sanitized by DOMPurify, and <c>&lt;style&gt;</c> elements are
    /// removed as well: an email's stylesheet would otherwise apply to the whole page.</para>
    /// </remarks>
    [Transpose.Name("tss.SquireEditor")]
    public sealed class SquireEditor : IComponent
    {
        private readonly HTMLElement _root;
        private readonly HTMLElement _scroller;
        private readonly HTMLElement _cursorLayer;
        private readonly Stack       _toolbar;
        private readonly Stack       _toolbarStart;
        private readonly HTMLElement _container;
        private readonly TaskCompletionSource<bool> _ready = new TaskCompletionSource<bool>();
        private readonly SquireMarkdownDocument     _markdown = new SquireMarkdownDocument();

        private Squire              _squire;
        private SquireEditorText    _text = new SquireEditorText();
        private string              _initialHtml = "";
        private string              _initialMarkdown;
        private bool                _focusOnReady;
        private double              _changeTimeout;
        private int                 _changeDelay = 1000;
        private bool                _toolbarBuilt;
        private bool                _showToolbar = true;
        private bool                _disposed;

        private Button _bold, _italic, _underline, _link, _orderedList, _unorderedList, _code, _quote;

        /// <summary>Raised on every change of the content made through the editor, as it happens.</summary>
        public event Action<SquireEditor> InputChanged;

        /// <summary>Raised once the content has stopped changing for <see cref="ChangeDelay"/> milliseconds (1 s by default).</summary>
        public event Action<SquireEditor> Changed;

        public event Action<KeyboardEvent> KeyDown;
        public event Action<KeyboardEvent> KeyUp;
        public event Action<KeyboardEvent> KeyPress;

        /// <summary>Raised when the selection or the caret moves.</summary>
        public event Action<SquireEditor> SelectionChanged;

        /// <summary>Creates an editor. Squire starts loading straight away.</summary>
        public SquireEditor()
        {
            _root        = Div(Att("tss-squire-content"));
            _cursorLayer = Div(Att("tss-squire-cursors"));
            _scroller    = Div(Att("tss-squire-scroller"), _root, _cursorLayer);

            _root.setAttribute("role", "textbox");
            _root.setAttribute("aria-multiline", "true");

            _toolbarStart = HStack().AlignItemsCenter().NoShrink();
            _toolbar      = HStack().AlignItemsCenter().WS().Wrap().Class("tss-squire-toolbar");

            _container = Div(Att("tss-squire"), _scroller, _toolbar.Render());

            InitializeAsync().FireAndForget();
        }

        /// <summary>Completes once Squire has loaded and the editor holds its initial content.</summary>
        public Task Ready => _ready.Task;

        /// <summary>The Squire instance, or null until <see cref="Ready"/> has completed.</summary>
        public Squire Squire => _squire;

        /// <summary>The element being edited.</summary>
        public HTMLElement ContentElement => _root;

        internal HTMLElement CursorLayer => _cursorLayer;

        internal HTMLElement Scroller => _scroller;

        internal SquireMarkdownDocument MarkdownDocument => _markdown;

        /// <summary>Whether the editor holds the focus.</summary>
        public bool HasFocus => _root == document.activeElement || _root.contains(document.activeElement);

        public HTMLElement Render() => _container;

        #region Setup

        private async Task InitializeAsync()
        {
            await SquireLibrary.LoadAsync();

            if (_disposed) return;

            _squire = new Squire(_root, new SquireConfig
            {
                blockTag              = "DIV",
                addLinks              = true,
                sanitizeToDOMFragment = Sanitize,
                didError              = error => console.error(error)
            });

            if (_initialMarkdown != null) _markdown.Render(_squire, _initialMarkdown);
            else _squire.setHTML(_initialHtml ?? "");

            _initialMarkdown = null;
            _initialHtml     = null;

            _squire.onInput(_ => OnInput());
            _squire.onPathChange(_ => UpdateToolbarState());
            _squire.onSelectionChange(_ => SelectionChanged?.Invoke(this));
            _squire.onKeyDown(e => KeyDown?.Invoke(e));
            _squire.onKeyUp(e => KeyUp?.Invoke(e));
            _squire.onKeyPress(e => KeyPress?.Invoke(e));

            if (_showToolbar) BuildToolbar();

            _squire.moveCursorToStart();
            if (_focusOnReady) Focus();

            _ready.TrySetResult(true);
        }

        private static DocumentFragment Sanitize(string html)
        {
            var fragment = Script.Write<DocumentFragment>("DOMPurify.sanitize({0}, { ALLOW_UNKNOWN_PROTOCOLS: true, WHOLE_DOCUMENT: false, RETURN_DOM: true, RETURN_DOM_FRAGMENT: true, FORCE_BODY: false, FORBID_TAGS: ['style'] })", html ?? "");
            return fragment != null ? (DocumentFragment)document.importNode(fragment, true) : document.createDocumentFragment();
        }

        private void OnInput()
        {
            InputChanged?.Invoke(this);

            window.clearTimeout(_changeTimeout);
            _changeTimeout = window.setTimeout(_ => Changed?.Invoke(this), _changeDelay);
        }

        #endregion

        #region Content

        /// <summary>
        /// The content as HTML. Set before Squire has loaded, it is applied once it has. Setting it starts the undo
        /// history over.
        /// </summary>
        public string HTML
        {
            get
            {
                if (_squire == null) return _initialMarkdown != null ? Tesserae.Markdown.ConvertMarkdownSanitized(_initialMarkdown) : _initialHtml;
                return SquireMarkdownDocument.StripIds(_squire.getHTML());
            }
            set
            {
                _markdown.Clear();

                if (_squire == null)
                {
                    _initialHtml     = value ?? "";
                    _initialMarkdown = null;
                }
                else
                {
                    _squire.setHTML(value ?? "");
                }
            }
        }

        /// <summary>
        /// The content as Markdown (see <see cref="SquireMarkdown"/> for what is written). Setting it renders the
        /// Markdown into the editor; reading it back after edits returns the text as it was written for every block
        /// that was not edited, and the edited blocks as <see cref="SquireMarkdown"/> writes them.
        /// </summary>
        public string Markdown
        {
            get
            {
                if (_squire == null) return _initialMarkdown ?? SquireMarkdown.HtmlToMarkdown(_initialHtml);
                return _markdown.Read(_root).Markdown;
            }
            set
            {
                if (_squire == null)
                {
                    _initialMarkdown = value ?? "";
                    _initialHtml     = null;
                }
                else
                {
                    _markdown.Render(_squire, value ?? "");
                }
            }
        }

        /// <summary>The text of the content, without formatting.</summary>
        public string PlainText => _squire == null ? "" : _root.innerText ?? "";

        /// <summary>Inserts HTML at the cursor (sanitized), replacing the selection.</summary>
        public SquireEditor InsertHTML(string html)
        {
            Ready.ContinueWith(_ => _squire.insertHTML(html ?? "", false));
            return this;
        }

        #endregion

        #region Configuration

        /// <summary>Sets the texts the toolbar shows, for a host that translates its interface.</summary>
        public SquireEditor Texts(SquireEditorText text)
        {
            _text = text ?? new SquireEditorText();
            if (_toolbarBuilt) BuildToolbar();
            return this;
        }

        /// <summary>Hides (or shows) the formatting toolbar. Keyboard shortcuts keep working.</summary>
        public SquireEditor Toolbar(bool show = true)
        {
            _showToolbar = show;

            if (show)
            {
                _toolbar.Show();
                if (_squire != null && !_toolbarBuilt) BuildToolbar();
            }
            else
            {
                _toolbar.Collapse();
            }

            return this;
        }

        /// <summary>Puts <paramref name="components"/> at the start of the toolbar, before the formatting buttons.</summary>
        public SquireEditor ToolbarStart(params IComponent[] components)
        {
            foreach (var c in components) _toolbarStart.Add(c);
            return this;
        }

        /// <summary>Removes the border around the editor, for an editor that is the page (a note) rather than a field on it.</summary>
        public SquireEditor Borderless(bool borderless = true)
        {
            _container.classList.toggle("tss-squire-borderless", borderless);
            return this;
        }

        /// <summary>How long <see cref="Changed"/> waits after the last edit, in milliseconds.</summary>
        public SquireEditor ChangeDelay(int milliseconds)
        {
            _changeDelay = Math.Max(0, milliseconds);
            return this;
        }

        /// <summary>Sets the tab index of the editable element.</summary>
        public SquireEditor TabIndex(int index)
        {
            _root.tabIndex = index;
            return this;
        }

        /// <summary>Calls <paramref name="onInput"/> on every change, as it happens.</summary>
        public SquireEditor OnInput(Action<SquireEditor> onInput)
        {
            InputChanged += onInput;
            return this;
        }

        /// <summary>Calls <paramref name="onChange"/> once the content has stopped changing for <see cref="ChangeDelay"/>.</summary>
        public SquireEditor OnChange(Action<SquireEditor> onChange)
        {
            Changed += onChange;
            return this;
        }

        #endregion

        #region Focus

        /// <summary>Moves the focus into the editor (once it is ready).</summary>
        public void Focus()
        {
            if (_squire == null)
            {
                _focusOnReady = true;
                return;
            }

            _squire.focus();
        }

        public void Blur()
        {
            _focusOnReady = false;
            _squire?.blur();
        }

        public void MoveCursorToStart() => _squire?.moveCursorToStart();

        public void MoveCursorToEnd() => _squire?.moveCursorToEnd();

        #endregion

        #region Commands

        public void Bold()                          => _squire?.bold();
        public void Italic()                        => _squire?.italic();
        public void Underline()                     => _squire?.underline();
        public void Strikethrough()                 => _squire?.strikethrough();
        public void RemoveBold()                    => _squire?.removeBold();
        public void RemoveItalic()                  => _squire?.removeItalic();
        public void RemoveUnderline()               => _squire?.removeUnderline();
        public void RemoveStrikethrough()           => _squire?.removeStrikethrough();
        public void MakeLink(string url)            => _squire?.makeLink(url);
        public void RemoveLink()                    => _squire?.removeLink();
        public void SetFontFace(string font)        => _squire?.setFontFace(font);
        public void SetFontSize(int px)             => _squire?.setFontSize(px + "px");
        public void SetTextColor(string color)      => _squire?.setTextColor(color);
        public void SetHighlightColor(string color) => _squire?.setHighlightColor(color);
        public void SetTextAlignment(string align)  => _squire?.setTextAlignment(align);
        public void IncreaseQuoteLevel()            => _squire?.increaseQuoteLevel();
        public void DecreaseQuoteLevel()            => _squire?.decreaseQuoteLevel();
        public void MakeUnorderedList()             => _squire?.makeUnorderedList();
        public void MakeOrderedList()               => _squire?.makeOrderedList();
        public void RemoveList()                    => _squire?.removeList();
        public void IncreaseListLevel()             => _squire?.increaseListLevel();
        public void DecreaseListLevel()             => _squire?.decreaseListLevel();
        public void Code()                          => _squire?.code();
        public void RemoveCode()                    => _squire?.removeCode();
        public void RemoveAllFormatting()           => _squire?.removeAllFormatting();
        public void InsertImage(string src)         => _squire?.insertImage(src, null);

        /// <summary>Undoes the last change (through the collaborative undo when the editor is bound to a shared text).</summary>
        public void Undo() => _squire?.undo();

        /// <summary>Redoes the last undone change.</summary>
        public void Redo() => _squire?.redo();

        /// <summary>Whether the selection is inside the given element (<c>B</c>, <c>A</c>, <c>UL</c>, …).</summary>
        public bool HasFormat(string tag) => _squire != null && _squire.hasFormat(tag.ToUpperInvariant());

        /// <summary>Turns the selected blocks into headings of <paramref name="level"/> (1 to 6), or paragraphs for 0.</summary>
        public void SetHeading(int level)
        {
            if (_squire == null) return;

            var tag = level >= 1 && level <= 6 ? "H" + level : _squire.getConfig().blockTag;

            _squire.modifyBlocks(fragment =>
            {
                var output = document.createDocumentFragment();

                foreach (var block in LeafBlocks(fragment))
                {
                    var el = document.createElement(tag);
                    while (block.firstChild != null) el.appendChild(block.firstChild);
                    if (el.childNodes.length == 0) el.appendChild(document.createElement("br"));
                    output.appendChild(el);
                }

                return output;
            });
        }

        private static List<Element> LeafBlocks(Node node)
        {
            var list     = new List<Element>();
            var children = node.childNodes;

            for (var i = 0; i < children.length; i++)
            {
                var child = (Node)children[(uint)i];
                if (!SquireMarkdown.IsBlock(child)) continue;

                var inner = LeafBlocks(child);
                if (inner.Count == 0) list.Add((Element)child);
                else list.AddRange(inner);
            }

            return list;
        }

        #endregion

        #region Toolbar

        private void BuildToolbar()
        {
            _toolbarBuilt = true;
            _toolbar.Clear();

            var fontSizes = new[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

            var colors = new[]
            {
                "#51A7F9", "#6FC040", "#F5D427", "#F3901D", "#ED5C57", "#B36AE2",
                "#0C64C0", "#0C882A", "#DCBE22", "#DE6A19", "#C82613", "#763E9B",
                "#174E86", "#0F5C1A", "#C3971D", "#BE5B17", "#861106", "#5E327C",
                "#002451", "#06400C", "#A37519", "#934511", "#570606", "#3B204D",
                "#FFFFFF", "#CCCCCC", "#999999", "#666666", "#333333", "#000000",
            };

            var fonts = new[] { "Arial", "Courier New", "Georgia", "Lucida Sans Unicode", "Tahoma", "Times New Roman", "Trebuchet MS", "Verdana" };

            Button ToolButton(UIcons icon, string label, Action onClick)
            {
                var b = Button().SetIcon(icon).NoMinSize().W(32).H(32).Class("tss-squire-button").OnClick(() => onClick());
                if (!string.IsNullOrEmpty(label)) b.Tooltip(label).AriaLabel(label);
                return b;
            }

            Button Menu(UIcons icon, string label, IComponent content, int maxWidth)
            {
                var b = Button().SetIcon(icon).NoMinSize().W(32).H(32).Class("tss-squire-button")
                   .Tooltip(content, interactive: true, placement: TooltipPlacement.Top, delayHide: 200, maxWidth: maxWidth, hideOnClick: false);
                if (!string.IsNullOrEmpty(label)) b.AriaLabel(label);
                return b;
            }

            _bold      = ToolButton(UIcons.Bold,      _text.Bold,      () => Toggle("B", RemoveBold,      Bold));
            _italic    = ToolButton(UIcons.Italic,    _text.Italic,    () => Toggle("I", RemoveItalic,    Italic));
            _underline = ToolButton(UIcons.Underline, _text.Underline, () => Toggle("U", RemoveUnderline, Underline));

            var heading = Menu(UIcons.Heading, _text.Heading, VStack().AlignItemsCenter().H(200).W(160).ScrollY().Children(
                Button(_text.Heading1).LessPadding().Large().WS().TextCenter().NoMinSize().MinHeight(32.px()).OnClick(() => { SetHeading(1); CloseMenu(); }),
                Button(_text.Heading2).LessPadding().MediumPlus().WS().TextCenter().NoMinSize().MinHeight(32.px()).OnClick(() => { SetHeading(2); CloseMenu(); }),
                Button(_text.Heading3).LessPadding().Medium().WS().TextCenter().NoMinSize().MinHeight(32.px()).OnClick(() => { SetHeading(3); CloseMenu(); }),
                Button(_text.Normal).LessPadding().WS().TextCenter().NoMinSize().MinHeight(32.px()).OnClick(() => { SetHeading(0); CloseMenu(); })), 220);

            _link          = ToolButton(UIcons.Link,           _text.InsertLink,   () => { if (HasFormat("A")) RemoveLink(); else AskForLink(); });
            _unorderedList = ToolButton(UIcons.List,           _text.BulletList,   () => Toggle("UL", RemoveList, MakeUnorderedList));
            _orderedList   = ToolButton(UIcons.Circle1,        _text.NumberedList, () => Toggle("OL", RemoveList, MakeOrderedList));
            _quote         = ToolButton(UIcons.QuoteRight,     _text.Quote,        () => Toggle("BLOCKQUOTE", DecreaseQuoteLevel, IncreaseQuoteLevel));
            _code          = ToolButton(UIcons.SquareTerminal, _text.Code,         () => { if (HasFormat("CODE") || HasFormat("PRE")) RemoveCode(); else Code(); });

            var undo = ToolButton(UIcons.Undo, _text.Undo, Undo);
            var redo = ToolButton(UIcons.Redo, _text.Redo, Redo);

            // What is used less goes behind "…": built when it opens, so the strikethrough row shows the selection's state.
            Button more = null;
            more = ToolButton(UIcons.MenuDots, _text.MoreOptions, () => BuildMoreMenu(fonts, fontSizes, colors).ShowFor(more));

            _toolbar.Children(_toolbarStart,
                _bold, _italic, _underline, heading,
                _link, _unorderedList, _orderedList, _quote, _code,
                undo, redo, more);

            if (!_showToolbar) _toolbar.Collapse();
        }

        private ContextMenu BuildMoreMenu(string[] fonts, int[] fontSizes, string[] colors)
        {
            var menu = ContextMenu();

            ContextMenu.Item Row(UIcons icon, string label, Action onClick = null, bool active = false)
            {
                var item = ContextMenuItem(HStack().AlignItemsCenter().Gap(8.px()).Children(
                    Icon(icon).W(16),
                    TextBlock(label).Grow(),
                    active ? (IComponent)Icon(UIcons.Check).Foreground(Theme.Primary.Background) : Empty()));

                if (onClick != null) item.OnClick(() =>
                {
                    menu.Hide();
                    onClick();
                    _squire?.focus();
                });

                return item;
            }

            var fontMenu = ContextMenu().Items(fonts.Select(f =>
                ContextMenuItem(TextBlock(f).Style(st => st.fontFamily = f)).OnClick(() => { menu.Hide(); SetFontFace(f); _squire?.focus(); })).ToArray());

            var sizeMenu = ContextMenu().Items(fontSizes.Select(px =>
                ContextMenuItem(px + " px").OnClick(() => { menu.Hide(); SetFontSize(px); _squire?.focus(); })).ToArray());

            var colorMenu = ContextMenu().Items(ContextMenuItem(HStack().Wrap().W(192).Children(
                colors.Select(c => Button().NoMinSize().W(28).H(28).MinHeight(28.px()).Background(c).AriaLabel(c).Class("tss-squire-swatch").OnClick(() =>
                {
                    menu.Hide();
                    SetTextColor(c);
                    _squire?.focus();
                })))));

            menu.Items(
                Row(UIcons.Strikethrough, _text.Strikethrough, () => Toggle("S", RemoveStrikethrough, Strikethrough), HasFormat("S")),
                Row(UIcons.F,             _text.Font).SubMenu(fontMenu),
                Row(UIcons.TextHeight,    _text.FontSize).SubMenu(sizeMenu),
                Row(UIcons.Palette,       _text.TextColor).SubMenu(colorMenu),
                ContextMenuItem().Divider(),
                Row(UIcons.PhotoVideo,    _text.InsertImage, AskForImage),
                ContextMenuItem().Divider(),
                Row(UIcons.AlignLeft,     _text.AlignLeft,   () => SetTextAlignment("left")),
                Row(UIcons.AlignCenter,   _text.AlignCenter, () => SetTextAlignment("center")),
                Row(UIcons.Symbol,        _text.AlignRight,  () => SetTextAlignment("right")),
                Row(UIcons.AlignJustify,  _text.Justify,     () => SetTextAlignment("justify")),
                ContextMenuItem().Divider(),
                Row(UIcons.TextSlash,     _text.RemoveFormatting, RemoveAllFormatting));

            return menu;
        }

        // A pick in a toolbar menu leaves the focus on the menu's button: give it back to the editor, at the selection it had.
        private void CloseMenu()
        {
            Tippy.HideAll();
            _squire?.focus();
        }

        private void Toggle(string tag, Action remove, Action add)
        {
            if (HasFormat(tag)) remove();
            else add();
        }

        private void UpdateToolbarState()
        {
            if (!_toolbarBuilt || _squire == null) return;

            _bold.IsPrimary          = HasFormat("B");
            _italic.IsPrimary        = HasFormat("I");
            _underline.IsPrimary     = HasFormat("U");
                        _link.IsPrimary          = HasFormat("A");
            _orderedList.IsPrimary   = HasFormat("OL");
            _unorderedList.IsPrimary = HasFormat("UL");
            _quote.IsPrimary         = HasFormat("BLOCKQUOTE");
            _code.IsPrimary          = HasFormat("CODE") || HasFormat("PRE");
        }

        /// <summary>
        /// Picks an image from the user's computer and inserts it at the cursor as a data URL, so it travels with the
        /// content (an email's recipients cannot reach the application that sent it).
        /// </summary>
        private void AskForImage()
        {
            var input = (HTMLInputElement)document.createElement("input");

            input.type   = "file";
            input.accept = "image/png,image/jpeg,image/gif,image/webp";

            input.onchange = _ =>
            {
                if (input.files.length == 0) return;

                var reader = new FileReader();
                reader.onload = __ => InsertImage((string)reader.result);
                reader.readAsDataURL(input.files[0]);
            };

            input.click();
        }

        private void AskForLink()
        {
            var textBox = TextBox().SetPlaceholder(_text.LinkPlaceholder).W(400).PT(16);

            textBox.WhenMounted(() => textBox.Focus());

            Dialog(title: TextBlock(_text.InsertLink).Medium(), content: textBox, centerContent: false)
               .OkCancel(
                    btnOk: b => b.SetText(_text.Ok).Primary(),
                    btnCancel: b => b.SetText(_text.Cancel),
                    onOk: () =>
                    {
                        if (!string.IsNullOrWhiteSpace(textBox.Text)) MakeLink(textBox.Text.Trim());
                    });
        }

        #endregion

        /// <summary>Stops the editor. It cannot be used again.</summary>
        public void Destroy()
        {
            _disposed = true;
            window.clearTimeout(_changeTimeout);
            _squire?.destroy();
        }
    }

    /// <summary>
    /// The texts a <see cref="SquireEditor"/>'s toolbar shows, in English by default. A host that translates its
    /// interface sets every member from its own catalogue and passes it to <see cref="SquireEditor.Texts"/>.
    /// </summary>
    public sealed class SquireEditorText
    {
        public string Bold             { get; set; } = "Bold";
        public string Italic           { get; set; } = "Italic";
        public string Underline        { get; set; } = "Underline";
        public string Strikethrough    { get; set; } = "Strikethrough";
        public string Font             { get; set; } = "Font";
        public string FontSize         { get; set; } = "Font size";
        public string TextColor        { get; set; } = "Text color";
        public string Heading          { get; set; } = "Heading";
        public string Heading1         { get; set; } = "Heading 1";
        public string Heading2         { get; set; } = "Heading 2";
        public string Heading3         { get; set; } = "Heading 3";
        public string Normal           { get; set; } = "Normal";
        public string InsertLink       { get; set; } = "Insert Link";
        public string LinkPlaceholder  { get; set; } = "Enter your link...";
        public string BulletList       { get; set; } = "Bullet List";
        public string NumberedList     { get; set; } = "Numbered List";
        public string InsertImage      { get; set; } = "Insert image";
        public string Quote            { get; set; } = "Quote";
        public string Code             { get; set; } = "Code";
        public string AlignLeft        { get; set; } = "Align Left";
        public string AlignCenter      { get; set; } = "Align Center";
        public string AlignRight       { get; set; } = "Align Right";
        public string Justify          { get; set; } = "Justify";
        public string Undo             { get; set; } = "Undo";
        public string Redo             { get; set; } = "Redo";
        public string RemoveFormatting { get; set; } = "Remove Formatting";
        public string MoreOptions      { get; set; } = "More options";
        public string Ok               { get; set; } = "Ok";
        public string Cancel           { get; set; } = "Cancel";
    }
}
