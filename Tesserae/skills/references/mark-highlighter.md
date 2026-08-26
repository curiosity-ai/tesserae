---
name: mark-highlighter
description: Mark every occurrence of a keyword inside a DOM subtree (same-origin iframes included) by wrapping matches in mark elements, with unmarking and focused-match navigation. Use for find-in-document / highlight-search-hits features in a Tesserae (C#/Transpose) app.
---

# MarkHighlighter

`MarkHighlighter` is a static helper (not an `IComponent`) that marks keyword matches inside rendered content — the DOM equivalent of a browser's find-in-page. It walks every text node under a root element (descending into same-origin iframes), wraps each match in a `<mark data-marked="true">` element, and can unwrap them all again, restoring the original text nodes. Adapted from mark.js.

Matching is case-insensitive by default, folds diacritics (searching `cafe` finds `café`), and merges whitespace runs (a keyword typed with one space matches text with several). Text inside `script`, `style`, `title`, `head` and already-marked elements is skipped.

## Methods (static on `MarkHighlighter`)

- `Task MarkAsync(HTMLElement ctx, string keyword, Action<Node> eachCb, CancellationToken cancellationToken)` — marks every occurrence of `keyword` under `ctx`; `eachCb` receives each created mark element in document order (collect them for navigation). Cancel the token to stop a long-running pass (e.g. when the user keeps typing).
- `Task UnmarkAsync(HTMLElement ctx)` — removes every mark under `ctx` and re-normalizes the split text nodes. Call before re-marking with a new keyword.
- `void FocusResult(HTMLElement ctx, HTMLElement elementToFocus, bool scrollIntoViewIfNeeded)` — moves the focused-match highlight (class `tss-highlight-focused` plus an inline theme danger background, so it also shows inside iframes) to one mark, clearing it from the others.

Configuration via static fields: `Element` (default `"mark"`), `MarkData` (the `data-*` attribute name, default `"marked"`), `ClassName` (optional extra class on each mark).

## Companions

- `RegExpCreator.Create(string)` — builds the escaped, diacritics-folding, blank-merging `es5.RegExp` used for matching; usable on its own for highlighting elsewhere (e.g. `OmniResult.Highlight`). `RegExpCreator.CaseSensitive` toggles case sensitivity.
- `DOMIterator.ForEachNodeAsync<T>(ctx, eachCb, whatToShow, nodeFilter, token)` — the underlying iframe-aware DOM walker (`DOMIterator.SHOW_TEXT` / `SHOW_ELEMENT`, `FILTER_ACCEPT` / `FILTER_REJECT` / `FILTER_SKIP` constants); `DOMIterator.QuerySelectorAllIframesRecursive(ctx, selector, acceptNode)` runs a selector across the subtree and its iframes.

## Example

```csharp
private readonly List<Node> _matches = new List<Node>();
private CancellationTokenSource _searchCTS;

private async Task DoSearchAsync(HTMLElement root, string term)
{
    _searchCTS?.Cancel();
    _searchCTS = new CancellationTokenSource();

    _matches.Clear();
    await MarkHighlighter.UnmarkAsync(root);

    if (!string.IsNullOrWhiteSpace(term))
    {
        await MarkHighlighter.MarkAsync(root, term, match => _matches.Add(match), _searchCTS.Token);
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
