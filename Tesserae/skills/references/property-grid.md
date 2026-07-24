---
name: property-grid
description: A reflection-driven editor that auto-generates a two-way-bound form from a typed object's public properties. Use when you need a settings editor, model form, or debug inspector without writing per-property markup in a Tesserae (C#/Transpose) app.
---

# PropertyGrid

`PropertyGrid<T>` reflects over the bound object's public properties at render time and maps each to an input by declared type: `string` → TextBox/TextArea, `bool` → Toggle, `enum` → Dropdown, integer types → NumberPicker, `double`/`float`/`decimal` → numeric TextBox, `DateTime` → DateTimePicker, `Color` → ColorPicker, nested class/struct → a recursive `Expander` section. Edits write straight back to the object. Properties without a setter become read-only.

## Create

`UI.PropertyGrid<T>(T instance)`, or non-generic `UI.PropertyGrid(instance)` → `PropertyGrid<object>`. Also `new PropertyGrid<T>(instance)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

Fluent config (wins over attributes when both present):

- `.OnChange(Action<T>)` / `.AsObservable()` — observe edits (fires with the bound instance).
- `.Label(name, text)` / `.Description(name, text)` / `.Order(name, n)` — per-property display.
- `.ReadOnly(params string[] names)` — no args = whole grid read-only; names = only those fields.
- `.Ignore(params string[] names)` — hide properties.
- `.Multiline(name)` — render a string as a TextArea.
- `.Validate(name, Func<object,string>)` — return an error message (or null/empty when valid).
- `.WithValidator(Validator)` — wire validators into a shared `Validator`.

Equivalent attributes on the class: `[PropertyGridLabel]`, `[PropertyGridDescription]`, `[PropertyGridOrder(n)]`, `[PropertyGridReadOnly]`, `[PropertyGridIgnore]`, `[PropertyGridMultiline]`.

## Example

```csharp
using static Tesserae.UI;

class Settings
{
    public string Name { get; set; } = "Demo";
    public bool Enabled { get; set; } = true;
    public int Retries { get; set; } = 3;
}

var model = new Settings();
var grid = PropertyGrid(model)
    .Label(nameof(Settings.Retries), "Retry count")
    .OnChange(m => console.log(m.Name));
```

## Related

- Full docs & API: `/tesserae/components/property-grid`
