---
name: sidenav
description: A narrow vertical icon navigation rail (icon + small label) for the leftmost app navigation, often paired with a Sidebar. Use when building a top-level section switcher in a Tesserae (C#/h5) app.
---

# Sidenav

A thin vertical rail of icon buttons used as the leftmost navigation in an app.
Each item is a `SidenavButton` (icon on top, small label below). Often combined
with a `Sidebar` to its right: the Sidenav picks the section, the Sidebar shows
its context. Can also stand alone.

## Create

`UI.Sidenav()` (i.e. `Sidenav()`) returns a `Sidenav`. Items implement
`ISidenavItem` — in practice `SidenavButton`. Add them to header / middle /
footer sections. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

Sidenav:

- `.AddHeader(item)` / `.AddContent(item)` / `.AddFooter(item)` — top / middle /
  bottom of the rail.
- `.Select(identifier)` — mark one item selected, deselect the rest.
- `.HideLabels(bool = true)` — show icons only.
- `.Secondary()` — secondary background colour.
- `.RemoveContent(item)`, `.Clear()`, `.ClearContent()`.

SidenavButton — `new SidenavButton(string identifier, UIcons icon, string text)`
(or with `href` / `UIconsWeight`):

- `.Selected(bool = true)`, `.OnClick(action)`, `.Tooltip(text)`.
- `.ShowDot(bool = true)` / `.DotDanger()` — notification dot.
- `.AsBrand()` — style as the brand/logo at the top.

## Example

```csharp
using static Tesserae.UI;

var sidenav = Sidenav();
sidenav.AddHeader(new SidenavButton("brand", UIcons.Rocket, "App").AsBrand());

var home  = new SidenavButton("home",  UIcons.Home,     "Home").Selected();
var build = new SidenavButton("build", UIcons.Database, "Build").ShowDot();
sidenav.AddContent(home);
sidenav.AddContent(build);
sidenav.AddFooter(new SidenavButton("user", UIcons.User, "Account"));

home.OnClick(()  => sidenav.Select("home"));
build.OnClick(() => sidenav.Select("build"));

var app = HStack().WS().H(600).Children(sidenav.HS(), VStack().Grow().HS());
```

## Related

- Sidebar (full nav panel to the right) — `../sidebar/SKILL.md`
- Full docs & API: `/tesserae/components/sidenav`
