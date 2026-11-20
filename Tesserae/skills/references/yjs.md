---
name: yjs
description: Real-time collaborative editing with Yjs in a Tesserae (C#/Transpose) app - loading the library on demand, the typed Y declarations (Doc, Text, Array, Map, XmlFragment, UndoManager, relative positions, updates, snapshots), Awareness for presence, YjsSyncProvider over a host-owned transport, stable user colours, and binding a TextArea or TextBox to shared text. Use when several users or tabs edit the same document.
---

# Yjs: shared documents

[Yjs](https://docs.yjs.dev) is a CRDT: every peer edits its own copy of a document (`Y.Doc`), changes travel as
binary updates, and two copies that have seen the same updates hold the same content whatever order they arrived in.
Tesserae ships Yjs 13.6 with y-protocols (sync and awareness) as a separate script, fetched the first time it is
needed, and declares it to the compiler so it is used from C# with types.

## Load it first

```csharp
await Yjs.LoadAsync();   // idempotent; every caller shares one fetch; a failed load is retried next call
```

Nothing on `Y`, `Awareness` or `YProtocols` exists before that completes. `Yjs.IsLoaded` says whether it has. The
script is the unlinked resource `assets/js/tss-yjs.min.js` (never in `index.html`); do not load another copy of Yjs on
the page - its type checks fail between two copies.

## Documents and shared types

```csharp
var doc  = new Y.Doc();
var text = doc.getText("content");          // Y.Text
var list = doc.getArray<string>("items");   // Y.Array<T>
var map  = doc.getMap<object>("settings");  // Y.Map<T>
var xml  = doc.getXmlFragment("prosemirror");

doc.transact(tr =>
{
    text.insert(0, "Hello", null);          // null attributes: continue the formatting there
    text.@delete(5, 0);
    list.push(new[] { "a", "b" });          // a C# array crosses as the JavaScript array it is
    map.set("mode", "dark");
}, origin: "my-code");                      // the origin observers and providers see

string content = text.asString();           // toString() in JavaScript
```

Members keep Yjs' own names (`[Convention(Notation.None)]`), so its documentation reads straight across. Three
spellings differ, all for C#'s sake: `@delete` (a keyword), `asString()` (`toString()`), and events, which are one
typed method per event rather than `on(name, f)`:

| Yjs | C# |
|---|---|
| `doc.on('update', f)` / `off` | `doc.onUpdate(f)` / `doc.offUpdate(f)` - `f(update, origin, doc, transaction)` |
| `doc.on('updateV2' / 'beforeTransaction' / 'afterTransaction' / 'beforeAllTransactions' / 'afterAllTransactions' / 'destroy', f)` | `onUpdateV2`, `onBeforeTransaction`, `onAfterTransaction`, `onBeforeAllTransactions`, `onAfterAllTransactions`, `onDestroy` (+ `off…`) |
| `ytext.observe(f)` | `text.observe((Y.TextEvent e, Y.Transaction tr) => …)`; `e.delta` is `retain` / `insert` / `@delete` runs |
| `type.observeDeep(f)` | `observeDeep((ReadOnlyArray<Y.YEvent> events, Y.Transaction tr) => …)` |
| `awareness.on('change' / 'update', f)` | `awareness.onChange(f)` / `onUpdate(f)` - `f(AwarenessChanges changes, object origin)` |
| `undoManager.on('stack-item-added', f)` | `onStackItemAdded(f)` / `onStackItemPopped(f)` - `e.stackItem.meta` is a `Map` for the host's own data (a selection to restore) |

Keep a handler you will remove in a field: Yjs removes listeners by reference, and a lambda written twice is two
functions.

## Binary data, updates and snapshots

Updates, state vectors and snapshots are `Uint8Array` (`Transpose.Core.es5`). `YjsBinary` converts:
`ToBytes(u8)` / `FromBytes(byte[])`, and `ToBase64(u8)` / `FromBase64(s)` (straight from the typed array, no
`byte[]` in between).

```csharp
Uint8Array whole  = Y.encodeStateAsUpdate(doc);
Uint8Array vector = Y.encodeStateVector(other);
Uint8Array diff   = Y.encodeStateAsUpdateSince(doc, vector);   // encodeStateAsUpdate(doc, sv)
Y.applyUpdate(other, diff, "replay");
Uint8Array merged = Y.mergeUpdates(new[] { a, b });
// …V2 variants, diffUpdate, encodeStateVectorFromUpdate, convertUpdateFormatV1ToV2 / V2ToV1

var snap = Y.snapshot(doc);                                    // needs doc.gc = false to be restorable
Y.Doc past = Y.createDocFromSnapshot(doc, snap);
```

## Positions that survive edits

```csharp
var rel = Y.createRelativePositionFromTypeIndex(text, index, 0);   // assoc: >= 0 sticks right, < 0 left
var abs = Y.createAbsolutePositionFromRelativePosition(rel, doc);  // null when its anchor is gone
int at  = abs.index;
object json = Y.relativePositionToJSON(rel);                       // what bindings put in awareness
var back    = Y.createRelativePositionFromJSON(json);
```

## Undo that only undoes this user

```csharp
var undo = new Y.UndoManager(text);
undo.addTrackedOrigin(myBinding);    // capture only transactions with this origin
undo.removeTrackedOrigin(null);      // starts as { null }: drop it to ignore origin-less edits
undo.undo(); undo.redo(); undo.stopCapturing(); undo.destroy();
```

Remote updates arrive with the provider as their origin, so they are never on this user's stack.

## Presence: Awareness

```csharp
var awareness = new Awareness(doc);
awareness.setLocalStateField("user", YjsUserColors.UserFor("Ada"));   // { name, color, colorLight }

awareness.getStates().forEach((state, clientId, map) =>
{
    YjsUser user = state.user;                // null when that client set none
    object sel   = state.field("selection");  // any other field
});

awareness.destroy();                          // stops its renewal timer - always call it when done
```

`YjsUserColors` gives a stable colour per name (or id) from ten clearly different hues - `ColorFor(key)`,
`LightColorFor(key)` (20% for selections), `SlotFor(key)`, `Palette`, `UserFor(name, key = null)`.

## Syncing: YjsSyncProvider

`YjsSyncProvider` speaks the y-websocket message protocol (type 0 sync step 1 / step 2 / update, 1 awareness,
2 auth, 3 query awareness) over a transport the host owns: it hands every outgoing message to a callback and takes
incoming ones through `Receive`.

```csharp
var provider = new YjsSyncProvider(doc, awareness, async bytes => await PostToServer(bytes));   // byte[]
// or, without the byte[] copy:
var provider = YjsSyncProvider.WithUint8ArraySender(doc, awareness, u8 => socket.SendAsync(u8));
var provider = YjsSyncProvider.WithBase64Sender(doc, awareness, b64 => PostTextAsync(b64));

serverEvents.OnMessage(bytes => provider.Receive(bytes));   // Receive(byte[]) / Receive(Uint8Array) / ReceiveBase64(string)

await provider.ConnectAsync();         // sends sync step 1, the local awareness, and a query for the others'
await provider.WhenSyncedAsync();      // the peer answered step 1
provider.Synced.Observe(s => …);       // IObservable<bool>; also IsSynced, IsConnected
provider.Disconnect();                 // transport went away: stop sending, drop the others' presence
await provider.ConnectAsync();         // after reconnecting: step 1 / step 2 carries what either side missed
provider.Dispose();                    // flushes, broadcasts this client's departure, unhooks doc and awareness
```

- Local changes (any origin but the provider) are sent while connected; one frame's worth is merged into one
  message (`BatchUpdates`, on by default). Changes made while disconnected wait for the next `ConnectAsync`.
- Messages are sent one at a time, in order. A failed send raises `OnSendFailed` (or logs when nothing listens)
  and clears `IsSynced`: connect again to resync. `OnPermissionDenied` reports an auth refusal.
- The peer must send its own sync step 1 when a client connects (as the y-websocket server does), or it never
  learns what the client wrote offline. The provider answers any step 1 it receives.
- A message that arrives after `Dispose` is ignored. The provider never destroys the doc or the awareness.

## Binding a text field

```csharp
var field   = TextArea();
var binding = field.BindYjs(text);   // also TextBox.BindYjs; the field takes the shared text's content
binding.Dispose();                   // unbinds; both keep their content
```

The binding diffs each input against the previous value (common prefix and suffix) and moves the caret through
remote changes. It is for plain fields; a code editor binds its own model (Tesserae.Monaco's `CodeEditor.BindYjs`).

## Binding a rich text editor

```csharp
var editor  = SquireEditor();
var binding = editor.BindYjs(text, awareness);   // text holds Markdown; the editor takes its content
```

The shared text is **Markdown**, so a `SquireEditor` and a code editor bound to the same `Y.Text` edit one document,
and their carets show in each other (both publish the awareness `selection` field y-monaco uses). Only the blocks a
change touches are re-rendered, and undo reverts only this user's edits. See `squire-editor.md`.

## Related

- `defer.md` - load Yjs inside `Defer(async () => { await Yjs.LoadAsync(); … })`.
- `observables.md` - `provider.Synced` is an `IObservable<bool>`.
- `text-area.md`, `text-box.md`, `squire-editor.md`.
