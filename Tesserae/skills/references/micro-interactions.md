---
name: micro-interactions
description: Extension methods that add small feedback animations and behaviours to any component - Shake, Pop, Nudge, SpinOnce, Flash, PressFeedback, Shimmer, RevealOnHoverOf, SwipeToDismiss. Use when an action needs visible feedback (refused, toggled, inserted, loading, dismissed) in a Tesserae (C#/Transpose) app.
---

# Micro-interactions

Extension methods on `IComponent` (`MicroInteractionExtensions.cs`) for the small
motions that tell a user what just happened. They work on any component, the
toolkit's or your own. CSS lives in `tss.microinteractions.css`.

**One-shots** — call from an event handler at the moment the feedback is due;
they restart if already playing and leave nothing behind:

- `.Shake()` — a refused action, an invalid submit, a wrong code.
- `.Pop()` — scale up and settle with overshoot: a toggled reaction, a changed count, an added chip.
- `.Nudge()` — a small pulse: a blocking modal whose backdrop was clicked.
- `.SpinOnce()` — turns the component's first icon (or the element) once: a refresh.
- `.Flash()` — washes the background with the primary colour and fades: a row inserted, pinned or moved.

**Behaviours** — switched on and left on:

- `.PressFeedback(bool = true)` — a slight press-in (scale 0.97) while held.
- `.Shimmer(bool = true)` — a light band sweeps across while content loads in place (sets `aria-busy`).
- `.RevealOnHoverOf(IComponent host)` — hidden until the pointer is over `host` or focus is inside it; always shown on touch devices; never removed from the tab order.
- `.SwipeToDismiss(Action onDismissed, double threshold = 0.35, bool removeOnDismiss = true)` — drag sideways; past `threshold` of its width it slides off, collapses its height and calls back, otherwise springs back. Vertical scrolling keeps working on touch. Buttons inside it still get their clicks.
- `.DismissWithSwipe(Action onDismissed = null, bool toTheLeft = false, bool removeOnDismiss = true)` — plays that exit from code (a close button, a Delete key).

Under `prefers-reduced-motion` a shake becomes a brief red ring and the rest stand
still, so nothing is carried by motion alone. Pair a shake with a message saying
why, and keep a visible control beside any swipe.

## Example

```csharp
using static Tesserae.UI;

var save = Button("Publish").Primary();
save.OnClick(() =>
{
    if (!form.IsValid) { save.Shake(); return; }
    Publish();
});

var actions = HStack().Children(Button().SetIcon(UIcons.Download), Button().SetIcon(UIcons.Trash));
var card    = Card(TextBlock("report.pdf")).SetFooter(actions);
actions.RevealOnHoverOf(card);

notification.SwipeToDismiss(() => store.Dismiss(id));
```

## Components built on the same pieces

hold-button · confirm-button · copy-button · animated-number · flip-card ·
compare-slider · code-input. Also `Button.SetProgress(percent)` (a fill behind the
label, `button.md`) and `Toast.Countdown()` (a draining bar, `toast.md`).

## Related

- Gestures (`OnPan`, `OnLongPress`, ...) — `gestures.md`
- Skeleton, for placeholders instead of shimmering real content — `skeleton.md`
- Full docs & API: `/tesserae/utilities/micro-interactions`
