# Build.Yjs

Builds `Tesserae/tps/assets/js/yjs.min.js`, the script `Yjs.LoadAsync()` fetches the first time a page uses a shared
document (`Tesserae/src/Yjs/`), and `yjs.LICENSE.txt` beside it, with the licence of every package bundled into it
(all MIT).

```bash
cd Build.Yjs
npm ci
npm run build
```

What goes in, pinned exactly in `package.json`:

- [`yjs`](https://github.com/yjs/yjs), the CRDT: `Y.Doc`, the shared types, updates, snapshots, `UndoManager`,
  relative positions.
- [`y-protocols`](https://github.com/yjs/y-protocols): the `sync` message handlers (step 1, step 2, update) and
  `awareness` (presence), which is what `YjsSyncProvider` frames into the y-websocket message format.
- [`lib0`](https://github.com/dmonad/lib0), the version yjs resolves to: `encoding` / `decoding` (the varint
  encoders the protocols write through) and `buffer` (base64).

`entry.js` is the whole of what the script defines: `window.Y` (all of yjs) and
`window.YProtocols = { sync, awareness, encoding, decoding, buffer }`.

**There must be exactly one copy of yjs in the bundle.** Yjs checks for a second instance of itself and warns
("Yjs was already imported. This breaks constructor checks"), and type checks such as `instanceof Y.Text` then fail
between the two. `build.mjs` reads esbuild's metafile and fails the build when any package was bundled from two
folders, or when yjs contributed more than one module file. A `y-protocols` or `lib0` bump that npm cannot dedupe
shows up there rather than in a browser.

The output is one classic script (an IIFE), shipped by `Tesserae/tps.json` as the unlinked `tss-yjs.min.js` resource
(`"load": false`): it is copied into a consuming site but never added to `index.html`.

To update: bump the versions in `package.json` (keep `lib0` at the version `yjs` resolves), `npm install`,
`npm run build`, check the gallery's **Yjs** sample (two documents converge, offline then online resyncs), and commit
the two generated files with the lock file.
