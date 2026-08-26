---
name: mark-highlighter
description: Mark every occurrence of a keyword inside a DOM subtree (same-origin iframes included) by wrapping matches in mark elements, with per-call options (whole word, across elements, wildcards), unmarking and focused-match navigation. Use for find-in-document / highlight-search-hits features in a Tesserae (C#/Transpose) app.
---

# MarkHighlighter

`MarkHighlighter` is a static helper (not an `IComponent`) that marks keyword matches inside rendered content — the DOM equivalent of a browser's find-in-page. It walks every text node under a root element (descending into same-origin iframes). Where the browser supports the **CSS Custom Highlight API** (`MarkHighlighter.IsHighlightApiSupported`), matches in the page itself are painted through `CSS.highlights` as live ranges — no DOM mutation at all, so components that hold references to their own text nodes are untouched; the registry names `tss-marked` / `tss-marked-focused` are styled by `tss.markhighlighter.css`. Text inside iframes (whose documents don't carry the page stylesheet), browsers without the API, and callers that opt out (`MarkOptions.UseHighlightApi = false`, or a custom `Element`/`MarkData`/`ClassName`) get the classic treatment instead: each match wrapped in a `<mark data-marked="true">` element, unwrapped again on unmark. Adapted from mark.js.

Matching is case-insensitive by default, folds diacritics (searching `cafe` finds `café`), and merges whitespace runs (a keyword typed with one space matches text with many). Text inside `script`, `style`, `title`, `head` and already-marked elements is skipped.

Passes on the same root are **serialized**: starting a new mark or unmark cancels the in-flight one and waits for it, so calling on every keystroke is safe without external guarding. Very large documents are walked cooperatively (the pass yields to the event loop periodically), so the page stays responsive and cancellation engages mid-pass.

## Methods (static on `MarkHighlighter`)

- `Task MarkAsync(HTMLElement ctx, string keyword, MarkOptions options, Action<MarkedMatch> eachCb, CancellationToken cancellationToken)` — marks every occurrence of `keyword` under `ctx`; `eachCb` receives one `MarkedMatch` per match in document order (collect them for navigation). A `MarkedMatch` carries either `Range` (highlight registry) or `Elements` (wrapper elements, one per crossed text node), plus `Text`.
- `Task MarkAsync(HTMLElement ctx, string keyword, Action<Node> eachCb, CancellationToken cancellationToken)` — the element-only legacy shape: forces element wrapping and reports each wrapper element individually.
- `Task UnmarkAsync(HTMLElement ctx, MarkOptions options = null, CancellationToken cancellationToken = default)` — removes every mark under `ctx`: registry ranges are deregistered, wrapper elements unwrapped and their parents re-normalized. Call before re-marking with a new keyword.
- `void FocusResult(HTMLElement ctx, MarkedMatch match, bool scrollIntoViewIfNeeded)` — moves the focused-match highlight to one match, clearing it from the others. A registry match joins the `tss-marked-focused` highlight (priority above the base one) and is scrolled into view by nudging its scrollable ancestors; a wrapped match gets the class `tss-highlight-focused` plus an inline theme danger background (so it also shows inside iframes) and the standard `scrollIntoView` (nearest). An `HTMLElement` overload remains for element-only callers.
- `bool IsHighlightApiSupported` — feature check for the CSS Custom Highlight API.

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
| `AcrossElements` | match phrases split by inline tags (`<b>web</b> applications`); through the registry a spanning match is one range, wrapped it becomes one mark element per crossed text node (all in that match's `Elements`) |
| `UseHighlightApi` (`bool?`) | null (default): CSS Custom Highlight API when supported and no custom `Element`/`MarkData`/`ClassName` is set; `true`: whenever supported; `false`: always wrap in elements. Iframe text is always wrapped |

Because per-call options exist, the static fields (`MarkHighlighter.Element`, `.MarkData`, `.ClassName`, `RegExpCreator.CaseSensitive`) are only defaults — prefer passing `MarkOptions` so independent surfaces never fight over global state.

## Companions

- `RegExpCreator.Create(string keyword, MarkOptions options = null)` — builds the escaped, diacritics-folding, blank-merging `es5.RegExp` used for matching (returns null when nothing remains to match); usable on its own for highlighting elsewhere (e.g. `OmniResult.Highlight`).
- `DOMIterator.ForEachNodeAsync<T>(ctx, eachCb, whatToShow, nodeFilter, token)` — the underlying iframe-aware DOM walker (`DOMIterator.SHOW_TEXT` / `SHOW_ELEMENT`, `FILTER_ACCEPT` / `FILTER_REJECT` / `FILTER_SKIP` constants); `DOMIterator.QuerySelectorAllIframesRecursive(ctx, selector, acceptNode)` runs a selector across the subtree and its iframes. `DOMIterator.IframesTimeoutMs` (default 5000) bounds how long a frame may take to load before it is skipped; a cross-origin or sandboxed frame is skipped without failing the pass.

## Example

```csharp
private readonly List<MarkedMatch> _matches = new List<MarkedMatch>();
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
        MarkHighlighter.FocusResult(root, _matches[0], scrollIntoViewIfNeeded: true);
    }
}
```

## Related

- [search-box.md](search-box.md) — the input to drive it from (`SearchAsYouType`)
- [omni-result.md](omni-result.md) — search-result rows that highlight with the same kind of RegExp
