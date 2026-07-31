---
name: message
description: An inline informational message strip with an icon, title, body, optional note and tone variants (default/success/warning/error). Use for empty states, alerts and static status messages in a Tesserae (C#/Transpose) app.
---

# Message

A static message block with an icon, title, text body, and optional note and
illustration areas. Comes with tone variants for standard, success, warning and
error states.

## Create

`Message(string title = null, string message = null)` — title and body text are
optional and can also be set fluently. Bring the factory into scope with
`using static Tesserae.UI;`.

## Key configuration

- `.Icon(UIcons icon, string color = null, TextSize size = Large)` — leading icon (also accepts an `Image`).
- `.Title(string)` / `.Title(IComponent)` — title.
- `.Text(string)` / `.Text(IComponent)` — body text.
- `.Note(string)` / `.Note(IComponent)` — extra note area below the body.
- `.Illustration(IComponent)` — a picture below the content (drawing, mascot, animation), for empty states.
- `.Variant(MessageVariant)` — tone: `Default`, `Success`, `Warning`, `Error`.

## Example

```csharp
using static Tesserae.UI;

var empty = Message("No results found", "We couldn't find any items matching your search.")
    .Icon(UIcons.Search)
    .Variant(MessageVariant.Default)
    .Note(HStack().AlignItemsCenter().Children(
        Icon(UIcons.Bulb, size: TextSize.Small).PR(8),
        TextBlock("Try a broader query").SemiBold()
    ));

var error = Message("Something went wrong", "We couldn't save your changes.")
    .Icon(UIcons.CrossCircle)
    .Variant(MessageVariant.Error);

// An empty state whose picture sits below the text
var emptyList = Message().Text("Sessions you start will show up here")
    .Illustration(PixelAvatar(key, PixelAvatarDesign.Grey, PixelAvatarAnimation.SitIdle).PixelSize(8));
```

## Related

- Toast — `/tesserae/surfaces/toast`
- Components overview — `/tesserae/components/`
- Full docs & API: `/tesserae/components/message`
