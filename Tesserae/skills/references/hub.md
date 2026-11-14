---
name: hub
description: A top-level page with a breadcrumb header (the hub's name first, then crumbs) and commands on the right, above SectionStack sections. Use for the frame of a full page in a Tesserae (C#/Transpose) app; HubHeader is the same header row on its own.
---

# Hub / HubHeader

`Hub` is the frame of a top-level page: a `HubHeader` row (a breadcrumb trail that starts
at the hub's name, then the page's commands on the right) above the page's sections. It
**is a `SectionStack`**, so content goes in with `.Section(...)` / `.FlatSection(...)`, and
it fills the area it is put in (`width/height: 100%`, page background).

The trail takes whatever room the commands leave and clips; commands keep their width and
are never squeezed. Crumbs between the root and the last one ellipsize first, the root
keeps its name. Every crumb after the first draws its own separator, so collapsing a
crumb's component (`.Collapse()`) hides the separator with it.

## Create

```csharp
using static Tesserae.UI;

Hub(string title, string href = null)          // root crumb links to href (Router.Navigate on a plain click,
                                               // a real <a> for middle-click / new tab)
Hub(string title, Action onClick)
Hub(IObservable<string> title, string href = null)
Hub(HubHeader header)

HubHeader(string title, string href = null)    // the header row on its own
HubHeader(string title, Action onClick)
HubHeader(IObservable<string> title, string href = null)
```

## Key configuration (on `Hub` and `HubHeader`, both fluent)

- `.Crumb(string text, string href = null)` — a text crumb; without a link it is the page you are on.
- `.Crumb(string text, Action onClick)` / `.Crumb(IObservable<string> text, string href = null)`.
- `.Crumb(IComponent component)` / `.Crumbs(params IComponent[])` — any component as a crumb (a title with
  an avatar, a picker, a name that comes and goes). Give a text in it the class `tss-hub-crumb-label` to
  draw it like the text crumbs.
- `.Commands(params IComponent[])` — right-aligned; the header puts a 4px gap between them, so don't add margins.
- `HubHeader.ClearCommands()`.
- `Hub.Header` — the `HubHeader`. `Hub.Clear()` removes the sections and keeps the header.
- Everything `SectionStack` has: `.Section(c, grow: true)` takes the remaining height (lists, split views),
  `.Title(...)` adds another title row.

The last crumb carries `aria-current="page"`; the trail is `role="navigation"` labelled `Breadcrumb`.

## CSS hooks

`.tss-hub`, `.tss-hub-header`, `.tss-hub-trail`, `.tss-hub-crumb` (`.tss-hub-crumb-root` for the first),
`.tss-hub-crumb-text` / `.tss-hub-crumb-link`, `.tss-hub-crumb-label`, `.tss-hub-commands`.
Hiding the root crumb (e.g. on a phone where a page bar already names it) also needs
`.tss-hub-crumb-root + .tss-hub-crumb::before { display: none }` unless the root is collapsed with `.Collapse()`,
which the stylesheet already handles.

## Example

```csharp
using static Tesserae.UI;

var chatName = TextBlock("").Class("tss-hub-crumb-label");
var chatCrumb = HStack().AlignItemsCenter().Children(chatName).Collapse();

var page = Hub("Data", "#/data")
   .Crumb("Schemas", "#/data/schemas")
   .Crumb("Invoice")
   .Commands(
        Button("Edit schema").SetIcon(UIcons.Pencil),
        Button().SetIcon(UIcons.Download).Tooltip("Download"))
   .Section(SplitView().Left(sidebar).Right(content).S(), grow: true);

// Several headed groups on one page you manage yourself
var groups = SectionStack().S()
   .Title(HubHeader("Organizations", "#/orgs").Commands(Button().SetIcon(UIcons.Search)))
   .Section(orgCards)
   .Title(HubHeader("People", "#/people"))
   .Section(peopleCards, grow: true);
```

## Related

- SectionStack — `section-stack.md`
- TextBreadcrumbs — `text-breadcrumbs.md`
- Breadcrumb — `breadcrumb.md`
- Full docs & API: `/tesserae/layout/hub`
