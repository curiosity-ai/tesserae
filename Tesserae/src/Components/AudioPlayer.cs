using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// An audio player with a waveform: a play button, the position and length, and the whole recording drawn as a waveform that is also the
    /// seek bar. The sound is the browser's own <c>&lt;audio&gt;</c>, exposed through <see cref="Element"/> and the playback members, so a
    /// transcript can follow it. The waveform is drawn by <see href="https://wavesurfer.xyz">wavesurfer.js</see>, fetched the first time a player is
    /// on the page; when it cannot be drawn (the script did not load, the audio cannot be decoded from another origin) the player falls back to the
    /// browser's native controls instead of showing nothing.
    /// </summary>
    [Transpose.Name("tss.AudioPlayer")]
    public class AudioPlayer : ComponentBase<AudioPlayer, HTMLDivElement>, ISpecialCaseStyling
    {
        private const int DEFAULT_WAVEFORM_HEIGHT = 80;

        private readonly HTMLAudioElement _audio;
        private readonly HTMLElement      _controlsRow;
        private readonly HTMLElement      _time;
        private readonly HTMLElement      _wave;
        private readonly Button           _playButton;

        private HTMLSourceElement _source;
        private object            _wavesurfer;
        private bool              _mountRequested;
        private bool              _waveform = true;
        private bool              _failed;
        private int               _waveformHeight = DEFAULT_WAVEFORM_HEIGHT;
        private string            _waveColor, _progressColor, _cursorColor;

        /// <summary>The styling container.</summary>
        public HTMLElement StylingContainer => InnerElement;

        /// <summary>Sizing helpers are tagged so a wrapper-building container can hoist them.</summary>
        public bool PropagateStylesToWrapper { get; private set; }

        /// <summary>Initializes a new audio player.</summary>
        /// <param name="source">The URL of the audio.</param>
        /// <param name="contentType">Its media type (<c>audio/mpeg</c>).</param>
        public AudioPlayer(string source, string contentType = null)
        {
            _audio = new HTMLAudioElement { controls = false };
            _audio.setAttribute("preload", "metadata");
            _audio.setAttribute("crossorigin", "anonymous");   // the waveform decodes the file itself, which a cross-origin response only allows with CORS

            _playButton = Button().SetIcon(UIcons.Play).NoBorder().Tooltip("Play");
            _playButton.OnClick(() => PlayOrPause());

            _time = document.createElement("span");
            _time.className = "tss-audioplayer-time";
            _time.textContent = "00:00 / 00:00";

            _controlsRow = Div(Att("tss-audioplayer-controls"), _playButton.Render(), _time);
            _wave = Div(Att("tss-audioplayer-wave"));

            InnerElement = Div(Att("tss-audioplayer"), _audio, _controlsRow, _wave);
            PropagateStylesToWrapper = true;

            SetSource(source, contentType);

            _audio.addEventListener("play", _ => ShowPlaying(true));
            _audio.addEventListener("pause", _ => ShowPlaying(false));
            _audio.addEventListener("ended", _ => ShowPlaying(false));
            _audio.addEventListener("timeupdate", _ => ShowTime());
            _audio.addEventListener("loadedmetadata", _ => ShowTime());
            _audio.addEventListener("durationchange", _ => ShowTime());
        }

        /// <summary>The underlying <c>&lt;audio&gt;</c> element, for what this class does not wrap.</summary>
        public HTMLAudioElement Element => _audio;

        /// <summary>The URL being played.</summary>
        public string Source => _source is null ? "" : _source.src;

        /// <summary>Replaces the audio. The waveform is drawn again for it.</summary>
        public AudioPlayer SetSource(string source, string contentType = null)
        {
            if (_source is object) _audio.removeChild(_source);

            _source = new HTMLSourceElement { src = source };
            if (!string.IsNullOrEmpty(contentType)) _source.type = contentType;

            _audio.appendChild(_source);
            _audio.load();

            if (_wavesurfer is object)
            {
                DestroyWaveform();
                CreateWaveform();
            }

            return this;
        }

        // ------------------------------------------------------------------ configuration

        /// <summary>The height of the waveform in pixels (80 by default).</summary>
        public AudioPlayer WaveformHeight(int pixels)
        {
            _waveformHeight = pixels;
            _wave.style.minHeight = pixels + "px";

            if (_wavesurfer is object)
            {
                DestroyWaveform();
                CreateWaveform();
            }
            return this;
        }

        /// <summary>
        /// The colours the waveform is drawn in, as CSS colours (a theme token works). Without this it follows the theme: the played part and the
        /// cursor in the primary colour, the rest in the text colour at low opacity.
        /// </summary>
        public AudioPlayer WaveformColors(string wave, string progress, string cursor = null)
        {
            _waveColor     = wave;
            _progressColor = progress;
            _cursorColor   = cursor ?? progress;
            ApplyColors();
            return this;
        }

        /// <summary>Shows the browser's own audio controls instead of the waveform (nothing is fetched for it).</summary>
        public AudioPlayer NoWaveform(bool noWaveform = true)
        {
            _waveform = !noWaveform;
            if (noWaveform) UseNativeControls();
            else if (!_failed) UseWaveformControls();
            return this;
        }

        /// <summary>Starts playing as soon as it can. Browsers only allow this after the user has interacted with the page.</summary>
        public AudioPlayer AutoPlay(bool autoPlay = true)
        {
            _audio.autoplay = autoPlay;
            return this;
        }

        /// <summary>Starts over when it ends.</summary>
        public AudioPlayer Loop(bool loop = true)
        {
            _audio.loop = loop;
            return this;
        }

        /// <summary>Plays without sound.</summary>
        public AudioPlayer Muted(bool muted = true)
        {
            _audio.muted = muted;
            return this;
        }

        /// <summary>How much the browser fetches before play.</summary>
        public AudioPlayer Preload(MediaPreload preload)
        {
            _audio.setAttribute("preload", preload == MediaPreload.None ? "none" : preload == MediaPreload.Auto ? "auto" : "metadata");
            return this;
        }

        /// <summary>Blocks the context menu's save item and the native controls' download button. It stops the casual save only.</summary>
        public AudioPlayer NoDownload(bool noDownload = true)
        {
            MediaElements.NoDownload(_audio, noDownload);
            return this;
        }

        // ------------------------------------------------------------------ playback

        /// <summary>Starts or resumes playback.</summary>
        public AudioPlayer Play()
        {
            _audio.play();
            return this;
        }

        /// <summary>Pauses.</summary>
        public AudioPlayer Pause()
        {
            _audio.pause();
            return this;
        }

        /// <summary>Plays when paused, pauses when playing.</summary>
        public AudioPlayer PlayOrPause()
        {
            if (_audio.paused) _audio.play(); else _audio.pause();
            return this;
        }

        /// <summary>Whether playback is paused.</summary>
        public bool IsPaused => _audio.paused;

        /// <summary>The playback position, in seconds. Setting it seeks.</summary>
        public double CurrentTime
        {
            get => _audio.currentTime;
            set => _audio.currentTime = value;
        }

        /// <summary>The length in seconds, or NaN until the browser has read the file's header.</summary>
        public double Duration => _audio.duration;

        /// <summary>Volume from 0 to 1.</summary>
        public double Volume
        {
            get => _audio.volume;
            set => _audio.volume = value;
        }

        /// <summary>Whether the sound is off.</summary>
        public bool IsMuted
        {
            get => _audio.muted;
            set => _audio.muted = value;
        }

        // ------------------------------------------------------------------ events

        /// <summary>Raised when playback starts or resumes.</summary>
        public AudioPlayer OnPlay(Action handler)
        {
            _audio.addEventListener("play", _ => handler());
            return this;
        }

        /// <summary>Raised when playback pauses.</summary>
        public AudioPlayer OnPause(Action handler)
        {
            _audio.addEventListener("pause", _ => handler());
            return this;
        }

        /// <summary>Raised when the audio plays to its end.</summary>
        public AudioPlayer OnEnded(Action handler)
        {
            _audio.addEventListener("ended", _ => handler());
            return this;
        }

        /// <summary>Raised as the position moves, with the new position in seconds.</summary>
        public AudioPlayer OnTimeUpdate(Action<double> handler)
        {
            _audio.addEventListener("timeupdate", _ => handler(_audio.currentTime));
            return this;
        }

        /// <summary>Raised when the file cannot be loaded or played.</summary>
        public AudioPlayer OnError(Action handler)
        {
            _audio.addEventListener("error", _ => handler());
            return this;
        }

        // ------------------------------------------------------------------ the waveform

        /// <summary>Renders the component's root element.</summary>
        public override HTMLElement Render()
        {
            if (!_mountRequested)
            {
                _mountRequested = true;
                ArmMountObserver();
            }

            return InnerElement;
        }

        // DomObserver's mount and removal callbacks fire once each. Teardown re-arms the mount one, so a player taken out of the page and put back
        // draws its waveform again instead of staying an empty box.
        private void ArmMountObserver() => DomObserver.WhenMounted(InnerElement, OnMounted);

        private void OnMounted()
        {
            Theme.OnThemeChanged += ApplyColors;
            DomObserver.WhenRemoved(InnerElement, OnRemoved);

            if (_waveform && !_failed) CreateWaveform();
        }

        private void OnRemoved()
        {
            Theme.OnThemeChanged -= ApplyColors;
            DestroyWaveform();
            ArmMountObserver();
        }

        private async void CreateWaveform()
        {
            if (_wavesurfer is object || !InnerElement.IsMounted()) return;

            try
            {
                await WaveSurfer.LoadAsync();
            }
            catch (Exception)
            {
                Fail();
                return;
            }

            // The player may have left the page, or drawn its waveform from another call, while the script loaded.
            if (_wavesurfer is object || !InnerElement.IsMounted() || !_waveform) return;

            _wave.style.minHeight = _waveformHeight + "px";
            _wavesurfer = WaveSurfer.Create(_wave, _audio, Colour(_waveColor, Theme.Default.Foreground, 0.35), Colour(_progressColor, Theme.Primary.Background, 1), Colour(_cursorColor, Theme.Primary.Background, 1), _waveformHeight);
            WaveSurfer.OnError(_wavesurfer, _ => Fail());
        }

        private void DestroyWaveform()
        {
            if (_wavesurfer is null) return;

            WaveSurfer.Destroy(_wavesurfer);
            _wavesurfer = null;
            ClearChildren(_wave);
        }

        private void Fail()
        {
            _failed = true;
            DestroyWaveform();
            UseNativeControls();
        }

        private string Colour(string explicitColour, string token, double alpha)
            => WaveSurfer.Resolve(InnerElement, explicitColour ?? token, explicitColour is null ? alpha : 1);

        private void ApplyColors()
        {
            if (_wavesurfer is null) return;
            WaveSurfer.SetColors(_wavesurfer, Colour(_waveColor, Theme.Default.Foreground, 0.35), Colour(_progressColor, Theme.Primary.Background, 1), Colour(_cursorColor, Theme.Primary.Background, 1));
        }

        private void UseNativeControls()
        {
            DestroyWaveform();
            _controlsRow.style.display = "none";
            _wave.style.display = "none";
            _audio.controls = true;
            _audio.classList.add("tss-audioplayer-native");
        }

        private void UseWaveformControls()
        {
            _controlsRow.style.display = "";
            _wave.style.display = "";
            _audio.controls = false;
            _audio.classList.remove("tss-audioplayer-native");
            if (InnerElement.IsMounted()) CreateWaveform();
        }

        private void ShowPlaying(bool playing)
        {
            _playButton.SetIcon(playing ? UIcons.Pause : UIcons.Play);
            _playButton.Tooltip(playing ? "Pause" : "Play");
        }

        private void ShowTime() => _time.textContent = MediaElements.FormatTime(_audio.currentTime) + " / " + MediaElements.FormatTime(_audio.duration);
    }
}
