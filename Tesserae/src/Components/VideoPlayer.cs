using System;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>How much of a media file the browser fetches before the user presses play.</summary>
    public enum MediaPreload
    {
        /// <summary>Nothing until play.</summary>
        None,
        /// <summary>Only what is needed to know the duration and the first frame (the default).</summary>
        Metadata,
        /// <summary>Whatever the browser thinks is best, usually the whole file.</summary>
        Auto
    }

    /// <summary>
    /// A video player: the browser's own <c>&lt;video&gt;</c> with its native controls, wrapped so it sizes like any other component
    /// and so its playback can be read and driven from C# (a transcript that follows the picture, a "jump to 1:30" link).
    /// </summary>
    [Transpose.Name("tss.VideoPlayer")]
    public class VideoPlayer : ComponentBase<VideoPlayer, HTMLVideoElement>, ISpecialCaseStyling
    {
        private HTMLSourceElement _source;

        /// <summary>The styling container.</summary>
        public HTMLElement StylingContainer => InnerElement;

        /// <summary>Sizing helpers are tagged so a wrapper-building container can hoist them.</summary>
        public bool PropagateStylesToWrapper { get; private set; }

        /// <summary>Initializes a new video player.</summary>
        /// <param name="source">The URL of the video.</param>
        /// <param name="contentType">Its media type (<c>video/mp4</c>), which lets the browser skip a file it cannot play without fetching it.</param>
        public VideoPlayer(string source, string contentType = null)
        {
            InnerElement = new HTMLVideoElement { controls = true };
            InnerElement.classList.add("tss-videoplayer");
            InnerElement.setAttribute("preload", "metadata");

            SetSource(source, contentType);
            PropagateStylesToWrapper = true;
        }

        /// <summary>The underlying <c>&lt;video&gt;</c> element, for what this class does not wrap (<c>requestPictureInPicture</c>, <c>playbackRate</c>).</summary>
        public HTMLVideoElement Element => InnerElement;

        /// <summary>The URL being played.</summary>
        public string Source => _source is null ? "" : _source.src;

        /// <summary>Replaces the video.</summary>
        public VideoPlayer SetSource(string source, string contentType = null)
        {
            if (_source is object) InnerElement.removeChild(_source);

            _source = new HTMLSourceElement { src = source };
            if (!string.IsNullOrEmpty(contentType)) _source.type = contentType;

            InnerElement.appendChild(_source);
            InnerElement.load();
            return this;
        }

        // ------------------------------------------------------------------ configuration

        /// <summary>An image shown before the video plays.</summary>
        public VideoPlayer Poster(string url)
        {
            InnerElement.poster = url;
            return this;
        }

        /// <summary>Shows or hides the browser's controls (on by default).</summary>
        public VideoPlayer Controls(bool show = true)
        {
            InnerElement.controls = show;
            return this;
        }

        /// <summary>Starts playing as soon as it can. Browsers only allow this for a muted video; call <see cref="Muted"/> as well.</summary>
        public VideoPlayer AutoPlay(bool autoPlay = true)
        {
            InnerElement.autoplay = autoPlay;
            return this;
        }

        /// <summary>Starts over when it ends.</summary>
        public VideoPlayer Loop(bool loop = true)
        {
            InnerElement.loop = loop;
            return this;
        }

        /// <summary>Plays without sound.</summary>
        public VideoPlayer Muted(bool muted = true)
        {
            InnerElement.muted = muted;
            return this;
        }

        /// <summary>Plays in the page rather than full screen on a phone.</summary>
        public VideoPlayer PlaysInline(bool inline = true)
        {
            if (inline) InnerElement.setAttribute("playsinline", "");
            else InnerElement.removeAttribute("playsinline");
            return this;
        }

        /// <summary>How much the browser fetches before play.</summary>
        public VideoPlayer Preload(MediaPreload preload)
        {
            InnerElement.setAttribute("preload", preload == MediaPreload.None ? "none" : preload == MediaPreload.Auto ? "auto" : "metadata");
            return this;
        }

        /// <summary>
        /// Takes the download item out of the browser's controls and blocks the context menu, for a video the page should be seen and not saved
        /// from. It stops the casual save, not a determined one: the file is still fetched by the page.
        /// </summary>
        public VideoPlayer NoDownload(bool noDownload = true)
        {
            MediaElements.NoDownload(InnerElement, noDownload);
            return this;
        }

        /// <summary>Adds a subtitles or captions track.</summary>
        /// <param name="url">A WebVTT file.</param>
        /// <param name="label">What the browser's captions menu calls it.</param>
        /// <param name="language">BCP 47 language tag (<c>en</c>, <c>pt-BR</c>).</param>
        /// <param name="isDefault">Whether it is on from the start.</param>
        public VideoPlayer Track(string url, string label, string language, bool isDefault = false)
        {
            var track = document.createElement("track");
            track.setAttribute("kind", "subtitles");
            track.setAttribute("src", url);
            track.setAttribute("label", label);
            track.setAttribute("srclang", language);
            if (isDefault) track.setAttribute("default", "");
            InnerElement.appendChild(track);
            return this;
        }

        /// <summary>How the picture fits the player's box when the two have different shapes.</summary>
        public VideoPlayer Contain() { InnerElement.style.objectFit = "contain"; return this; }

        /// <summary>Fills the box, cropping the picture.</summary>
        public VideoPlayer Cover() { InnerElement.style.objectFit = "cover"; return this; }

        /// <summary>Fills the box, stretching the picture.</summary>
        public VideoPlayer Fill() { InnerElement.style.objectFit = "fill"; return this; }

        // ------------------------------------------------------------------ playback

        /// <summary>Starts or resumes playback. A browser may refuse before the user has interacted with the page.</summary>
        public VideoPlayer Play()
        {
            InnerElement.play();
            return this;
        }

        /// <summary>Pauses.</summary>
        public VideoPlayer Pause()
        {
            InnerElement.pause();
            return this;
        }

        /// <summary>Whether playback is paused (it is until it starts, and after it ends).</summary>
        public bool IsPaused => InnerElement.paused;

        /// <summary>Whether the video played to its end.</summary>
        public bool HasEnded => InnerElement.ended;

        /// <summary>The playback position, in seconds. Setting it seeks.</summary>
        public double CurrentTime
        {
            get => InnerElement.currentTime;
            set => InnerElement.currentTime = value;
        }

        /// <summary>The length in seconds, or NaN until the browser has read the file's header.</summary>
        public double Duration => InnerElement.duration;

        /// <summary>Volume from 0 to 1.</summary>
        public double Volume
        {
            get => InnerElement.volume;
            set => InnerElement.volume = value;
        }

        /// <summary>Whether the sound is off.</summary>
        public bool IsMuted
        {
            get => InnerElement.muted;
            set => InnerElement.muted = value;
        }

        // ------------------------------------------------------------------ events

        /// <summary>Raised when playback starts or resumes.</summary>
        public VideoPlayer OnPlay(Action handler)
        {
            InnerElement.addEventListener("play", _ => handler());
            return this;
        }

        /// <summary>Raised when playback pauses.</summary>
        public VideoPlayer OnPause(Action handler)
        {
            InnerElement.addEventListener("pause", _ => handler());
            return this;
        }

        /// <summary>Raised when the video plays to its end.</summary>
        public VideoPlayer OnEnded(Action handler)
        {
            InnerElement.addEventListener("ended", _ => handler());
            return this;
        }

        /// <summary>Raised as the position moves, with the new position in seconds (about four times a second while playing, and after a seek).</summary>
        public VideoPlayer OnTimeUpdate(Action<double> handler)
        {
            InnerElement.addEventListener("timeupdate", _ => handler(InnerElement.currentTime));
            return this;
        }

        /// <summary>Raised when the file cannot be loaded or played.</summary>
        public VideoPlayer OnError(Action handler)
        {
            InnerElement.addEventListener("error", _ => handler());
            return this;
        }

        /// <summary>Renders the component's root element.</summary>
        public override HTMLElement Render() => InnerElement;
    }

    /// <summary>What the two media players share.</summary>
    internal static class MediaElements
    {
        public static void NoDownload(HTMLMediaElement element, bool noDownload)
        {
            if (noDownload)
            {
                element.setAttribute("controlslist", "nodownload");
                element.oncontextmenu = e => e.preventDefault();
            }
            else
            {
                element.removeAttribute("controlslist");
                element.oncontextmenu = null;
            }
        }

        /// <summary>Seconds as <c>m:ss</c>, or <c>h:mm:ss</c> from an hour.</summary>
        public static string FormatTime(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0) seconds = 0;

            var total = (int)seconds;
            var h = total / 3600;
            var m = (total % 3600) / 60;
            var s = total % 60;

            return h > 0 ? h + ":" + Two(m) + ":" + Two(s) : Two(m) + ":" + Two(s);
        }

        private static string Two(int v) => v < 10 ? "0" + v : v.ToString();
    }
}
