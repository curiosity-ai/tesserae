---
name: javascript-interop
description: How to call JavaScript and browser APIs from C# in Tesserae via the Transpose compiler (Script.Write, [Transpose.Name], [Template], [External], Transpose.Core.dom). Use when you need to reach a browser API or inline JS that Transpose doesn't already expose.
---

# Invoking JavaScript with Transpose

Tesserae compiles C# to JavaScript with the **Transpose** compiler. Most of the time
you use the typed `Transpose.Core.dom` bindings, but when you need raw JS — a browser
API Transpose doesn't surface, or a global from a bundled library — Transpose gives you
several escape hatches.

## `Transpose.Script.Write` — inline JS

```csharp
using Transpose;   // brings Script into scope
```

`Script.Write("…")` emits the JS verbatim; the generic form returns a value.
Positional placeholders `{0}`, `{1}`, … are replaced with the **compiled
form** of the C# arguments (real JS references, not string concatenation):

```csharp
// no return value
Script.Write("{0}.scrollIntoView()", element);

// typed return value
double now      = Script.Write<double>("Date.now()");
bool   isArray  = Script.Write<bool>("Array.isArray({0})", children);
object instance = Script.Write<object>("new SomeLib({0}, { gutter: {1} })", el, 10);
```

Keep the JS short and defensive (`try { … } catch (e) { }` for calls that may
throw). This is exactly how the toolkit drives tippy tooltips, masonry layout,
pointer capture in gestures, etc.

## Reaching the DOM and globals

```csharp
using static Transpose.Core.dom;   // document, window, console, alert, navigator, …
```

`Transpose.Core.dom` exposes typed bindings for the standard DOM and `window`/
`document` globals, so prefer it over `Script.Write` when a binding exists:

```csharp
document.body.style.overflow = "hidden";
window.setTimeout((_) => DoThing(), 16);
console.error(ex);
```

Convert between C# and JS views of a value with `.As<T>()` when needed.

## Binding to existing JS with attributes

When you want a *typed* C# surface over existing JS instead of scattering
`Script.Write` calls:

- `[Transpose.Name("globalName")]` on a type maps the C# type to that JS
  global/namespace (e.g. `[Transpose.Name("tss.Button")]`). Used throughout Tesserae
  to keep generated class names stable.
- `[External]` + `extern` members declare a binding with **no C# body** — the
  call compiles straight to the underlying JS member.
- `[Transpose.Template("…")]` emits a specific JS expression for a member, with
  `{this}`, `{index}`, argument names as placeholders.

```csharp
[Transpose.Name("tss.ROA")]
public class ReadOnlyArray<T>
{
    public extern ReadOnlyArray(T[] data);

    [External]
    public extern T this[int index] { [Template("{this}[{index}]")] get; }

    public extern int Length { [Name("length")] get; }   // maps to JS .length
}
```

## Loading a library at run time

A bundle only some screens need does not belong in `index.html`. Fetch it when it is first
used, through the Transpose runtime's loader:

```csharp
using Transpose;

await Require.RequireAsync("assets/js/chart.min.js");                  // classic script
await Require.RequireAsync("assets/css/chart.css", "assets/js/chart.js"); // in order: css, then js
await Require.RequireAsync(RequireKind.Module, "./viewer.js");         // a module that keeps a .js name
```

It picks the element from the URL (`.css` → a stylesheet link, `.mjs` → a module, anything else →
a classic script), resolves the URL against the document base, shares one fetch between every
caller, and — the part worth knowing — **falls back between the `.js` and `.min.js` spellings** of
the same file, because a site keeps whichever variant its own build produced. Several URLs load in
order, so a plugin arrives after the library it extends.

`Tesserae.Require` (`LoadScriptAsync` / `LoadModuleAsync` / `LoadStyle`) still works and forwards
to exactly this.

## Gotchas

- `Script.Write` strings are opaque to the compiler — typos surface only at
  runtime. Pass C# values as `{n}` placeholders rather than interpolating them
  into the string so they compile correctly under minification.
- Property/feature checks: `Script.Write<bool>("(typeof {0}.disabled === 'undefined')", el)`.
- A Debug build and a Release build link different variants of a bundle (`x.js` vs `x.min.js`);
  any external script you call must be bundled so the global exists at runtime — see
  `wrap-a-javascript-library` — and fetched through `Require.RequireAsync`, which resolves the
  variant difference for you.

## Related

- `wrap-a-javascript-library` — bundle and wrap a whole JS library as a component.
- `creating-a-component` — where interop code usually lives (inside `Render`/ctor).
- Docs: `/tesserae/extending/javascript-interop`
