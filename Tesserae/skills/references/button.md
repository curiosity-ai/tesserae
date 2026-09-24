---
name: button
description: The standard clickable button with tone variants, icons, an href that makes it a real link, and inline async spinner states. Use for any action trigger — submit, confirm, navigate, run — in a Tesserae (C#/Transpose) app.
---

# Button

`Button` is the primary action control. It supports tone variants, icons, hotkeys, an inline
spinner for async actions, and an href that turns it into a real link.

## Create

`UI.Button(string text = "", string href = null)` or `UI.Button(UIcons icon)` (icon-only).
Bring factories into scope with `using static Tesserae.UI;`.

## A button that goes somewhere

Pass an `href` and the button renders as an **anchor** rather than a `button` element, so it is
middle-clickable, opens in a new tab on ctrl/cmd-click and shows where it goes in the status bar —
while looking exactly like any other button. There is no separate `Link` component: a link that
looks like a link is `Button(text, href).Link()`, and a small fact that happens to link somewhere is
an `InlineLabel` with `.SetHref(...)`.

```csharp
Button("Open documentation", href: DocsUrl).Primary().SetIcon(UIcons.ArrowUpRightFromSquare)
```

Only a button with an href underlines its label on hover (and only in the link-toned variant) — one
that merely runs a handler is a button, and underlining it would promise an address it doesn't have.

## A button that reads as text

`.Link()` drops the border and the background so the button reads as text rather than as a box, for
an action that should not be the thing the eye lands on. A **tone composes with it**: it colours the
label rather than filling a surface, so a destructive action written as text is `.Link().Danger()` —
the form to reach for when a red button would shout louder than the action deserves.

```csharp
Button("Make private").Link().Danger().OnClick(StageMakePrivate)
```

Hovering it still answers: a neutral link takes the default hover wash, a toned one a tint of its own
tone — never the fill it would have as a button. The label moves *away* from that surface as it is
hovered and pressed, darkening in a light theme and lightening under `.tss-dark-mode`, so the action
gains contrast either way rather than losing it. The label underlines on hover only when the button
really goes somewhere (it was built with an `href`) or when `.LinkOnHover()` is set as well, which is
how a handler-only link asks for the underline.

`.LinkOnHover()` on its own is the quieter cousin: an ordinary button that only takes the link colour
and the underline while the pointer is on it.

A button can have both an href and an `.OnClick(...)` — the usual shape of a link the app would
rather route itself. A plain click runs the handler (which stops the event, so the browser does not
navigate as well), but **ctrl/cmd-click, shift-click and middle-click skip the handler entirely** and
let the browser open the href in a new tab or a new window. Handlers never see a modified click on a
link, so there is nothing to check for in one; the same holds for anything with an `OnClick` that
sits inside a link (a `SidebarButton` or `SidenavButton` built with an href, say). `UI.IsModifiedLinkClick(element, mouseEvent)`
is the check itself, for a custom component that dispatches its own clicks.

## Key configuration

Tone: `.Primary()`, `.Success()`, `.Danger()` (default is neutral).
Style: `.Compact()`, `.NoBorder()`, `.NoBackground()`, `.Link()` (reads as text rather than as a
box, and composes with a tone), `.LinkOnHover()`, `.Color(background, textColor, borderColor, iconColor)`.

Content:

- `.SetText(string)` / `.SetTitle(string)` (hover title) / `.Tooltip(string)`.
- `.SetIcon(UIcons icon, string color = "", ..., bool afterText = false)`.
- `.Wrap()` / `.NoWrap()` / `.Ellipsis()`.
- `.HideTextOnMobile(bool = true)` — while `Theme.IsMobileMode` is on, show only the
  icon (a button with no icon keeps its text). The text stays the `aria-label`.

Behaviour:

- `.OnClick((sender, evt) => ...)` or `.OnClick(Action)`.
- `.OnClickSpinWhile(Func<Task> action, string text = null, ...)` — show a spinner while the async action runs.
- `.Disabled(bool = true)`.
- `.AI(bool withSparklesIcon = true)` — an action that asks a model for something: a filled purple-to-blue gradient, plus the Sparkles glyph unless the button already has an icon. The AI equivalent of `Primary()` — one per surface.
- `.AISubtle(bool withSparklesIcon = true)` — the quiet form of the same hue, for the second and third AI action beside it. See `ai-variants.md`.
- `.WithHotKey(string keys)` — bind a keyboard shortcut.
- `.Focus()` — move focus to the button.

Spinner control: `.ToSpinner(text)` / `.UndoSpinner()` / `.SpinWhile(Func<Task>)`.

## Example

```csharp
using static Tesserae.UI;

var actions = HStack().Children(
    Button("Confirm").SetIcon(UIcons.Check).Success().OnClick(() => alert("ok")),
    Button("Delete").SetIcon(UIcons.Trash).Danger().OnClick(() => alert("deleted")),
    Button("Save").Primary().OnClickSpinWhile(async () => await SaveAsync(), "saving..."),
    Button("Make private").Link().Danger().OnClick(() => MakePrivate()));
```

## Related

- ActionButton — `action-button.md`
- AI variants — the `.AI()` / `.AISubtle()` variant — `ai-variants.md`
- Full docs & API: `/tesserae/components/button`
