# Micro-interactions: style or script

A review of the 113 interactions at <https://www.typeui.sh/micro-interactions>, asking for each one how
it should be built in Tesserae: in **style alone**, in **style driven by one class (or variable) set from
C#**, or with **script** doing part of the motion. It also records where each one lives in Tesserae
today.

## What the source shows

The review is from a saved copy of the page (MHTML), which holds the markup of all 113 demos and the
site's stylesheets, but not its scripts; the live site refuses script and stylesheet requests from
automated browsers. That is enough to see how each demo is built:

- **Every motion on the page is script-driven.** The stylesheets carry colours, hover and active
  backgrounds, focus rings, a 0.2s colour transition on the theme preview and one reduced-motion rule
  that zeroes every duration. Everything that moves is an animation library writing inline styles, and
  the markup is frozen at each demo's start state: the second icon of *Swap state icon* waits at
  `opacity:0; transform:scale(0.6) rotate(90deg)`, the hidden labels of *Confirm reveal* at
  `opacity:0; transform:translateX(16px)`, *Action reveal*'s toolbar at `translateY(100%)`,
  *Stacked deck cycle*'s back cards at `translateY(-12px) scale(0.95)` and `translateY(-24px) scale(0.9)`.
- **The shapes are simple.** Most demos are two or three states of one element (stacked labels or
  icons, a bar's `scaleX`, a clip-path inset, a panel's `translateY`), and the states are visible in the
  markup. What the script adds is mostly *sequencing*: when to go to the next state.

TypeUI reaches for script by default because its demos are React components. Tesserae is in a
different position, and that changes the answer.

## The rule for Tesserae

**1. Style** — the state is one CSS can already see: `:hover`, `:focus-within`, `:focus-visible`,
`:active`, `:has(...)`, or an attribute the component maintains anyway (`aria-expanded`,
`aria-checked`, `aria-pressed`, a state class). The stylesheet does all of it; no code runs.

**2. Style + a class flip** — the trigger is something only code knows (an action was refused, a copy
succeeded, a timer ran out, a value arrived), but the motion is between known states. C# sets or
removes one class, or writes one CSS variable (a percentage, a position); keyframes, durations and
easing stay in the stylesheet.

**3. Script** — the motion follows the pointer continuously (drag, swipe, a compare divider, a slider
thumb, rubber-banding), a value is computed every frame (counting through numbers), or layout has to be
read before it can be animated (reordering with FLIP, a shared element moving between containers).
Even here the script writes a transform or a variable and leaves timing to CSS where it can.

Prefer the lowest tier that works. The reasons are Tesserae's, not taste:

- **Themes restyle by CSS.** A motion in the stylesheet can be retimed or switched off by a theme
  (RetroOS opens an Expander in one step, as its windows did); a motion in C# cannot be reached by one.
- **Reduced motion is one media query** when the motion is CSS, and has to be remembered in every code
  path when it is not.
- **It costs nothing at rest.** A transition runs on the compositor and only when the state changes; no
  listener sits on every element of an application (the reasoning behind "Scrollbars are CSS only" in
  `CLAUDE.md`).
- **It reaches consumers' components.** A class or an attribute selector applies to a component written
  in an application as much as to one written here.

Two script habits that look necessary and are not:

- **Measuring a height to animate it.** TypeUI's *Accordion* demo makes the point itself ("Why measure
  the height? A hardcoded max-height eases wrongly…"). A one-row grid whose track goes from `0fr` to
  `1fr` animates to the content's real height with nothing measured, and stays right when the content
  grows. `Expander` (and so `Accordion`) now does this.
- **Measuring a width to morph a label.** Putting every label of a control in one grid cell makes the
  control as wide as its longest label, so a swap needs a crossfade and nothing else. TypeUI's
  *Confirm reveal* is built that way (a fixed-width button over three stacked labels); `ConfirmButton`
  and `CopyButton` now are too.

## The 113, classified

Column **Tier**: 1 = style, 2 = style + class flip / variable, 3 = script. Column **Tesserae**:
*Exists* (already there), *New* (added on this branch), *App* (application logic around existing
components; the tier is still given), *Later* (worth adding, not done; reason given).

### Buttons (24)

| Interaction | TypeUI's markup | Tier | Tesserae |
|---|---|---|---|
| Denied action shake | primary button, lock icon | 2 | New: `.Shake()` (keyframes; class added on refusal) |
| Toggle reaction state | count in a 14px clipped box, `aria-pressed` | 2 | New: `AnimatedNumber.Tick()` + `.Pop()` |
| Swap state icon | two icons stacked, hidden one at `scale(.6) rotate(90deg)` | 1 | Exists: `IconToggle`; the rotate-and-scale swap is a CSS change keyed on its pressed state (Later) |
| Reveal secondary actions | `aria-expanded` button, menu above it | 1 | Exists: `ActionButton`, `ContextMenu` |
| Hold to confirm | bar (`scaleX`) and percentage above the button | 2 | New: `HoldButton` — fill is a CSS transition over the hold time; C# only handles press, release and keyboard |
| Fluid action button | label split into per-letter spans | 2 | Later: per-letter stagger is CSS (`transition-delay` per letter) once the label is split |
| Copy feedback | icon + label | 2 | New: `CopyButton` (icons and labels stacked; one class) |
| Confirm reveal | three labels stacked, hidden at `translateX(16px)` | 2 | New: `ConfirmButton` (same construction) |
| Action slide-out | invisible label sizes the button, real one absolute | 2 | Covered by `ConfirmButton`'s label stack |
| Roll reveal | column of two labels, `translateY` | 2 | Later: a vertical label stack (same pieces, `translateY`) |
| Wall reveal | button over a panel, list rows at `translateX(10px)` | 2 | Exists: `Expander`; staggered rows are `transition-delay` |
| Slide-off reveal | button slides off a usage bar | 2 | App: `Button` over a `ProgressIndicator` |
| State morph | label swap on a primary button | 2 | Exists: `Button.OnClickSpinWhile` with an outcome |
| Refresh spin | icon turns | 2 | New: `.SpinOnce()` |
| Undo countdown | danger button, then a toast | 2 | New: `Toast.Countdown()` (a CSS animation the length of the toast) |
| Bounce dots / Dual ring / Bar wave | label swapped for a loader | 1 | Exists: `Spinner`, `ProgressIndicator.Indeterminated()` (loops are keyframes) |
| Count ramp | percentage counted in the button | 3 | New: `AnimatedNumber` (counting is per frame) |
| Fill sweep | inverted copy of the label clipped by `clip-path: inset(0 X% 0 0)` | 2 | New: `Button.SetProgress()` (C# writes the percentage, CSS moves the fill); the inverted-label clip is Later |
| Upload progress | three stacked labels + a bar | 2 | New: `Button.SetProgress()` + label |
| Multi-file upload | panel growing out of the button, rows sliding in | 2 | App: `FileDropArea` + `ProgressIndicator` rows |
| Slide bar / Shimmer | a bar or a sheen across the button | 1 | Exists: `ProgressIndicator`; New: `.Shimmer()` |

### Inputs (23)

| Interaction | TypeUI's markup | Tier | Tesserae |
|---|---|---|---|
| Reveal toggle | eye button inside the field | 2 | Later: needs an adornment slot in `Input`'s container |
| Copy confirm | copy button inside a read-only field | 2 | New: `CopyButton` beside a read-only `TextBox` |
| Prefix reveal | "@" slot at `width:0; opacity:0` | 1 | Later: the slot can open on `:focus-within` / `:has(input:not(:placeholder-shown))`, no script |
| Suffix fade | unit inside the field | 1 | Later: same adornment slot, `:placeholder-shown` |
| Confirm delayed state | status text inside the field | 2 | Exists: `SavingToast`, `SaveButton` |
| Clear fade | clear button inside a search field | 1 | Exists: `SearchBox` (shown when there is text) |
| Segment fill | one input per digit | 2 | New: `CodeInput` (one real input under the cells; the pop is a class) |
| Auto-grow | textarea in a box with an explicit height | 3 | Exists: `TextArea.AutoResize()` (`field-sizing: content` would make it tier 1 where supported) |
| Counter tick | "0/80" under the field | 2 | New: `AnimatedNumber.Tick()` fed the length |
| Ring drain | svg ring in the field | 2 | Exists: `ProgressRing` fed the length |
| Strength fill | four segments, each `scaleX` | 2 | Exists: `ProgressIndicator`, `ContributionBar` |
| Count spring | word count under the field | 2 | New: `AnimatedNumber` + `.Pop()` |
| Chip pop-in | chips at `opacity:1; transform:none` after entering | 2 | Exists: `TagsInput` |
| Toolbar reveal | toolbar over a textarea, focus ring on `:focus-within` | 1 | New: `.RevealOnHoverOf(host)` (hover and focus, pure CSS) |
| Digit grouping / Mask fill | monospaced field | 3 | Later: an input mask is a feature, not a motion |
| Build derived value | slug under the title | 2 | App: bound `TextBlock` |
| Filter triggered suggestions | "@" opens a list | 3 | Exists: `AutoCompleteBox`, `OmniBox` |
| Inline validation | status icon slot in the field | 2 | Exists: `Validator`, `TextBox.IsInvalid` |
| Invalid submit shake | field + submit | 2 | New: `.Shake()` on the field |
| Offer inline correction | "Did you mean…" + button, fades in | 2 | App: `TextBox.Error` + `Button.Link()` |
| Confirm matching values | status slot in the second field | 2 | Exists: `Validator` |
| Reveal modifier warning | Caps Lock badge in the field | 2 | Later: needs the adornment slot; the key state comes from a key event |

### Selection controls (21)

All 21 are a checkbox, toggle or radio driving something else. The controls exist (`CheckBox`,
`Toggle`, `ChoiceGroup`, `IconToggle`, `SegmentedPivot`); what changes around them is the
application's. The markup shows the same few motions throughout, and their tiers:

| Interaction | TypeUI's markup | Tier | Tesserae |
|---|---|---|---|
| Reveal contextual actions | bar appears under a list once items are checked | 2 | App: `CommandBar` shown on selection |
| Recalculate derived value / Update linked values / Update derived progress | total, days and credits, or a `scaleX` bar | 2–3 | New: `AnimatedNumber`; `ProgressIndicator` |
| Select-all cascade | indeterminate parent checkbox | 2 | Exists: `DetailsList` selection, `Tree` |
| Animate completed item | strike-through line at `scaleX(0)` → `none` | 1 | Later: a `CheckBox`-driven strike is pure CSS with `:has(:checked)` |
| Filter chips | pill buttons | 1 | Exists: `SegmentedPivot`, `ToggleButton` |
| Enable dependent action | button disabled until checked | 1 | App: `Button.Disabled(...)` |
| Enforce selection threshold | counter + disabled next | 2 | App: + `.Shake()` on a refused pick |
| Update selection summary | "Selected: Email" | 2 | App |
| Reveal dependent content / settings | section under a checkbox or toggle | 1 | Exists: `Expander` (now animates in CSS) |
| Icon toggle group | pressed background `opacity:1; transform:none` | 1 | Exists: `IconToggle`, `SegmentedPivot` |
| Toggle visual layer / Change display density / Update masked preview | knob `transform`, padding change, blur | 1 | Exists: `Toggle` (knob transition); `DetailsList.Compact()` |
| Swap dependent timeline | timeline line `scaleY(0)` | 2 | App: `Timeline` |
| Move shared item | badge moves between rows | 3 | App: needs FLIP (read both positions) |
| Swap dependent controls / Update option matrix | content swapped by a radio | 2 | App |
| Rebuild dependent steps | steps absolutely placed at `top:0` / `48px` | 2 | Exists: `Stepper` |

### Cards (6)

| Interaction | TypeUI's markup | Tier | Tesserae |
|---|---|---|---|
| Flip card | `perspective:800px`, `preserve-3d`, back at `rotateY(180deg)` | 2 | New: `FlipCard` (one class; the turn is a transition) |
| Expandable | card with a chevron | 1 | Exists: `Expander` |
| Action reveal | toolbar at `opacity:0; translateY(100%)`, "hover, focus or tap" | 1 | New: `.RevealOnHoverOf(card)` |
| Swipe to dismiss | card follows the pointer, Left/Right hints fade in | 3 | New: `.SwipeToDismiss()` (drag is script; the fling-off and collapse are CSS transitions) |
| Stacked deck cycle | back cards at `translateY(-12px) scale(.95)`, `(-24px) scale(.9)` | 2 | Later: positions are per-index classes; cycling moves the classes |
| Shared element expand | card grows into a detail view | 3 | Later: needs a View Transition or FLIP |

### Drawers (4)

| Interaction | TypeUI's markup | Tier | Tesserae |
|---|---|---|---|
| Edge swipe open | drawer at `translateX(-224px)`, grab handle | 3 | Later: `Drawer` opens from code; an edge drag would be an addition there |
| Drag-linked backdrop | backdrop opacity tied to the drag | 3 | Exists: `Drawer` drag-to-dismiss |
| Expand shared surface | bottom sheet grows into a page | 3 | Later: as shared element expand |
| Collapse persistent panel | 144px aside, labels fade | 2 | Exists: `Sidebar`, `SplitView`, `DockPanel` |

### Lists (7)

| Interaction | TypeUI's markup | Tier | Tesserae |
|---|---|---|---|
| Accordion | content at `height:auto` (measured) | 1 | Exists: `Accordion` — now animated in CSS (`0fr`→`1fr`), no measuring |
| Move between groups / Drag to reorder | rows with move buttons, `touch-action:pan-x` | 3 | Exists: `TaskBoard`, `SortableStack` |
| Pin item to top | pin button per row | 3 | New: `.Flash()` on the row where it landed (the move itself would be FLIP) |
| Archive item with undo | rows at an explicit `height:48px` | 2 | New: `.DismissWithSwipe()` collapses the row; `Toast.Countdown()` for the undo |
| Insert new list item | rows at `opacity:1; height:48px; transform:none` after entering | 2 | New: `.Flash()` |
| Preview selected item | accent bar on the selected row, panel below | 1 | Exists: `ItemsList` + `SplitView` |

### Modals (7)

| Interaction | TypeUI's markup | Tier | Tesserae |
|---|---|---|---|
| Drag down to dismiss | bottom sheet | 3 | Exists: `Drawer` |
| Backdrop-click nudge | blocking modal | 2 | New: `.Nudge()` |
| Error shake | modal with a field | 2 | New: `.Shake()` |
| Confirm completed action | modal button resolves | 2 | Exists: `Button.OnClickSpinWhile` outcome |
| Gate exact-match action | type the name to enable | 1 | App: `TextBox.OnInput` → `Button.Disabled` |
| Hold to trigger action | hold inside the modal | 2 | New: `HoldButton` |
| Block unsafe dismissal | unsaved changes | 2 | Exists: `UnsavedChangesGuard` |

### Step navigation (4)

| Interaction | TypeUI's markup | Tier | Tesserae |
|---|---|---|---|
| Stepper progress / Return to completed step | connector line `scaleX(0)` → 1 | 1 | Exists: `Stepper` |
| Block invalid advance | required field, next blocked | 2 | New: `.Shake()` + `Stepper` |
| Update remaining time | "2 min", `opacity`/`transform` on change | 2 | New: `AnimatedNumber` |

### Sliders (11)

| Interaction | TypeUI's markup | Tier | Tesserae |
|---|---|---|---|
| Drag thumb / Stepped snap | custom track and thumb, focus halo in CSS | 1 | Exists: `Slider` (native range), `StepsSlider` |
| Tooltip thumb | bubble over the thumb | 2 | Later: needs the value as a CSS variable (`Slider` would set `--tss-slider-pct` on input); the bubble is then CSS |
| Tick highlights | ticks lit up to the value | 2 | Later: same variable; ticks compare against it |
| Dual-thumb range | two thumbs, filled span between | 3 | Later: a range slider is a new input |
| Before / after compare | after layer clipped `inset(0 50% 0 0)` | 3 | New: `CompareSlider` (drag is script; the clip reads a CSS variable) |
| Snap carousel | strip translated, dots scale 1.2 | 3 | Exists: `Carousel` |
| Rubber band overdrag | thumb pulled past the end | 3 | Later: `Carousel` edge resistance |
| Scrubber preview | timeline with a hover preview | 3 | App: `TimeHistogramPicker` covers time |
| Magnetic detents | hidden real thumb, visual thumb snaps | 3 | Later: snapping logic |
| Update level icon | icon swapped by value | 2 | App: `Slider.OnInput` → icon |

### Toasts (3)

| Interaction | TypeUI's markup | Tier | Tesserae |
|---|---|---|---|
| Undo countdown toast | toast with an undo action | 2 | New: `Toast.Countdown()` |
| Promise toast morph | one toast changing state | 2 | Exists: `SavingToast` |
| Swipe to dismiss toast | stack bottom-right | 3 | Later: `.SwipeToDismiss` works on any element, but a toast is a positioned layer; wiring it belongs in `Toast` |

### Tooltips (3)

| Interaction | TypeUI's markup | Tier | Tesserae |
|---|---|---|---|
| Hover card / Link preview | inline trigger, card on hover or focus | 2 | Exists: `.Tooltip(component, interactive: true)` (Tippy positions it) |
| Warm delay group | toolbar of icon buttons | 3 | Later: Tippy's `createSingleton` shares the delay |

## What changed in Tesserae

New components, each with a gallery sample and a skill reference: `HoldButton`, `ConfirmButton`,
`CopyButton`, `AnimatedNumber`, `FlipCard`, `CompareSlider`, `CodeInput`. New extensions on any
`IComponent` (sample "Micro Interactions"): `Shake`, `Pop`, `Nudge`, `SpinOnce`, `Flash`,
`PressFeedback`, `Shimmer`, `RevealOnHoverOf`, `SwipeToDismiss`, `DismissWithSwipe`. On existing
components: `Button.SetProgress` / `ClearProgress`, `Toast.Countdown`.

Following the rule above:

- `Expander` (and `Accordion`) opens and closes with an animated height, in CSS only. Closed content is
  `visibility: hidden` once the close finishes, so it leaves the tab order and the accessibility tree as
  before. RetroOS keeps it instant.
- `ConfirmButton` and `CopyButton` stack their labels in one cell instead of measuring and animating
  their width; they keep one width in every state and name themselves after the label they show.
- `AnimatedNumber.Tick()` slides the old value out as the new one comes in (two class flips).
- `GestureRecognizer` no longer captures a press that starts on a nested control until it becomes a
  pan, so buttons inside a swipeable card keep their clicks.

Everything honours `prefers-reduced-motion`: a shake becomes a red ring, a flip a crossfade, labels
crossfade without sliding, and the rest stand still.
