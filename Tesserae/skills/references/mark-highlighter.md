---
name: mark-highlighter
description: Mark every occurrence of a keyword inside a DOM subtree (same-origin iframes included) by wrapping matches in mark elements, with per-call options (whole word, across elements, wildcards), unmarking and focused-match navigation. Use for find-in-document / highlight-search-hits features in a Tesserae (C#/Transpose) app.
---

# MarkHighlighter

`MarkHighlighter` is a static helper (not an `IComponent`) that marks keyword matches inside rendered content — the DOM equivalent of a browser's find-in-page. It walks every text node under a root element (descending into same-origin iframes), wraps each match in a `<mark data-marked="true">` element, and can unwrap them all again, restoring the original text nodes. Adapted from mark.js.

Matching is case-insensitive by default, folds diacritics (searching `cafe` finds `café`), and merges whitespace runs (a keyword typed with one space matches text with many). Text inside `script`, `style`, `title`, `head` and already-marked elements is skipped.

Passes on the same root are **serialized**: starting a new mark or unmark cancels the in-flight one and waits for it, so calling on every keystroke is safe without external guarding. Very large documents are walked cooperatively (the pass yields to the event loop periodically), so the page stays responsive and cancellation engages mid-pass.

## Methods (static on `MarkHighlighter`)

- `Task MarkAsync(HTMLElement ctx, string keyword, Action<Node> eachCb, CancellationToken cancellationToken)` — marks every occurrence of `keyword` under `ctx`; `eachCb` receives each created mark element in document order (collect them for navigation).
- `Task MarkAsync(HTMLElement ctx, string keyword, MarkOptions options, Action<Node> eachCb, CancellationToken cancellationToken)` — same, with per-call options (below).
- `Task UnmarkAsync(HTMLElement ctx, MarkOptions options = null, CancellationToken cancellationToken = default)` — removes every mark under `ctx` and re-normalizes the split text nodes. Call before re-marking with a new keyword.
- `void FocusResult(HTMLElement ctx, HTMLElement elementToFocus, bool scrollIntoViewIfNeeded)` — moves the focused-match highlight (class `tss-highlight-focused` plus an inline theme danger background, so it also shows inside iframes) to one mark, clearing it from the others, scrolling it into view with the standard `scrollIntoView` (nearest).

## MarkOptions (all optional; unset falls back to the static defaults)

| Option | Effect |
| --- | --- |
| `Element` / `MarkData` / `ClassName` | wrapper tag, data-* attribute name, extra class (defaults: statics on `MarkHighlighter`) |
| `CaseSensitive` (`bool?`) | default: `RegExpCreator.CaseSensitive` |
| `Diacritics` (default `true`) | fold diacritic variants |
| `WholeWord` | only match where not part of a longer word |
| `SeparateWordSearch` | split a multi-word keyword on whitespace, mark each word |
| `Wildcards` | `*` = any run of non-space characters, `?` = one optional non-space character |
| `IgnoreJoiners` | match across soft hyphens (`­`) and zero-width joiners |
| `MinLength` | skip keywords shorter than this |
| `AcrossElements` | match phrases split by inline tags (`<b>web</b> applications`); a spanning match becomes one mark element per crossed text node, and `eachCb` fires per wrapper |

Because per-call options exist, the static fields (`MarkHighlighter.Element`, `.MarkData`, `.ClassName`, `RegExpCreator.CaseSensitive`) are only defaults — prefer passing `MarkOptions` so independent surfaces never fight over global state.

## Companions

- `RegExpCreator.Create(string keyword, MarkOptions options = null)` — builds the escaped, diacritics-folding, blank-merging `es5.RegExp` used for matching (returns null when nothing remains to match); usable on its own for highlighting elsewhere (e.g. `OmniResult.Highlight`).
- `DOMIterator.ForEachNodeAsync<T>(ctx, eachCb, whatToShow, nodeFilter, token)` — the underlying iframe-aware DOM walker (`DOMIterator.SHOW_TEXT` / `SHOW_ELEMENT`, `FILTER_ACCEPT` / `FILTER_REJECT` / `FILTER_SKIP` constants); `DOMIterator.QuerySelectorAllIframesRecursive(ctx, selector, acceptNode)` runs a selector across the subtree and its iframes. `DOMIterator.IframesTimeoutMs` (default 5000) bounds how long a frame may take to load before it is skipped; a cross-origin or sandboxed frame is skipped without failing the pass.

## Example

```csharp
private readonly List<Node> _matches = new List<Node>();
private CancellationTokenSource _searchCTS;

private async Task DoSearchAsync(HTMLElement root, string term)
{
    _searchCTS?.Cancel();
    _searchCTS = new CancellationTokenSource();

    _matches.Clear();
    await MarkHighlighter.UnmarkAsync(root, cancellationToken: _searchCTS.Token);

    if (!string.IsNullOrWhiteSpace(term))
    {
        var options = new MarkOptions { AcrossElements = true, IgnoreJoiners = true };
        await MarkHighlighter.MarkAsync(root, term, options, match => _matches.Add(match), _searchCTS.Token);
    }

    if (_matches.Count > 0)
    {
        MarkHighlighter.FocusResult(root, _matches[0].As<HTMLElement>(), scrollIntoViewIfNeeded: true);
    }
}
```

## Related

- [search-box.md](search-box.md) — the input to drive it from (`SearchAsYouType`)
- [omni-result.md](omni-result.md) — search-result rows that highlight with the same kind of RegExp
