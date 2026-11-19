// The whole of what tss-yjs.min.js puts on the page: Yjs as window.Y, and the y-protocols message handlers with the
// lib0 encoders they write through as window.YProtocols. Every import below resolves to the one copy of yjs and the
// one copy of lib0 in node_modules - build.mjs fails the build when that stops being true, because Yjs breaks with
// two instances on a page ("Yjs was already imported").

import * as Y from 'yjs';
import * as sync from 'y-protocols/sync';
import * as awareness from 'y-protocols/awareness';
import * as encoding from 'lib0/encoding';
import * as decoding from 'lib0/decoding';
import * as buffer from 'lib0/buffer';

globalThis.Y = Y;
globalThis.YProtocols = { sync, awareness, encoding, decoding, buffer };
