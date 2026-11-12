---
name: audio-player
description: An audio player whose waveform is the seek bar, with a play button, position and length, typed playback and events, theme-aware colours and a native-controls fallback. Use to play a recording, or to drive one from C# (a transcript that follows the audio) in a Tesserae (C#/Transpose) app.
---

# AudioPlayer

`AudioPlayer` shows a play button, `position / length`, and the whole recording as a waveform you click to seek. The sound
is the browser's own `<audio>`, so every format it plays works; `Element` is there for anything not wrapped.
The waveform is drawn by [wavesurfer.js](https://wavesurfer.xyz), shipped as an unlinked resource (`tss-wavesurfer.min.js`) and
fetched the first time a player is on the page, so an application that never plays audio never downloads it. When the waveform
cannot be drawn (the script did not load, the audio cannot be decoded from another origin without CORS) the player shows the
browser's native controls instead of nothing.

## Create

`UI.AudioPlayer(string source, string contentType = null)` — the URL, and its media type (`audio/mpeg`). Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

- `.WaveformHeight(px)` (80 by default).
- `.WaveformColors(wave, progress, cursor = null)` — CSS colours or theme tokens. Without it the waveform follows the theme
  (the played part and the cursor in the primary colour, the rest in the text colour at low opacity) and recolours on a theme change.
- `.NoWaveform()` — the browser's own audio bar, nothing extra fetched.
- `.AutoPlay()` (after the user has interacted with the page), `.Loop()`, `.Muted()`, `.Preload(MediaPreload)`, `.NoDownload()` (casual save only).
- `.SetSource(url, contentType)` — swap the audio; the waveform is drawn again.

## Playback and events

- `.Play()`, `.Pause()`, `.PlayOrPause()`, `.CurrentTime` (seconds; setting it seeks), `.Duration`, `.Volume` (0 to 1), `.IsMuted`, `.IsPaused`.
- `.OnPlay(Action)`, `.OnPause(Action)`, `.OnEnded(Action)`, `.OnTimeUpdate(Action<double>)`, `.OnError(Action)`.
- `.Element` — the underlying `HTMLAudioElement`.

The player can leave the page and come back (a parent that detaches rather than hides): its waveform is torn down on removal and drawn again on re-mount.

## Example

```csharp
using static Tesserae.UI;

var player = AudioPlayer(url, "audio/mpeg").WaveformHeight(100);

// A transcript: each excerpt knows where it starts.
player.OnTimeUpdate(t => HighlightExcerptAt(t));
var jump = Button().SetIcon(UIcons.Play).Tooltip("Play from 0:42").OnClick(() => { player.CurrentTime = 42; player.Play(); });
```

## Related

- VideoPlayer — `video-player.md`
- Avatar — `avatar.md`
