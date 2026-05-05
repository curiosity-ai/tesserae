# Tesserae Component Review & Improvement Plan

> Reviewed: May 2025  
> Scope: All components in `Tesserae/src/Components/` and all samples in `Tesserae.Tests/src/Samples/`

---

## Executive Summary

The library has ~90 components organised into Components, Collections, Surfaces, Progress, and Utilities. The architecture is solid: a generic `ComponentBase<T, THTML>` provides a consistent fluent API, observable reactivity, and DOM-event plumbing across the entire library. Samples are high quality and follow a consistent Overview → Best Practices → Usage structure.

The main gaps are:
- **Bugs**: one event-handling bug in `ComponentBase`, two `NotImplementedException` stubs in `ContextMenu`, a misspelling in `Nav`, and console.log debugging left in `VirtualizedList`.
- **Incomplete features**: vertical `Slider`, `ContextMenu.Render()`/`Show()` base stubs, `Picker` cache TODO.
- **Accessibility**: ARIA attributes are missing or incomplete across many interactive components.
- **Missing samples**: `Teaching` and `TutorialModal` have no samples; `StepsSlider`, `MonthPicker`, `WeekPicker`, `ProgressModal`, `Carousel` samples are thin or absent.
- **Missing components**: ~15 components that would round out a production UI library are absent entirely (see Section 4).

---

## 1. Bug Fixes (Critical – Act First)

### 1.1 `ComponentBase.OnMouseOver` – wrong event cleared
**File:** `Tesserae/src/Components/ComponentBase.cs` ~line 74

```csharp
// BUG: first loop removes MouseOut delegates, not MouseOver
foreach (Delegate d in MouseOver.GetInvocationList())
{
    MouseOut -= (ComponentEventHandler<T, MouseEvent>)d;  // ← wrong target
}
```

The first `foreach` should unsubscribe from `MouseOver`, not `MouseOut`. This means calling `OnMouseOver` with `clearPrevious: true` silently fails to clear previous enter handlers while still clearing leave handlers.

**Fix:** Change `MouseOut -=` to `MouseOver -=` in the first loop.

---

### 1.2 `ContextMenu.Render()` / `Show()` – stubs block inheritance contract
**File:** `Tesserae/src/Components/ContextMenu.cs`

`ShowFor(IComponent)` and `ShowAt(x, y, minWidth)` are fully implemented, but the base-class overrides `Render()` and `Show()` throw `NotImplementedException`. Any code that calls these via the `IComponent` interface or `Layer<T>` base will crash at runtime.

**Fix:** Implement `Render()` to return the popup container element (or throw a descriptive `InvalidOperationException` explaining that `ShowFor`/`ShowAt` must be used). Implement `Show()` to forward to a sensible default (e.g. `ShowAt(0, 0, 100)`) or throw with a clear message.

---

### 1.3 `Nav.UnselectRecursivelly` – typo in public API
**File:** `Tesserae/src/Components/Nav.cs` ~line 92

Method name is `UnselectRecursivelly` (double `l`). This is public API, so a rename to `UnselectRecursively` requires a deprecation alias during the transition.

---

### 1.4 `VirtualizedList` – debug console.log left in production code
**File:** `Tesserae/src/Components/VirtualizedList.cs` ~lines 286, 297, 314

Three `console.log` calls remain from development. Remove them.

---

## 2. Per-Component Improvements

### 2.1 Slider – fix and expose vertical mode
**File:** `Tesserae/src/Components/Slider.cs`  
**Sample:** `SliderSample.cs` (lines 41-47 are commented out with "TODO: Fix vertical sliders")

The vertical slider is fully commented out. Issues to fix:
- The CSS fake-progress div approach only updates on `InputUpdated`, not on programmatic `Value` changes. Extend the setter to also call the update function.
- The WebKit browser detection uses `userAgent.indexOf("WebKit")` which is unreliable. Use `@supports` CSS feature detection instead.
- ARIA attributes missing: add `role="slider"`, `aria-valuemin`, `aria-valuemax`, `aria-valuenow`, `aria-orientation`.

**New API to expose:**
```csharp
Slider().Vertical()       // already exists, just broken
Slider().ShowValue()      // display current value as a tooltip/label
Slider().Labels(...)      // tick marks / labels at defined positions
```

---

### 2.2 Dropdown – virtual scrolling & keyboard improvements
**File:** `Tesserae/src/Components/Dropdown.cs` (~1,190 lines)

- **Performance:** `RecomputePopupPosition` recalculates all item heights on every open. Cache item heights.
- **Large lists:** No virtual scrolling — loading 1,000+ items renders all DOM nodes at once. Add optional `VirtualScrolling()` opt-in that uses the same page-cache pattern as `VirtualizedList`.
- **Grouping:** Items can have headers and dividers but groups are not collapsible.
- **Keyboard:** Arrow-key navigation exists, but there is no `Home`/`End` key support and no `PageUp`/`PageDown`.
- **ARIA:** `aria-expanded`, `aria-haspopup="listbox"`, `aria-activedescendant`, `role="option"` on items are missing.
- **API consistency:** `SearchAsYouType` naming should be aligned with `SearchBox.SearchAsYouType`.

---

### 2.3 Picker – async loading & cache
**File:** `Tesserae/src/Components/Picker.cs`

- Line 212: cache for suggestion results is `TODO` — implement it to avoid redundant network calls.
- Debounce timeout hardcoded at 50ms (line 22); expose as `DebounceMs(int)`.
- No async item source support (`Func<string, Task<IPickerItem[]>>`); add it to match Dropdown's `LoadItemsAsync`.
- Suggestion-layer positioning breaks when the picker is near the bottom of the viewport; add viewport-edge detection matching the ContextMenu pattern.

---

### 2.4 Pagination – accessibility & keyboard nav
**File:** `Tesserae/src/Components/Pagination.cs`

- No keyboard navigation: wrapping `<nav>` with `role="navigation"` and `aria-label="Pagination"`, marking active page with `aria-current="page"`.
- The `GetPageNumbers` algorithm has hardcoded UI assumptions; extract to a pure, testable static method.
- Expose a `PageSizes(int[])` API so the user can switch items-per-page from the control itself (pattern used by most data grids).

---

### 2.5 DetailsList – inline editing & frozen columns
**File:** `Tesserae/src/Components/DetailsList.cs`

- No inline editing mode. Add an `Editable()` flag that swaps the render of a column from a read component to an edit component on click.
- No column pinning/freezing. This is the most commonly requested feature for data grids.
- Sort animation uses a magic 100ms delay (line 288); extract to a named constant.
- Column resizing by drag (resize handle) is absent; add a `Resizable()` column option.
- Export: add a `WithCsvExport()` / `WithExcelExport()` helper that serialises text values.

---

### 2.6 Timeline – clickable events & status indicators
**File:** `Tesserae/src/Components/Timeline.cs`

- Items are plain `IComponent`; there is no concept of an item being "current", "past", or "future" — add an `ITimelineItem` interface with `Status` (Pending / InProgress / Done / Error) rendered as a colour-coded dot/icon.
- Horizontal orientation is absent; add `Horizontal()`.
- No click handler at the timeline-item level; add `OnItemClick`.

---

### 2.7 Toast – deduplication API & progress toasts
**File:** `Tesserae/src/Components/Toast.cs`

- No "progress toast" pattern (a toast with a `ProgressIndicator` inside, updated from async code). This is heavily used in long-running operations.
- `ClearAll()` on line 515 uses `replaceChildren()` which is not available in all browser versions targeted; use `innerHTML = ""` or loop-remove as fallback.
- Expose `Toast().Unique("key")` that replaces a previous toast with the same key instead of stacking.

---

### 2.8 Tree – drag-and-drop reordering
**File:** `Tesserae/src/Components/Tree.cs`

- Drag-and-drop is available in `SortableStack` and `Nav` (via Sortable.js) but the `Tree` component has no drag support. Add `Sortable()` opt-in.
- Keyboard navigation: `ArrowRight` should expand, `ArrowLeft` collapse, `ArrowUp`/`Down` move between visible nodes — this is the standard tree keyboard pattern (ARIA `role="tree"`).
- `ARIA`: add `role="tree"`, `role="treeitem"`, `aria-expanded`, `aria-level`, `aria-selected`.

---

### 2.9 OmniBox – richer mode switching UX
**File:** `Tesserae/src/Components/OmniBox.cs`

- The mode indicator (Search vs Chat) is a subtle icon. Add an optional visible `ModePill` that the user can click to toggle, so the switching affordance is discoverable.
- Expose `MaxHistoryItems(int)` — currently history is loaded wholesale from the callback with no truncation.
- The logical-operator token renderer is not customisable; allow apps to provide custom token colours/labels for domain-specific operators.

---

### 2.10 Avatar – group/stack layout
**File:** `Tesserae/src/Components/Avatar.cs`

- Add an `AvatarGroup` component that overlaps 2-5 avatars with an overflow count bubble ("+3"), a common pattern for showing assignees or collaborators.
- The hash-based gradient is deterministic and correct; document it as a deliberate design (so developers don't "fix" it).

---

### 2.11 Badge / Tag / Chip – filtering integration
**File:** `Tesserae/src/Components/Badge.cs`

- Add a `TagGroup` container that manages selection state across multiple `Tag` instances — useful for filter bars.
- `Chip` has a remove callback but no `OnSelect` for toggle-style selection; add it.
- Expose `Badge().Count(int)` shorthand for numeric notification badges.

---

### 2.12 Chat – streaming & tool-use events
**File:** `Tesserae/src/Components/Chat.cs`

- The typewriter animation in `ChatSample` is hand-rolled with `window.setTimeout`. Promote this to a first-class `ChatMessage.StreamText(IAsyncEnumerable<string>)` API.
- Add a `ChatMessage.Thinking()` state (animated dots) to use before a streaming response arrives.
- Add an event system to `ChatArea`: `OnScrolledToTop` (for loading history) and `OnScrolledToBottom`.
- Auto-scroll threshold (5px, line 36-44) should be a configurable property.

---

### 2.13 Teaching / TutorialModal – missing samples
**Files:** `Tesserae/src/Components/Teaching.cs`, `Tesserae/src/Components/TutorialModal.cs`

Both components exist in the core library but have **no samples in the app**. Add:
- `TeachingSample.cs` demonstrating `NextButton`, `After5seconds`, and `After10seconds` step types, plus `RunIf`, `OnComplete`.
- `TutorialModalSample.cs` showing a multi-step modal wizard with forward/back navigation.

---

### 2.14 Sidebar – sample improvements
**File:** `Tesserae.Tests/src/Samples/Components/SidebarSample.cs`

- The current sample doesn't demonstrate: `SidebarPivot`, `SidebarSearchBox`, hierarchical group ordering, or the `Navbar` variant in context (a full-page layout). The `NavbarSample` is separate but minimal.
- Add a "Full Application Shell" composed sample showing sidebar + content area + navbar together.

---

### 2.15 Accordion – nested accordions & lazy content
**File:** `Tesserae/src/Components/Accordion.cs`

- Content of each expander is always rendered at mount time; add a `LazyContent(Func<IComponent>)` overload that renders content only when first opened.
- Add an `OnToggle(Action<bool>)` callback per expander.
- Sample doesn't show nested accordions (accordion inside an expander); add that example.

---

### 2.16 Stepper – non-linear flows & validation gating
**File:** `Tesserae/src/Components/Stepper.cs`

- Steps are purely sequential; add `AllowNonLinear()` to let the user jump to any visited step.
- Add `ValidateBeforeNext(Func<bool>)` to gate the Next button behind a validation check.
- Step indicators don't show state (completed ✓, error ✗, current ●); add `IStepStatus`.

---

### 2.17 Metric – sparkline range & comparison
**File:** `Tesserae/src/Components/Metric.cs`

- The embedded `Sparkline` has no tooltip on hover — add crosshair hover with value tooltip.
- Add `Metric.ComparisonValue` that shows a second bar/number for period-over-period comparison.
- Add `Metric.Goal(double)` to render a target line on the sparkline.

---

### 2.18 NodeView – save/load UX & read-only mode
**File:** `Tesserae/src/Components/NodeView/NodeView.cs`

- The sample shows JSON state round-tripping via a `TextArea` but there is no built-in Save button or auto-save debounce. Add `AutoSave(Func<string, Task>)`.
- No read-only mode; add `ReadOnly()` that disables dragging, connection editing, and node inputs.
- Mini-map/overview panel is absent for large graphs; consider wrapping the BaklavaJS minimap plugin.

---

### 2.19 CronEditor – raw-input sync & validation feedback
**File:** `Tesserae/src/Components/CronEditor.cs`

- Switching between the simple UI and the raw cron input does not validate the cron expression on blur; add inline validation with a clear error message.
- Add a human-readable summary ("Runs every Monday at 9:00 AM") below the raw input.
- Expose `CronEditor.WithSeconds()` for systems that use 6-part cron expressions.

---

### 2.20 GridPicker – keyboard selection
**File:** `Tesserae/src/Components/SchedulePicker/GridPicker.cs`

- Click-and-drag selection works, but there is no keyboard selection (Tab to focus cell, Space to toggle). Add it.
- State cycling direction is always forward; add `CycleBackward(bool)` option for cases where there are many states.

---

## 3. Missing Samples (Components that Exist but Have No Sample)

| Component | File | Suggested Sample Content |
|-----------|------|--------------------------|
| `Teaching` | `Teaching.cs` | 3-step walkthrough with NextButton, After5seconds step types, condition gating |
| `TutorialModal` | `TutorialModal.cs` | Multi-step wizard modal with prev/next, finish handler |
| `StepsSlider` | `StepsSlider.cs` | Step-labelled slider (e.g. T-shirt sizes S/M/L/XL) |
| `MonthPicker` | `MonthPicker.cs` | Month-only date picker with min/max |
| `WeekPicker` | `WeekPicker.cs` | ISO week picker |
| `TimePicker` | `TimePicker.cs` | Time-only input with AM/PM toggle |
| `ProgressModal` | `ProgressModal.cs` | Full-screen progress overlay for long operations |
| `Carousel` | `Carousel.cs` | Image/content carousel with autoplay, dots indicator |
| `SortableStack` | `SortableStack/SortableStack.cs` | (only shown via StackSample; deserves its own sample with groups) |
| `SegmentedPivot` | `SegmentedPivot.cs` | (exists in Surfaces but thin — add icon-only variant example) |
| `HorizontalSplitView` | `HorizontalSplitView.cs` | Side-by-side resizable panes (vs. the existing vertical `SplitView`) |
| `Sparkline` | `Sparkline.cs` | Standalone sparkline with multiple datasets and hover tooltip |
| `UptimeBars` / `UptimeCalendar` / `UptimeStatus` | `Uptime*.cs` | Shown together in one `UptimeSample` — split into individual samples |
| `FocusTrap` | `FocusTrap.cs` | Demonstrate modal-style focus lock for custom overlays |
| `LazyVirtualItem` | `LazyVirtualItem.cs` | Explain the intersection-observer pattern used for lazy rendering |

---

## 4. New Components to Add

These components are absent from the library entirely but represent common UI patterns used in every production app.

### 4.1 `DateRangePicker`
Two linked date inputs (Start / End) with a shared popup calendar that highlights the selected range. The popup should show two months side by side. 

**API sketch:**
```csharp
DateRangePicker()
    .Min(DateTime.Today)
    .Max(DateTime.Today.AddYears(1))
    .OnChange((start, end) => ...)
    .Placeholder("Select range")
```

---

### 4.2 `RichTextEditor`
A basic WYSIWYG editor (bold, italic, underline, lists, links, heading levels). The library has `DeltaComponent` for reading rich text, but no write-capable editor. Wrap an existing JS library (Quill, Tiptap, or ProseMirror) with a thin H5 binding.

**API sketch:**
```csharp
RichTextEditor()
    .Toolbar(RichTextToolbar.Basic)      // Bold, Italic, Lists
    .Toolbar(RichTextToolbar.Full)       // + Headings, Links, Images
    .OnChange(delta => ...)
    .ReadOnly()
```

---

### 4.3 `FormField` / `Form`
A vertical layout wrapper that pairs a `Label`, an input control, and a validation message in a consistent column layout. Without it, every sample hand-rolls `Label().SetContent(TextBox())` differently.

**API sketch:**
```csharp
Form()
    .Field("Name", TextBox().Required())
    .Field("Email", TextBox().Email())
    .Field("Role", Dropdown().Items(...).Required())
    .OnSubmit(values => ...)
    .SubmitButton("Save")
```

---

### 4.4 `EmptyState`
A centred illustration + heading + subtext + optional CTA button for empty lists, search-no-results, and error screens. Currently every component implements its own ad-hoc empty message.

**API sketch:**
```csharp
EmptyState()
    .Icon(UIcons.InboxOut)
    .Title("No results found")
    .Description("Try adjusting your search or filters.")
    .Action(Button("Clear filters").OnClick(...))
```

---

### 4.5 `Rating`
Star (or custom icon) rating component for user feedback flows.

**API sketch:**
```csharp
Rating()
    .Stars(5)
    .Value(3)
    .ReadOnly()
    .AllowHalf()
    .OnChange(v => ...)
```

---

### 4.6 `TagInput`
A text box that converts typed text into removable chips on Enter or comma, with optional autocomplete. Distinct from `Picker` (which requires predefined items).

**API sketch:**
```csharp
TagInput()
    .Placeholder("Add tags...")
    .MaxTags(10)
    .Suggestions(async q => await GetSuggestions(q))
    .OnChange(tags => ...)
```

---

### 4.7 `NotificationCenter`
A bell-icon button that opens a panel listing recent notifications, with read/unread state, grouping by date, and a "Mark all read" action.

**API sketch:**
```csharp
NotificationCenter()
    .LoadItems(async () => await GetNotifications())
    .OnMarkRead(id => ...)
    .OnClearAll(() => ...)
    .BadgeCount(observable)
```

---

### 4.8 `Combobox`
A free-form text input combined with a dropdown suggestion list — the user can type a value not in the list. Currently `Dropdown` is strictly constrained to predefined items.

**API sketch:**
```csharp
Combobox()
    .Items("Apple", "Banana", "Cherry")
    .AllowCustom()
    .Placeholder("Select or type...")
    .OnChange(value => ...)
```

---

### 4.9 `ProgressRing` (circular progress)
A circular/donut progress indicator to complement the existing linear `ProgressIndicator`. Commonly used for file upload, quota usage, and dashboard metrics.

**API sketch:**
```csharp
ProgressRing()
    .Value(67)
    .Max(100)
    .Size(ProgressRingSize.Large)
    .Color(Theme.Colors.Blue500)
    .Label("67%")
```

---

### 4.10 `ResizablePanel`
A panel whose size can be adjusted by the user by dragging a handle, similar to `SplitView` but standalone — useful for sidebar-to-content or editor-to-preview layouts without requiring the full split-view shell.

**API sketch:**
```csharp
ResizablePanel()
    .MinWidth(200)
    .MaxWidth(600)
    .DefaultWidth(300)
    .Children(sidebarContent)
    .OnResize(width => ...)
```

---

### 4.11 `ColorSwatch` / `ColorPalette`
A grid of named colour swatches for letting users choose from a predefined palette (e.g. label colours, category colours). The existing `ColorPicker` is the raw browser native control; this fills the "choose from a set" pattern.

**API sketch:**
```csharp
ColorPalette()
    .Swatches(Theme.Colors.AllNamedColors)
    .Value(Theme.Colors.Blue500)
    .OnChange(color => ...)
    .WithCustomColor()   // also allow raw picker
```

---

### 4.12 `KbdShortcut` / `KeyboardShortcutChip`
A small `<kbd>`-styled inline component to render keyboard shortcuts consistently (e.g. `Ctrl+K`). Currently every sample uses raw `TextBlock` for this.

**API sketch:**
```csharp
KbdShortcut("Ctrl", "K")       // renders ⌃K or Ctrl+K depending on OS
KbdShortcut(Keys.Ctrl, Keys.K) // typed overload
```

---

### 4.13 `CommandPalette` – promote from Utilities to first-class
**File:** `Tesserae/src/Components/CommandPalette.cs`  
**Sample:** `Utilities/CommandPaletteSample.cs`

`CommandPalette` is currently categorised as a Utility, but it's a complete interactive component. Move it to the Components group. Add:
- Section/group headers in the results list.
- Recent-commands history.
- A default `Ctrl+K` keybinding hook via `FocusTrap` + window-level `KeyDown`.

---

### 4.14 `DataTable` (enhanced DetailsList)
The existing `DetailsList` is good, but for read-only tabular data at scale, a lighter `DataTable` would be useful:
- No DOM components in cells — text-only cells rendered as native `<td>` for performance.
- Built-in column resizing and column visibility toggle.
- Server-side sort/filter callbacks.
- CSV download button.
- This could be a subclass of `DetailsList` with a simplified API.

---

### 4.15 `Breadcrumb` improvements
**File:** `Tesserae/src/Components/Breadcrumb.cs` / `TextBreadcrumbs.cs`

Two breadcrumb implementations exist (`Breadcrumb` and `TextBreadcrumbs`). Consolidate and add:
- Overflow handling (ellipsis menu for long paths, already hinted at in `OverflowSet`).
- `aria-label="breadcrumb"` + `aria-current="page"` on last item.
- A `BreadcrumbItem` that supports async navigation and icons.

---

## 5. Cross-Cutting Improvements

### 5.1 Accessibility (ARIA) – systematic audit

Apply these ARIA patterns consistently across all interactive components:

| Component type | Required ARIA |
|----------------|---------------|
| All list boxes | `role="listbox"`, items `role="option"`, `aria-selected` |
| All dialogs / modals | `role="dialog"`, `aria-modal="true"`, `aria-labelledby` |
| All navigation | `role="navigation"`, `aria-label` |
| Tree | `role="tree"`, `role="treeitem"`, `aria-expanded`, `aria-level` |
| Tabs / Pivots | `role="tablist"`, `role="tab"`, `role="tabpanel"` |
| Slider | `role="slider"`, `aria-valuemin/max/now/text` |
| Pagination | `role="navigation"`, `aria-label`, `aria-current="page"` |
| Progress | `role="progressbar"`, `aria-valuenow/min/max` |
| Accordion | `role="region"`, button `aria-expanded`, `aria-controls` |
| Checkbox | `role="checkbox"`, `aria-checked` (already present in most) |
| Toggle | `role="switch"`, `aria-checked` (already present) |

Suggest creating an `IAccessibilityAudit` interface (or an extension method `AssertAriaComplete()`) that validates minimum ARIA presence in test builds.

---

### 5.2 Keyboard navigation – standard patterns

Many components respond to mouse events only. Apply these universal patterns:

- **Escape** → close any overlay (Modal, Panel, Dropdown, ContextMenu, Toast) — most already do this, but not consistently.
- **Arrow keys** → navigate focusable lists (Dropdown, Picker, Accordion, Nav, Tree).
- **Enter / Space** → activate focused item.
- **Tab / Shift+Tab** → move between form fields, respecting FocusTrap in modals.
- **Home / End** → jump to first/last item in lists.

---

### 5.3 Validation – unified story

Two parallel patterns exist:
1. Per-component `.Validation(Func<T, string>)` inline.
2. The `Validator` utility that aggregates multiple components.

**Recommendation:** Unify by having all `ICanValidate` components fire a `ValidationChanged` event consumed by the nearest ancestor `Validator` automatically (similar to React's form context). This eliminates the need to register each field manually.

---

### 5.4 Async loading states – unified `LoadingState`

Several components (Dropdown, DetailsList, InfiniteScrollingList, Defer) independently implement their own loading state. Introduce a shared `IHasLoadingState` interface:

```csharp
interface IHasLoadingState
{
    bool IsLoading { get; }
    IComponent LoadingContent { get; set; }  // defaults to Spinner
    IComponent ErrorContent   { get; set; }  // defaults to Message(Error)
}
```

---

### 5.5 Performance – shared observer infrastructure

`ResizeObserver` instances are created per-component in `Pivot`, `Timeline`, and `Sidebar` with no shared coordinator. This adds layout recalculation pressure when many components exist on the same page. Introduce a singleton `ResizeObserverHost` that batches entries, similar to the existing `Layers` singleton pattern.

---

### 5.6 Empty-state API consistency

Different list components use different APIs for empty states:

| Component | Current API |
|-----------|-------------|
| DetailsList | `.WithEmptyMessage(string)` |
| SearchableList | `.WithEmptyMessage(string)` |
| VirtualizedList | `.WithEmptyComponent(IComponent)` |
| ItemsList | no built-in empty state |
| InfiniteScrollingList | no built-in empty state |

Standardise to `.EmptyState(IComponent)` across all list components, and wire in the new `EmptyState` component (§4.4) as the default factory.

---

### 5.7 Mobile / touch – responsive patterns

Most components are designed for pointer devices. Recommended improvements:
- `Sidebar.AsNavbar()` is the only mobile-oriented layout; add a `Sidebar.AsDrawer()` that slides in from the edge on mobile viewports.
- `Dropdown` and `ContextMenu` should use native `<select>` / `<dialog>` fallbacks on touch devices where custom popups are awkward.
- `Slider`: touch-drag is handled by the browser natively on `<input type="range">`, but the fake-progress overlay may interfere. Test and document.
- `DetailsList`: horizontal scroll should be touch-scrollable; add `-webkit-overflow-scrolling: touch`.

---

### 5.8 Component documentation – missing "When to use X vs Y" guidance

Several component pairs are ambiguous:

| Pair | Guidance needed |
|------|-----------------|
| `Dropdown` vs `ChoiceGroup` | Dropdown for 5+ items, ChoiceGroup for 2-5 |
| `Picker` vs `Dropdown` (multi-select) | Picker for search-first typed selection, Dropdown for browse selection |
| `Modal` vs `Panel` vs `Dialog` | Modal for complex tasks, Panel for context without blocking, Dialog for confirmation only |
| `VirtualizedList` vs `ItemsList` vs `InfiniteScrollingList` | VirtualizedList for known-size large sets, Infinite for unknown-size, ItemsList for small sets |
| `SectionStack` vs `Stack` + `Card` | SectionStack for full-page doc layout, raw Stack for inline grouping |
| `Breadcrumb` vs `TextBreadcrumbs` | Unclear — consolidate (§4.15) |

Add a "Choosing the right component" section to each sample's Best Practices card.

---

## 6. Priority Ordering

| Priority | Item | Effort |
|----------|------|--------|
| P0 (fix now) | §1.1 OnMouseOver bug | XS |
| P0 (fix now) | §1.4 Remove console.log from VirtualizedList | XS |
| P0 (fix now) | §1.3 Nav typo fix + deprecation alias | XS |
| P1 (soon) | §1.2 ContextMenu Render/Show stubs | S |
| P1 (soon) | §2.1 Slider vertical fix + ARIA | M |
| P1 (soon) | §3 Add missing samples (Teaching, TutorialModal, StepsSlider, TimePicker, MonthPicker, WeekPicker) | M |
| P1 (soon) | §4.3 FormField / Form component | M |
| P1 (soon) | §4.4 EmptyState component | S |
| P2 (next sprint) | §2.3 Picker async + cache | M |
| P2 (next sprint) | §2.5 DetailsList inline edit | L |
| P2 (next sprint) | §4.1 DateRangePicker | L |
| P2 (next sprint) | §4.6 TagInput | M |
| P2 (next sprint) | §4.8 Combobox | M |
| P2 (next sprint) | §5.1 ARIA audit across all components | L |
| P3 (backlog) | §2.2 Dropdown virtual scrolling | L |
| P3 (backlog) | §2.8 Tree drag-and-drop | L |
| P3 (backlog) | §4.2 RichTextEditor | XL |
| P3 (backlog) | §4.7 NotificationCenter | L |
| P3 (backlog) | §4.9 ProgressRing | S |
| P3 (backlog) | §4.10 ResizablePanel | M |
| P3 (backlog) | §4.12 KbdShortcut chip | S |
| P3 (backlog) | §5.3 Unified validation story | L |
| P3 (backlog) | §5.5 Shared ResizeObserver host | M |
