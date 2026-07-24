---
name: gestures
description: Fluent tap / double-tap / long-press / pan / pinch / rotate recognition on any IComponent, built on Pointer Events. Use when adding touch or pointer gestures to a Tesserae (C#/Transpose) app.
---

# Gestures

`GestureExtensions` adds gesture recognition to any `IComponent` via extension methods. One `GestureRecognizer` is attached per element and shared across all handlers, so DOM-listener overhead is fixed. Built on Pointer Events (mouse + touch + stylus). Pan/pinch/rotate set `touch-action: none` to avoid native scroll/zoom hijacking.

## Methods (extensions on `IComponent`)

Each takes either `Action<GestureState>` or a parameterless `Action`:

- `.OnTapped(handler)` — quick press-and-release (≤10px move).
- `.OnDoubleTapped(handler)` — two taps within 300ms; registering this delays single taps to disambiguate.
- `.OnLongPress(handler)` — stationary press held ≥500ms.
- `.OnPan(handler)` — drag; read per-event `DeltaX/DeltaY` and cumulative `OffsetX/OffsetY`.
- `.OnPinch(handler)` — two-finger scale; read `Scale` / `ScaleDelta`.
- `.OnRotate(handler)` — two-finger rotation in degrees; read `Rotation` / `RotationDelta`.

## GestureState fields

`Phase` (`GesturePhase.Start`/`Move`/`End`), `X`/`Y` (client coords or centroid), `DeltaX`/`DeltaY`, `OffsetX`/`OffsetY`, `Scale`/`ScaleDelta`, `Rotation`/`RotationDelta`, `PointerCount`, `Component`, `Event`. The instance is reused per recognizer — do not hold a reference across events.

## Example

```csharp
someComponent
    .OnTapped(s => Console.WriteLine($"tap at {s.X},{s.Y}"))
    .OnDoubleTapped(() => Console.WriteLine("double-tap"))
    .OnLongPress(s => ShowMenu(s.X, s.Y));

draggable.OnPan(s =>
{
    if (s.Phase == GesturePhase.Start) { /* reset */ }
    UpdatePosition(s.OffsetX, s.OffsetY);
});

canvas.OnPinch(s => ApplyScale(s.Scale));
```

## Related

- Full docs & API: `/tesserae/utilities/gestures`
