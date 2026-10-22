# Micro-interactions: what TypeUI shows, and where it lives in Tesserae

A review of the 113 interactions catalogued at <https://www.typeui.sh/micro-interactions> (11
categories), each mapped to one of:

- **Exists** — Tesserae already does it; the cell names the component or method.
- **New** — added with this change (see the list at the end).
- **App** — not a toolkit concern: the motion is ordinary, and what the example demonstrates is
  application logic (a total recalculated from a selection, a matrix of permissions). It is built by
  composing existing components, often with one of the new pieces for the feedback.
- **Later** — a reasonable addition that was left out of this change; the reason is given.

This classification is from each example's name, its labels and its resting state, as found in the
site's server-rendered markup. It is not from the motion itself: the site's scripts, which run the
interactions, could not be loaded from the environment this was written in (every script and
stylesheet request was refused by the host's bot protection), and the implementation prompts are
paywalled. The new components follow the names; check them against the live examples.

## Buttons (24)

| Interaction | Status | Where |
|---|---|---|
| Denied action shake | New | `.Shake()` |
| Toggle reaction state | New | `AnimatedNumber.Tick()` + `.Pop()` |
| Swap state icon | Exists | `IconToggle`, `Button.SetIcon` |
| Reveal secondary actions | Exists | `ActionButton`, `ContextMenu`, `Menu` |
| Hold to confirm | New | `HoldButton` |
| Fluid action button | New | width morph in `ConfirmButton` / `CopyButton` |
| Copy feedback | New | `CopyButton` |
| Confirm reveal | New | `ConfirmButton` |
| Action slide-out | New | `ConfirmButton` (label morphs to the done text) |
| Roll reveal | Exists | `Expander` / `Accordion` |
| Wall reveal | Exists | `Expander`, `Popover` |
| Slide-off reveal | Exists | `Expander`, `ProgressIndicator` |
| State morph | Exists | `Button.OnClickSpinWhile` with `ButtonSpinnerOutcome` |
| Refresh spin | New | `.SpinOnce()` |
| Undo countdown | New | `Toast.Countdown()` + `Banner.Action("Undo", …)` |
| Bounce dots | Exists | `Spinner` (a ring, not dots: a loader shape is a theme decision) |
| Dual ring | Exists | `Spinner`, `ProgressRing` |
| Bar wave | Exists | `Spinner`, `ProgressIndicator.Indeterminated()` |
| Count ramp | New | `AnimatedNumber` |
| Fill sweep | New | `Button.SetProgress(percent)` |
| Upload progress | New | `Button.SetProgress` + label |
| Multi-file upload | Exists | `FileDropArea` + `ProgressIndicator` per row |
| Slide bar | Exists | `ProgressIndicator.Indeterminated()` |
| Shimmer | New / Exists | `.Shimmer()` on real content; `Skeleton` as a placeholder |

## Inputs (23)

| Interaction | Status | Where |
|---|---|---|
| Reveal toggle (password) | Later | needs an adornment slot in `Input`'s container; worth its own change |
| Copy confirm | New | `CopyButton` beside a read-only `TextBox` |
| Prefix reveal / Suffix fade | Later | same adornment slot as the password toggle |
| Confirm delayed state (autosave) | Exists | `SavingToast`, `SaveButton` |
| Clear fade | Exists | `SearchBox` clear button |
| Segment fill | New | `CodeInput` |
| Auto-grow | Exists | `TextArea.AutoResize()` |
| Counter tick | New | `AnimatedNumber.Tick()` bound to the length |
| Ring drain | Exists | `ProgressRing` bound to the length |
| Strength fill | Exists | `ProgressIndicator` / `ContributionBar` |
| Count spring | New | `AnimatedNumber` + `.Pop()` |
| Chip pop-in | Exists | `TagsInput` |
| Toolbar reveal | New | `.RevealOnHoverOf(host)` (focus counts as hover) |
| Digit grouping / Mask fill | Later | an input mask is a feature of its own, not a motion |
| Build derived value (slug) | App | `TextBox.OnInput` → `TextBlock` |
| Filter triggered suggestions | Exists | `AutoCompleteBox`, `OmniBox` |
| Inline validation | Exists | `Validator`, `TextBox.Error` / `IsInvalid` |
| Invalid submit shake | New | `.Shake()` on the field |
| Offer inline correction | App | `TextBox.Error` + a `Button.Link()` |
| Confirm matching values | Exists | `Validator` |
| Reveal modifier warning (Caps Lock) | Later | small, but belongs with the password toggle |

## Selection controls (21)

Every one of these is a checkbox, toggle or radio driving something else. The controls exist
(`CheckBox`, `Toggle`, `ChoiceGroup`, `IconToggle`, `SegmentedPivot`); what changes is the app's.

| Interaction | Status | Where |
|---|---|---|
| Reveal contextual actions | App | `CommandBar` shown on selection; `.RevealOnHoverOf` for hover |
| Recalculate derived value | New | `AnimatedNumber` (the sample does exactly this) |
| Select-all cascade | Exists | `DetailsList` selection, `Tree` |
| Animate completed item | New | `.Pop()` / `.Flash()` on the row |
| Filter chips | Exists | `SegmentedPivot`, `Badge`, `ToggleButton` |
| Enable dependent action | App | `Button.Disabled(...)` bound to a `CheckBox` |
| Update derived progress | New | `AnimatedNumber` / `ProgressIndicator` |
| Enforce selection threshold | App | + `.Shake()` on a refused pick |
| Update selection summary | App | bound `TextBlock` |
| Reveal dependent content / settings | Exists | `Expander`, `.Collapse()` / `.Show()` |
| Icon toggle group | Exists | `IconToggle`, `SegmentedPivot` |
| Toggle visual layer | App | |
| Change display density | Exists | `DetailsList.Compact()` |
| Swap dependent timeline | App | |
| Update masked preview | App | |
| Update linked values | New | `AnimatedNumber` |
| Move shared item | Exists | `SortableStack`, `TaskBoard` |
| Swap dependent controls | App | |
| Update option matrix | App | `DetailsList` |
| Rebuild dependent steps | Exists | `Stepper` |

## Cards (6)

| Interaction | Status | Where |
|---|---|---|
| Flip card | New | `FlipCard` |
| Expandable | Exists | `Expander`, `Accordion` |
| Action reveal | New | `.RevealOnHoverOf(card)` |
| Swipe to dismiss | New | `.SwipeToDismiss(...)` |
| Stacked deck cycle | Later | a `Carousel` variant; niche |
| Shared element expand | Later | needs view transitions across containers |

## Drawers (4)

| Interaction | Status | Where |
|---|---|---|
| Edge swipe open | Later | `Drawer` opens from code; an edge gesture would be an addition there |
| Drag-linked backdrop | Exists | `Drawer` drag-to-dismiss (`NoDragToDismiss()` turns it off) |
| Expand shared surface | Later | as shared element expand |
| Collapse persistent panel | Exists | `Sidebar`, `SplitView`, `DockPanel` auto-hide |

## Lists (7)

| Interaction | Status | Where |
|---|---|---|
| Accordion | Exists | `Accordion` |
| Move between groups / Drag to reorder | Exists | `TaskBoard`, `SortableStack` |
| Pin item to top | New | `.Flash()` where it landed (sample shows it) |
| Archive item with undo | New | `.SwipeToDismiss` / `.DismissWithSwipe` + `Toast.Countdown()` |
| Insert new list item | New | `.Flash()` |
| Preview selected item | Exists | `SplitView` + `ItemsList` |

## Modals (7)

| Interaction | Status | Where |
|---|---|---|
| Drag down to dismiss | Exists | `Drawer` (bottom sheet) |
| Backdrop-click nudge | New | `.Nudge()` |
| Error shake | New | `.Shake()` |
| Confirm completed action | Exists | `Button.OnClickSpinWhile` outcome |
| Gate exact-match action | App | `TextBox.OnInput` → `Button.Disabled` |
| Hold to trigger action | New | `HoldButton` |
| Block unsafe dismissal | Exists | `UnsavedChangesGuard` |

## Navigation / steps (4)

| Interaction | Status | Where |
|---|---|---|
| Stepper progress / Return to completed step | Exists | `Stepper` |
| Block invalid advance | New | `.Shake()` on the field + `Stepper` |
| Update remaining time | New | `AnimatedNumber` |

## Sliders (11)

| Interaction | Status | Where |
|---|---|---|
| Drag thumb / Stepped snap | Exists | `Slider`, `StepsSlider` |
| Tooltip thumb / Tick highlights / Magnetic detents | Later | `Slider` is a native range input; a value bubble and ticks need it measured and drawn — its own change |
| Dual-thumb range | Later | a range slider is a new input |
| Before / after compare | New | `CompareSlider` |
| Snap carousel | Exists | `Carousel` |
| Rubber band overdrag | Later | `Carousel` edge resistance |
| Scrubber preview | App | `TimeHistogramPicker` covers the time case |
| Update level icon | App | `Slider.OnInput` → icon |

## Toasts (3)

| Interaction | Status | Where |
|---|---|---|
| Undo countdown toast | New | `Toast.Countdown()` |
| Promise toast morph | Exists | `SavingToast` |
| Swipe to dismiss toast | Later | `.SwipeToDismiss` works on any element, but a toast is a layer with its own positioning; wiring it in belongs in `Toast` |

## Tooltips (3)

| Interaction | Status | Where |
|---|---|---|
| Hover card / Link preview | Exists | `.Tooltip(IComponent, interactive: true)`, `Popover` |
| Warm delay group | Later | Tippy's `createSingleton` would give it; not wired up |

## What this change adds

Components (each with a sample and a skill reference):

| Component | Gallery group | Covers |
|---|---|---|
| `HoldButton` | Buttons & Commands | hold to confirm, hold to trigger |
| `ConfirmButton` | Buttons & Commands | confirm reveal, fluid action button, action slide-out |
| `CopyButton` | Buttons & Commands | copy feedback, copy confirm |
| `AnimatedNumber` | Text & Content | count ramp, counter tick, count spring, recalculated values |
| `FlipCard` | Layout | flip card |
| `CompareSlider` | Media & Graphics | before / after compare |
| `CodeInput` | Inputs | segment fill |

Extension methods on any `IComponent` (`MicroInteractionExtensions`, sample "Micro Interactions"
under Utilities & Behaviors): `Shake`, `Pop`, `Nudge`, `SpinOnce`, `Flash`, `PressFeedback`,
`Shimmer`, `RevealOnHoverOf`, `SwipeToDismiss`, `DismissWithSwipe`.

On existing components: `Button.SetProgress` / `ClearProgress` (fill sweep, upload progress) and
`Toast.Countdown` (undo countdown).

One behaviour change in `GestureRecognizer`: a press that starts on a button, link or field nested
inside a gesture element is no longer captured until it becomes a pan, so the nested control gets
its click. Before, a `.OnTapped` header or any `.OnPan` surface swallowed the clicks of the controls
inside it. A press anywhere else is captured immediately, as before (a thin `SplitView` handle
depends on that).

Everything honours `prefers-reduced-motion`: a shake becomes a red ring, a flip a crossfade, and the
rest stand still.
