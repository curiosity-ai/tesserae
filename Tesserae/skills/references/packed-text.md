# PackedText

Squeezes a string into a shorter, scrambled, ASCII-safe one and back again. Built for bulky
repetitive literals that would otherwise sit in source in plain sight — the `PixelAvatar`
artwork is the one the toolkit itself uses.

```csharp
var packed = PackedText.Pack(bigString, "my-key");   // compress + scramble + Base64
var back   = PackedText.Unpack(packed, "my-key");    // back to bigString
```

## What it does

`Pack` runs the text through four stages and `Unpack` undoes all four:

1. **UTF-8** — so non-ASCII text survives and ASCII does not waste a zero byte per character.
2. **LZSS** — a sliding window of 4096 bytes, matches of 3 to 18 bytes. Groups of eight items
   led by a flag byte, one bit each: set means a literal byte, clear means a two-byte
   back-reference. Nothing is entropy-coded afterwards, which costs some ratio and buys a
   decoder twenty lines long.
3. **Repeating-key XOR** — the scrambling. Its own inverse, so both directions call it.
4. **Base64** — hand-rolled, so the result is safe to paste into a source file as a plain
   string literal.

An FNV-1a checksum of the original text rides along in the header, so unpacking with the wrong
key throws `FormatException` instead of quietly handing back rubbish. A null or empty key skips
the scrambling and just compresses.

## What it is not

**The XOR is obfuscation, not security.** A repeating key over a known-plaintext header is
broken by anyone who cares to, and a key that has to reach the browser is readable there by
definition. Use it to keep bulk data out of casual sight and out of a `grep`; never to protect
anything that matters.

## Ratio

Depends entirely on how repetitive the input is. Base64 adds a third, so short or already-dense
strings come out *bigger* — this is for bulk, not for a label:

| input | packed |
|---|---|
| 3440 chars of sprite artwork | 752 (22%) |
| 4000 chars of one repeated pattern | 648 (16%) |
| 5000 chars of low-entropy noise | 836 (17%) |
| 43 chars of prose | 76 (177%) |

`Pack` is a build-time operation — the match search is a straightforward scan of the window,
which is fine for the literals this is meant for but is not something to run on megabytes in a
render loop. `Unpack` is linear and cheap.

## Generating packed literals

`Build.PackPixelSprites` is the CLI that produces the artwork literal, and doubles as a worked
example: it links `PackedText.cs` directly rather than reimplementing it, packs, and then proves
the result unpacks to exactly the input (and that a wrong key is rejected) before writing
anything.

```bash
dotnet run --project Build.PackPixelSprites -- \
    --frames Build.PackPixelSprites/sprite-frames.txt \
    --key    the-key-from-the-samples-project \
    --out    Tesserae/src/Components/PixelAvatar.Sprites.cs
```

It rewrites whatever sits between the `// <packed-frames>` and `// </packed-frames>` markers, so
re-running it is idempotent. Without `--out` it prints the literal and the statistics.

## Related

- The artwork packed this way, and `PixelAvatarSprites.Unlock` — `pixel-avatar.md`
