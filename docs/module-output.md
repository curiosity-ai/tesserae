# Loading the sample gallery as JavaScript modules

Both `Tesserae` and `Tesserae.Tests` build with `outputBy: "Module"`, so `tps` emits one ES module
per chunk instead of a single bundle. Only the code needed to draw the shell ships up front; each
sample's chunk — and each Tesserae component's — is fetched when something actually needs it.

Requires `Transpose.Compiler` 26.8.4092 / `Transpose.BCL` 26.8.4093 or newer.

## What it costs, what it saves

Tesserae's and the gallery's own JavaScript — `tss` + the app, leaving out the `tps` runtime and the
third-party bundle, which are the same either way:

| | initial JavaScript |
| --- | --- |
| single bundle (`tss.js` + `app.js`) | 3,542 KB raw / 547 KB gzipped |
| both as modules | **1,055 KB raw / 188 KB gzipped** |

70% of the raw bytes and 66% of the gzipped ones. Counting everything the page loads — runtime,
metadata, `tss-dep.js` — it is 7,029 KB → 4,542 KB raw and 1,120 KB → 761 KB gzipped.

682 chunks (521 from Tesserae, 161 from the gallery, 3,280 KB raw in total), of which **121 load up
front** and 561 on demand.

The rendered page is unchanged. Built both ways and compared with
[`Tesserae.Bench/playwright/textdiff-samples.js`](../Tesserae.Bench/playwright/textdiff-samples.js),
126 of 132 samples are identical run for run, and every difference it reports also appears when the
module build is compared **against itself** — the Charts run count, three clock timestamps, Progress
Ring's animated fill and Masonry's debounced relayout. All 141 sidebar entries render with zero
console errors.

## The four changes it needed

**1. `[SkipTypeClustering]` on `UI`.** This is the one that matters. A chunk is a strongly-connected
component of the reference graph, so a static facade whose 300 factories construct half the library
fuses that half into a single chunk: `UI` reaches every component, and every component calls back
into `UI` for `Div`/`VStack`. The attribute drops the edges *out of* the facade and attributes each
member's dependencies to the code that **calls** it, which is where a static method body actually
runs:

```csharp
[Transpose.SkipTypeClustering]
public static partial class UI
```

Without it the largest library chunk is **193 types / 1,612 KB** and 213 chunks load up front. With
it the largest is 5 types / 67 KB, 513 of the 521 library chunks hold exactly one type, and the eager
payload is less than half. Nothing else about `UI` changes — every factory method stays.

**2. `tps.json`** — opt in, in both the library and the app:

```json
"outputBy": "Module",
```

A library needs no other change: it defers everything, publishes a chunk map, and (because of the
attribute) publishes the facade's per-member dependency sets so the consuming build can turn a
`UI.Card(...)` call into an import of Card's chunk.

**3 & 4. Instantiating a sample became asynchronous.** The gallery discovers samples by reflection
(`typeof(ISample).Assembly.GetTypes()`) and constructs the selected one with `Activator`. Reflection
keeps working against a deferred type — the runtime registers a stub carrying its name, interfaces
and attributes — but *constructing* it has to fetch its module first, and fetching is asynchronous:

```csharp
// Sample.cs
- public Func<IComponent> ContentGenerator { get; }
+ public Func<Task<IComponent>> ContentGenerator { get; }

// App.cs — the factory
- () => Activator.CreateInstance(sampleType) as IComponent
+ async () => await Activator.CreateInstanceAsync(sampleType) as IComponent

// App.cs — the content area
- DeferSync(currentPage, page => … page.ContentGenerator() …)
+ Defer(currentPage, async page => … (await page.ContentGenerator()) …)
```

`Defer` already existed for exactly this shape, so the change is two signatures and an `await`.
Calling the synchronous `Activator.CreateInstance` on a deferred type throws and names the module
rather than failing somewhere inside the constructor — the failure is loud, not silent.

## Why not remove the `UI` facade instead

That was tried first: deleting the same-named factory wrappers and rewriting every call site to
`new Card(...)`. It works, but it is ~630 changed lines across 40 files, it has to be redone for
every component added afterwards, and it is **worse** — 1,788 KB / 328 KB gzipped eager, against
1,055 KB / 188 KB for the attribute. Removing the factories still leaves the `Div`/`VStack` helper
edges that make `UI` a hub; the attribute removes the hub itself.

## Trying it

```bash
dotnet build
cd Tesserae.Tests/bin/Debug/netstandard2.0/tps/
dotnet serve --port 5000
```

`index.html` carries a `<script type="module">` for each of `tss.js` and `app.js`; open the network
panel and watch `chunks/<assembly>/cN.mjs` arrive as you click through the sidebar.

The compiler-side design — why a chunk is a strongly-connected component, why a per-class split is
unsound, and what `[SkipTypeClustering]` does to the graph — is in
[`TODO.modules.md`](https://github.com/curiosity-ai/transpose/blob/master/TODO.modules.md).
