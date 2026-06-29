---
name: carousel
description: A horizontal slideshow that cycles through one slide at a time with prev/next arrows and pagination dots; any component can be a slide. Use for image galleries, feature highlights, or onboarding sequences in a Tesserae (C#/h5) app.
---

# Carousel

`Carousel` shows one slide at a time and provides arrow controls plus clickable indicator
dots. Slides can be any `IComponent`. Give it a fixed height (`.H(...)`) so slides have
room.

## Create

`UI.Carousel(params IComponent[] slides)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.AddSlide(IComponent)` / `.AddSlides(params IComponent[])` — add slides.
- `.PadSlides()` — add inner padding to each slide track cell.
- `.SetIndex(int index, bool raiseEvent = true)` / `.Next()` / `.Previous()` — navigation.
- `.CurrentIndex` — get/set the active slide.
- `.SlideCount` — number of slides.
- `.OnSlideChange(Action<Carousel>)` — fired on slide change.

## Example

```csharp
using static Tesserae.UI;

var carousel = Carousel(
    VStack().P(32).Children(TextBlock("Discover").Large().Bold(),
        TextBlock("Build UIs in C#.")).WS(),
    VStack().P(32).Children(TextBlock("Fluent API").Large().Bold(),
        TextBlock("Typed and composable.")).WS()
).PadSlides().H(150);
```

## Related

- Image — `/tesserae/components/image`
- Full docs & API: `/tesserae/components/carousel`
