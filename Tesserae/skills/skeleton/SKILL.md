---
name: skeleton
description: An animated placeholder shape (line, circle, or rectangle) shown while content loads. Use when reserving layout space for loading content in a Tesserae (C#/h5) app.
---

# Skeleton

A grey, animated placeholder used to reserve space for content that is still
loading, reducing layout shift. Comes in three shapes: `Line` (default),
`Circle` (avatars), `Rect` (images/blocks).

## Create

`UI.Skeleton(SkeletonType type = SkeletonType.Line)` (i.e. `Skeleton()` /
`Skeleton(SkeletonType.Circle)`) returns a `Skeleton`. Size it with the usual
sizing helpers (`.W()`, `.H()`, `.WS()`). Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

- `SkeletonType` — `Line`, `Circle`, `Rect` (constructor arg, or `.Type(t)`).
- `.Animated(bool = true)` — toggle the shimmer animation (on by default).
- `.Background(color)` — override the placeholder colour.
- Sizing helpers (`.W()`, `.H()`, `.WS()`, `.ML()`, `.MT()`, ...) set the box.

## Example

```csharp
using static Tesserae.UI;

// Avatar + two text lines
var loadingRow = HStack().Children(
    Skeleton(SkeletonType.Circle).W(48).H(48),
    VStack().Children(
        Skeleton().W(200).H(16).ML(16).MB(8),
        Skeleton().W(140).H(12).ML(16)));

// Image block + paragraph lines
var loadingArticle = VStack().Children(
    Skeleton(SkeletonType.Rect).WS().H(200),
    Skeleton().WS().H(16).MT(16),
    Skeleton().W(80.percent()).H(16).MT(8));
```

## Related

- Spinner (indeterminate loading) — `../spinner/SKILL.md`
- Full docs & API: `/tesserae/components/skeleton`
