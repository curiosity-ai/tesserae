---
name: modal-stack
description: A deck of modals - the newest sheet in front, the ones it was opened from peeking behind it, with go-back tabs, Escape to pop and a backdrop that dismisses the chain. Use when one modal opens another in a Tesserae (C#/Transpose) app.
---

# ModalStack

A static stack that shows several `Modal`s as a deck of sheets: the newest one in
front, the ones it was opened from lifted and scaled behind it, each a little
quieter than the one in front of it. Use it when opening something inside a modal
opens another modal — a preview that links to another preview — instead of
stacking independent overlays that bury each other.

`ModalStack` takes the modal's own surface and shows it itself, so **`Show()` is
not what opens a stacked modal — `Push` is**. Everything else about the modal
still works: `Hide()` pops it, and its show/hide handlers run as they would have.

## Behaviour

- Each sheet is pushed under a **key**. Pushing a key already in the stack
  **rewinds** to it instead of opening a second copy of the same thing.
- Past `ModalStack.MaxDepth` (4) sheets the oldest is dropped and `IsTruncated`
  becomes true — which only the breadcrumb needs to say.
- Clicking a peeking sheet goes back to it (it is a real button labelled
  "Back to `<name>`"; everything under it is `inert` and `aria-hidden`).
- A peeking sheet is **only the strip of itself that clears the one in front**: its content and its
  footer are `display: none` rather than laid out and covered, its header commands are hidden, and
  what is left of its header is squeezed into that strip - so its icon and title sit alone at the top
  of it. Colour drains out of it (`saturate`) and its title fades with how far back it is, while the
  sheet itself stays solid.
- **Escape** closes the sheet in front — unless a menu, dropdown or dialog is
  open above it, which answers Escape itself.
- **Clicking the backdrop** dismisses the whole chain.
- A breadcrumb of the chain is drawn above the deck when more than one sheet is
  open; its steps go back to that sheet.

## Key API

- `ModalStack.Push(string key, string name, Modal modal)` — open a sheet in front (or rewind to it).
- `ModalStack.Replace(string key, string name, Modal modal)` — swap the sheet in front, leaving the chain behind it — what stepping through a list of results does.
- `ModalStack.Pop()` / `.PopTo(string key)` / `.Clear()` / `.Remove(Modal)`.
- `ModalStack.TryRewindTo(string key)` — go back to that sheet if it is open; false if it isn't.
- `ModalStack.Rename(string key, string name)` — for a sheet whose title arrives with its content.
- `ModalStack.Depth` / `.IsEmpty` / `.Top` / `.Entries` / `.Contains(key)` / `.Get(key)` / `.IsStacked(Modal)` / `.IsTruncated`.
- `ModalStack.Changed` — raised on every change, so a host can keep the route naming what is open in step with it.

## Example

```csharp
using static Tesserae.UI;

void OpenPreview(SearchHit doc, bool steppingThroughResults)   // `document`/`Document` are the DOM's
{
    var modal = BuildPreviewModal(doc);   // any Modal; OmniResult<T>.ToModal() is the usual one

    if (steppingThroughResults) ModalStack.Replace(doc.Id, doc.Title, modal);
    else                        ModalStack.Push(doc.Id, doc.Title, modal);
}

// Keep the URL naming the whole chain, so a refresh reopens it.
ModalStack.Changed += () => Router.ReplaceQueryParameters(
    p => p.With("open", string.Join(",", ModalStack.Entries.Select(e => e.Key))));
```

## Related

- Modal — the surface a sheet is — `modal.md`
- OmniResult — `ToModal()` builds the sheet a search result opens into — `omni-result.md`
- Layer — the overlay infrastructure underneath — `layer.md`
- Routing — keeping the URL in step with the chain — `routing.md`
- Full docs & API: `/tesserae/components/modal-stack`
