---
name: video-player
description: The browser's video player (native controls) as a component, with typed playback (position, play, pause) and events, subtitles, poster, loop and a no-download option. Use to show a video, or to drive one from C# (a transcript that follows the picture) in a Tesserae (C#/Transpose) app.
---

# VideoPlayer

`VideoPlayer` wraps the browser's own `<video>` with its native controls, so every format the browser plays
works and users get the controls they know. It sizes like any other component, and adds a typed handle on playback.

## Create

`UI.VideoPlayer(string source, string contentType = null)` — the URL, and its media type (`video/mp4`), which lets
the browser skip a file it cannot play without fetching it. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Poster(url)` — an image shown before it plays.
- `.Controls(bool = true)` — the native controls (`Controls(false)` leaves only the picture).
- `.AutoPlay()` — browsers only allow it for a muted video: pair it with `.Muted()`. `.Loop()`, `.PlaysInline()` (stay in the page on a phone).
- `.Preload(MediaPreload.None | Metadata | Auto)` — how much is fetched before play (default `Metadata`).
- `.NoDownload()` — removes the controls' download item and blocks the context menu. It stops the casual save only: the page still fetches the file.
- `.Track(url, label, language, isDefault = false)` — a WebVTT subtitles track.
- `.Contain()` / `.Cover()` / `.Fill()` — how the picture fits a box of another shape (`object-fit`).
- `.SetSource(url, contentType)` — swap the video. `.Source` reads it.

## Playback and events

- `.Play()`, `.Pause()`, `.CurrentTime` (seconds; setting it seeks), `.Duration` (NaN until the header is read), `.Volume` (0 to 1), `.IsMuted`, `.IsPaused`, `.HasEnded`.
- `.OnPlay(Action)`, `.OnPause(Action)`, `.OnEnded(Action)`, `.OnTimeUpdate(Action<double>)` (the position in seconds, about four times a second), `.OnError(Action)`.
- `.Element` — the underlying `HTMLVideoElement`, for what is not wrapped (`requestPictureInPicture`, `playbackRate`).

## Example

```csharp
using static Tesserae.UI;

var position = TextBlock("0.0 s");

var player = VideoPlayer("/media/intro.mp4", "video/mp4")
    .Poster("/media/intro.jpg")
    .OnTimeUpdate(t => position.Text = t.ToString("0.0") + " s")
    .W(480);

var jump = Button("Jump to 1:30").OnClick(() => { player.CurrentTime = 90; player.Play(); });

// A transcript that follows the picture: highlight the excerpt for the current time in OnTimeUpdate,
// and set CurrentTime from a click on an excerpt.
```

## Related

- AudioPlayer — `audio-player.md`
- Image — `image.md`
- Carousel — `carousel.md`
