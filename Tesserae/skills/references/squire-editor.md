# SquireEditor

A rich text (WYSIWYG) editor with a formatting toolbar, built on [Squire](https://github.com/fastmail/Squire) 2. It
reads and writes HTML and **Markdown**, and binds to a shared Yjs text of Markdown for collaborative editing.

Squire is loaded on demand (`SquireLibrary.LoadAsync()`, the unlinked `tss-squire.min.js`); an editor starts the load
itself, and content set before it has loaded is applied when it has. `editor.Ready` completes then.

## Create

```csharp
var editor = SquireEditor()          // UI.SquireEditor()
   .H(400)
   .OnChange(e => Save(e.Markdown))  // debounced, 1 s by default (.ChangeDelay(ms))
   .OnInput(e => Preview(e.HTML));   // every change
```

## Content

| Member | |
|---|---|
| `HTML` | get/set. Sanitized by DOMPurify; `<style>` elements are dropped (the editor is in the page, not an iframe). Setting it resets undo. |
| `Markdown` | get/set. Blocks nobody edited come back exactly as written; edited blocks as `SquireMarkdown` writes them. Setting it turns on Markdown mode. |
| `SetMarkdown(md)` / `SetHTML(html)` | chainable setters for the two properties. |
| `PlainText` | the text without formatting. |
| `InsertHTML(html)` | at the cursor. |

`SquireMarkdown.ToMarkdown(element)` / `SquireMarkdown.HtmlToMarkdown(html)` convert any HTML. Markdown covers headings,
paragraphs, hard breaks, bold, italic, strikethrough, inline code, code blocks, links, images, quotes, nested lists,
tables and rules; underline, sub- and superscript are written as HTML tags; fonts, sizes, colours and alignment are dropped.

## Toolbar

Bold, italic, underline, heading, link, bullet and numbered lists, quote, code, undo and redo are buttons; the rest
(strikethrough, font, size and colour submenus, insert image, alignment, remove formatting) is under the `…` button, a
`ContextMenu` built each time it opens.

## Markdown mode

`.MarkdownMode()` limits the editor to what Markdown can represent. The toolbar becomes bold, italic, strikethrough,
heading, link, lists, quote, code, undo, redo and `…` (insert image, remove formatting); the underline / sub- / superscript
shortcuts do nothing; and HTML that comes in (paste, `InsertHTML`, the `HTML` setter) is converted to Markdown and back
(`SquireMarkdown.HtmlToMarkdown(html, strict: true)`), so the editor always holds exactly what `Markdown` returns.
Turning it on converts the current content the same way. Setting `Markdown` / `SetMarkdown(...)` and `BindYjs` turn it on;
call `.MarkdownMode(false)` afterwards for the full toolbar.

## Configure

- `.Texts(new SquireEditorText { Bold = "Fett", ... })` — toolbar labels, for translation.
- `.Toolbar(false)` — hide the toolbar (shortcuts still work).
- `.ToolbarStart(components...)` — put your own controls before the formatting buttons.
- `.Borderless()` — no frame, for an editor that is the page (a note).
- `.TabIndex(i)`, `.Focus()`, `.Blur()`, `MoveCursorToStart()/End()`.
- Commands: `Bold()`, `Italic()`, `MakeLink(url)`, `SetHeading(1..6 | 0)`, `MakeUnorderedList()`, `Code()`, `Undo()`, … and
  `HasFormat("B")`. `Squire` exposes the underlying instance after `Ready`.
- Events: `InputChanged`, `Changed`, `SelectionChanged`, `KeyDown`, `KeyUp`, `KeyPress`.

## Collaboration

```csharp
await Yjs.LoadAsync();
var doc       = new Y.Doc();
var text      = doc.getText("markdown");
var awareness = new Awareness(doc);
awareness.setLocalStateField("user", YjsUserColors.UserFor("Ada"));

var binding = editor.BindYjs(text, awareness);   // SquireYjsBinding; Dispose() to unbind
```

- The shared text is Markdown, so any Markdown editor can be on the other end (Tesserae.Monaco's
  `CodeEditor.BindYjs(text, awareness)` included); carets show across both.
- Local edits are diffed against the shared text; remote edits re-render only the changed blocks.
- Undo/redo (toolbar, Ctrl+Z/Y) go through a `Y.UndoManager` that only reverts this user's edits.
- Empty paragraphs have no Markdown spelling, so they stay local to the editor that has them.

## Related

- `yjs.md`, `markdown-block.md`, `text-area.md`.
