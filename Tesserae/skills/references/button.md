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
the form to reach for when a red button would shout louder than the action deserves. `.Link().Warning()` is
the same for an action that needs care but is not destructive (Archive, Overwrite).

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

Tone: `.Primary()`, `.Success()`, `.Warning()`, `.Danger()` (default is neutral). `IsPrimary` / `IsSuccess` / `IsWarning` / `IsDanger` read and set the same tones; they are mutually exclusive.
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
- `.OnClickSpinWhile(Func<Task<ButtonSpinnerOutcome>> action, string text = null, ...)` — the same, ending on the outcome the action returns (a check for `Success`, an x for `Error` or an exception).
- `.Disabled(bool = true)`.
- `.AI(bool withSparklesIcon = true, bool strongEffect = false)` — an action that asks a model for something: a filled purple-to-blue gradient, plus the Sparkles glyph unless the button already has an icon. The AI equivalent of `Primary()` — one per surface.
- `.AISubtle(bool withSparklesIcon = true, bool strongEffect = false)` — the quiet form of the same hue, for the second and third AI action beside it. `strongEffect` asks for the louder AI form a custom theme may draw. See `ai-variants.md`.
- `.WithHotKey(string keys)` — bind a keyboard shortcut.
- `.Focus()` — move focus to the button.

Spinner control: `.ToSpinner(text)` / `.UndoSpinner()` / `.UndoSpinner(ButtonSpinnerOutcome)` / `.SpinWhile(...)`. Every button spins in place, the "orbiting dot": its content (icon, label) fades, a dot appears on a ring drawn in the button's own colour, orbits and stretches into an arc, and collapses back to the content when the spinner ends. The button keeps its box (its width and height do not change, so nothing around it shifts; a spinner text sits beside the ring and is ellipsized if it does not fit). To end on a result, give `OnClickSpinWhile` / `SpinWhile` an action that returns `Task<ButtonSpinnerOutcome>`: `Success` closes the ring around a check, `Error` (or an exception) around an x, `None` goes straight back. A ghost button draws the mark green or red; a filled button keeps its text colour for the mark and turns its fill green or red instead. `.UndoSpinner(ButtonSpinnerOutcome.Success)` does the same by hand after `.ToSpinner()`.

Progress: `.SetProgress(double percent)` fills the button from the left, behind its label, sweeping to each new value (the progress of an upload or export the button started); `.ClearProgress()` removes the fill. Keep the number in the label when it matters (`SetText($"Uploading {p}%")`). For press-and-hold, two-step and copy buttons see `hold-button.md`, `confirm-button.md`, `copy-button.md`.

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
- Full docs & API: `/tesserae/commands/button`
