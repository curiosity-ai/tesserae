using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae.Themes.Curiosity
{
    /// <summary>How a <see cref="PixelGlyph"/> plays its frames.</summary>
    public enum PixelGlyphPlayback
    {
        /// <summary>Plays the frames, holds the last one (the glyph at rest) and starts over, while on screen.</summary>
        Loop,
        /// <summary>Plays the frames once, when the glyph first comes into view, and stands on the last one.</summary>
        Once
    }

    /// <summary>One decoded frame of a pixel glyph animation.</summary>
    public sealed class PixelGlyphFrame
    {
        /// <summary>Creates a frame.</summary>
        public PixelGlyphFrame(int[] cells, int duration, string label)
        {
            Cells    = cells;
            Duration = duration;
            Label    = label;
        }

        /// <summary>The 16 cells in reading order (left to right, top to bottom), as palette keys.</summary>
        public int[] Cells { get; }

        /// <summary>How long the frame holds, in milliseconds.</summary>
        public int Duration { get; }

        /// <summary>A caption for the frame, or null to keep the previous one.</summary>
        public string Label { get; }
    }

    /// <summary>
    /// The Curiosity website's 4 by 4 pixel glyph: sixteen squares on a 24 unit grid (20 unit squares), each a
    /// palette tone, playing a short animation as frames. Every glyph the website draws is in
    /// <see cref="PixelGlyphKind"/>; <see cref="Custom(int[], int)"/> takes your own 16 cells, and
    /// <see cref="FromAnimation(string, PixelGlyphPlayback, int)"/> any animation in the notation below.
    ///
    /// <para><b>The animation notation.</b> A string of frames separated by spaces. A frame is 16 cells in reading
    /// order, optionally split into rows by <c>/</c>, then optionally <c>:ms</c> (how long it holds, default 190) and
    /// <c>:label</c> (a caption, <c>_</c> for a space). A cell is a palette key: a digit <c>0</c>-<c>9</c>, an
    /// uppercase letter for 10-35, or the website's letters: <c>.</c> empty (0), <c>k</c> ink (1), <c>s</c> slate (2),
    /// <c>a</c> ash (3), <c>*</c> Signal (4), <c>o</c> hollow (5), <c>d</c> deep blue (6), <c>t</c> stone (7).
    /// For example <c>"k.a./..../..../.... k.a./k.a./.*../....:400"</c>. The last frame is the glyph at rest: it is
    /// what shows with animation off or reduced motion, and what a loop holds between runs.</para>
    ///
    /// <para><b>The palette</b> maps keys to CSS colours (<see cref="DefaultPalette"/> is the website's grey tones and
    /// two blues). Key 0 is drawn as an empty cell and key 5 as a hollow one (an outline in its colour); every
    /// other key is a filled square.</para>
    /// </summary>
    public sealed class PixelGlyph : IComponent
    {
        /// <summary>Palette key of an empty cell.</summary>
        public const int Empty = 0;
        /// <summary>Palette key of the ink (text colour) tone.</summary>
        public const int Ink = 1;
        /// <summary>Palette key of slate.</summary>
        public const int Slate = 2;
        /// <summary>Palette key of ash.</summary>
        public const int Ash = 3;
        /// <summary>Palette key of the Signal.</summary>
        public const int Signal = 4;
        /// <summary>Palette key of a hollow cell (an outline in its colour).</summary>
        public const int Hollow = 5;
        /// <summary>Palette key of the deep blue.</summary>
        public const int Deep = 6;
        /// <summary>Palette key of stone.</summary>
        public const int Stone = 7;

        /// <summary>
        /// The website's palette: ink, slate, ash, stone, the Signal and the deep blue. Ink and hollow read the
        /// page's text colour first (<c>--tss-default-foreground-color</c>), so they follow a dark theme the way the
        /// website's glyphs follow <c>currentColor</c>; slate and the deep blue read <c>--tss-cur-glyph-slate</c> and
        /// <c>--tss-cur-glyph-deep</c>, which the Curiosity theme lightens on the dark canvas, where both are too faint.
        /// </summary>
        public static IReadOnlyDictionary<int, string> DefaultPalette { get; } = new Dictionary<int, string>
        {
            [Empty]  = "transparent",
            [Ink]    = "var(--tss-default-foreground-color, " + BrandColors.Ink + ")",
            [Slate]  = "var(--tss-cur-glyph-slate, " + BrandColors.Slate + ")",
            [Ash]    = BrandColors.Ash,
            [Signal] = BrandColors.Signal,
            [Hollow] = "var(--tss-default-foreground-color, " + BrandColors.Ink + ")",
            [Deep]   = "var(--tss-cur-glyph-deep, " + BrandColors.Deep + ")",
            [Stone]  = BrandColors.Stone,
        };

        private readonly HTMLElement   _container;
        private readonly HTMLElement   _grid;
        private readonly HTMLElement[] _cells = new HTMLElement[16];
        private readonly HTMLElement   _label;

        private List<PixelGlyphFrame>   _frames;
        private string                  _animation;
        private PixelGlyphPlayback      _playback;
        private Dictionary<int, string> _palette = new Dictionary<int, string>();
        private bool                    _animated = true, _showEmpty, _showLabel;
        private int                     _restHold = 1700;
        private int                     _size;

        // Glyphs made one after another start out of step, as the website staggers a grid of them, so a row does
        // not move in unison: a loop waits (n % 6) x 260ms before its first run, a play 250ms + (n % 6) x 180ms.
        private static int _instances;
        private readonly int _order = _instances++ % 6;

        private bool   _mounted, _seen, _playedOnce;
        private int    _frame;
        private double _timer = -1;
        private IntersectionObserver _visibility;

        /// <summary>One of the website's glyphs.</summary>
        public PixelGlyph(PixelGlyphKind glyph, int size = 48) : this(size)
        {
            PixelGlyphCatalog.Get(glyph, out var animation, out var playback, out var showEmpty);
            _showEmpty = showEmpty;
            SetAnimation(animation, playback);
        }

        private PixelGlyph(int size)
        {
            _size = size;
            _grid = Div(Att("tss-pixel-glyph-grid"));
            _grid.style.display             = "grid";
            _grid.style.gridTemplateColumns = "repeat(4, 1fr)";
            _grid.style.gridTemplateRows    = "repeat(4, 1fr)";
            _grid.style.boxSizing           = "border-box";

            for (int i = 0; i < 16; i++)
            {
                var cell = Div(Att("tss-pixel-glyph-cell"));
                cell.style.boxSizing = "border-box";
                _cells[i] = cell;
                _grid.appendChild(cell);
            }

            _label = Div(Att("tss-pixel-glyph-label"));
            _label.style.display       = "none";
            _label.style.fontFamily    = "var(--tss-monospace-font-family, ui-monospace, monospace)";
            _label.style.fontSize      = "11px";
            _label.style.letterSpacing = ".08em";
            _label.style.textTransform = "uppercase";
            _label.style.marginTop     = "8px";
            _label.style.color         = "var(--tss-secondary-foreground-color, " + BrandColors.Mute + ")";
            _label.style.whiteSpace    = "nowrap";

            _container = Div(Att("tss-pixel-glyph"), _grid, _label);
            _container.setAttribute("aria-hidden", "true");
            _container.style.display       = "inline-flex";
            _container.style.flexDirection = "column";
            _container.style.alignItems    = "flex-start";
            ApplySize();

            DomObserver.WhenMounted(_container, OnMounted);
        }

        /// <summary>
        /// A glyph from your own 16 cells (reading order, palette keys, e.g. <see cref="Ink"/> or <see cref="Signal"/>).
        /// It animates by building itself in: a diagonal sweep from the top left lays each cell down behind an ash
        /// leading edge, the Signal cells land last, and it holds, then builds again.
        /// </summary>
        public static PixelGlyph Custom(int[] cells, int size = 48)
        {
            if (cells is null || cells.Length != 16) throw new ArgumentException("A pixel glyph needs exactly 16 cells.", nameof(cells));
            var g = new PixelGlyph(size);
            g.SetAnimation(BuildAnimation(Encode(cells)), PixelGlyphPlayback.Loop);
            return g;
        }

        /// <summary>A glyph that plays an animation written in the notation described on <see cref="PixelGlyph"/>.</summary>
        public static PixelGlyph FromAnimation(string animation, PixelGlyphPlayback playback = PixelGlyphPlayback.Loop, int size = 48)
        {
            var g = new PixelGlyph(size);
            g.SetAnimation(animation, playback);
            return g;
        }

        /// <summary>The animation this glyph plays, in the pixel notation.</summary>
        public string Animation => _animation;

        /// <summary>
        /// Sets the palette: palette keys to CSS colours. Keys you leave out keep <see cref="DefaultPalette"/>'s
        /// colour, so a palette only has to name what it changes.
        /// </summary>
        public PixelGlyph Palette(IDictionary<int, string> palette)
        {
            _palette = palette is null ? new Dictionary<int, string>() : new Dictionary<int, string>(palette);
            Paint(CurrentFrame());
            return this;
        }

        /// <summary>Turns the animation on or off. Off, the glyph stands on its last frame. On by default; reduced motion turns it off whatever this says.</summary>
        public PixelGlyph Animated(bool animated = true)
        {
            _animated = animated;
            Restart();
            return this;
        }

        /// <summary>Turns off the animation. Same as <c>Animated(false)</c>.</summary>
        public PixelGlyph NoAnimation() => Animated(false);

        /// <summary>Whether it loops or plays once. Each website glyph comes with the website's choice.</summary>
        public PixelGlyph Playback(PixelGlyphPlayback playback)
        {
            _playback = playback;
            Restart();
            return this;
        }

        /// <summary>How long a loop holds its last frame before starting over, in milliseconds. 1700 by default, as on the website.</summary>
        public PixelGlyph RestHold(int milliseconds)
        {
            _restHold = Math.Max(0, milliseconds);
            return this;
        }

        /// <summary>Whether empty cells show a hairline square, so the 4 by 4 box stays visible (the website's pixel plays do).</summary>
        public PixelGlyph ShowEmptyCells(bool show = true)
        {
            _showEmpty = show;
            Paint(CurrentFrame());
            return this;
        }

        /// <summary>Shows the frames' captions under the glyph, in the mono label voice (the pixel plays carry them).</summary>
        public PixelGlyph ShowLabel(bool show = true)
        {
            _showLabel = show;
            _label.style.display = show ? "block" : "none";
            Paint(CurrentFrame());
            return this;
        }

        /// <summary>The glyph's width and height in pixels. 48 by default.</summary>
        public PixelGlyph Size(int size)
        {
            _size = Math.Max(8, size);
            ApplySize();
            Paint(CurrentFrame());
            return this;
        }

        /// <summary>Plays the animation again from its first frame.</summary>
        public PixelGlyph Replay()
        {
            _playedOnce = false;
            Restart();
            return this;
        }

        /// <inheritdoc />
        public HTMLElement Render() => _container;

        /// <summary>Encodes 16 palette keys as one frame of the notation (digits, or letters for keys over 9).</summary>
        public static string Encode(int[] cells)
        {
            var sb = new StringBuilder(19);
            for (int i = 0; i < cells.Length; i++)
            {
                if (i > 0 && i % 4 == 0) sb.Append('/');
                var k = cells[i];
                if (k < 0 || k > 35) throw new ArgumentOutOfRangeException(nameof(cells), "Palette keys run from 0 to 35.");
                sb.Append(k < 10 ? (char)('0' + k) : (char)('A' + k - 10));
            }
            return sb.ToString();
        }

        /// <summary>Decodes an animation in the pixel notation into its frames.</summary>
        public static List<PixelGlyphFrame> Decode(string animation)
        {
            var frames = new List<PixelGlyphFrame>();
            if (string.IsNullOrWhiteSpace(animation)) return frames;

            foreach (var token in animation.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = token.Split(':');
                var cells = new int[16];
                var n     = 0;

                foreach (var ch in parts[0])
                {
                    if (ch == '/') continue;
                    if (n >= 16) throw new FormatException($"Pixel glyph frame '{token}' has more than 16 cells.");
                    cells[n++] = KeyOf(ch, token);
                }

                if (n != 16) throw new FormatException($"Pixel glyph frame '{token}' has {n} cells, not 16.");

                var ms    = parts.Length > 1 && int.TryParse(parts[1], out var d) ? d : 190;
                var label = parts.Length > 2 ? parts[2].Replace('_', ' ') : null;
                frames.Add(new PixelGlyphFrame(cells, ms, label));
            }

            return frames;
        }

        /// <summary>
        /// The build-in animation for a static glyph (one frame in the notation): a diagonal sweep from the top left
        /// lays each cell down, an ash leading edge one step ahead of it, and the Signal cells land last. This is how
        /// the website's static glyphs and every <see cref="Custom(int[], int)"/> glyph animate.
        /// </summary>
        public static string BuildAnimation(string glyph)
        {
            var target = Decode(glyph).Last().Cells;
            var sb     = new StringBuilder();
            var accent = target.Any(k => k == Signal);

            sb.Append("................:220");

            for (int d = 0; d <= 7; d++)
            {
                var frame = new int[16];
                for (int i = 0; i < 16; i++)
                {
                    var k    = target[i];
                    var diag = (i % 4) + (i / 4);
                    if (k == Empty || k == Signal) continue;
                    if (diag < d)       frame[i] = k;
                    else if (diag == d) frame[i] = Ash;
                }
                sb.Append(' ').Append(Encode(frame)).Append(":70");
            }

            // Everything but the Signal, settled; then the Signal lands.
            var settled = target.Select(k => k == Signal ? Empty : k).ToArray();
            sb.Append(' ').Append(Encode(settled)).Append(accent ? ":320" : "");
            if (accent) sb.Append(' ').Append(Encode(target));

            return sb.ToString();
        }

        private static int KeyOf(char ch, string token)
        {
            if (ch >= '0' && ch <= '9') return ch - '0';
            if (ch >= 'A' && ch <= 'Z') return ch - 'A' + 10;
            switch (ch)
            {
                case '.': return Empty;
                case 'k': return Ink;
                case 's': return Slate;
                case 'a': return Ash;
                case '*': return Signal;
                case 'o': return Hollow;
                case 'd': return Deep;
                case 't': return Stone;
                default: throw new FormatException($"Pixel glyph frame '{token}' has an unknown cell '{ch}'.");
            }
        }

        private void SetAnimation(string animation, PixelGlyphPlayback playback)
        {
            _animation = animation;
            _frames    = Decode(animation);
            if (_frames.Count == 0) _frames.Add(new PixelGlyphFrame(new int[16], 190, null));
            _playback  = playback;
            _frame     = _frames.Count - 1;
            Paint(_frames[_frame]);
        }

        private string ColorOf(int key)
        {
            if (_palette.TryGetValue(key, out var c)) return c;
            return DefaultPalette.TryGetValue(key, out var d) ? d : "transparent";
        }

        private void ApplySize()
        {
            // The website's construction: a 96 unit box, 24 unit cells with 20 unit squares.
            _grid.style.width   = _size + "px";
            _grid.style.height  = _size + "px";
            _grid.style.gap     = (_size * 4.0 / 96.0) + "px";
            _grid.style.padding = (_size * 2.0 / 96.0) + "px";
        }

        private PixelGlyphFrame CurrentFrame() => _frames is null ? null : _frames[Math.Min(_frame, _frames.Count - 1)];

        private void Paint(PixelGlyphFrame frame)
        {
            if (frame is null) return;
            var line = Math.Max(1, _size / 48.0 * 1.5);

            for (int i = 0; i < 16; i++)
            {
                var key  = frame.Cells[i];
                var cell = _cells[i];

                if (key == Empty)
                {
                    cell.style.background = "transparent";
                    cell.style.boxShadow  = _showEmpty ? "inset 0 0 0 1px " + BrandColors.Ink2 : "none";
                }
                else if (key == Hollow)
                {
                    cell.style.background = "transparent";
                    cell.style.boxShadow  = "inset 0 0 0 " + line + "px " + ColorOf(Hollow);
                }
                else
                {
                    cell.style.background = ColorOf(key);
                    cell.style.boxShadow  = "none";
                }
            }

            if (_showLabel && frame.Label is object) _label.textContent = frame.Label;
            else if (_showLabel && string.IsNullOrEmpty(_label.textContent)) _label.textContent = LastLabel();
        }

        private string LastLabel()
        {
            for (int i = _frames.Count - 1; i >= 0; i--) if (_frames[i].Label is object) return _frames[i].Label;
            return "";
        }

        private bool IsAnimated => _animated && !BrandMotion.PrefersReducedMotion && _frames.Count > 1;

        private void OnMounted()
        {
            _mounted = true;

            _visibility = new IntersectionObserver((entries, obs) =>
            {
                var seen = entries[entries.Length - 1].isIntersecting;
                if (seen == _seen) return;
                _seen = seen;
                if (seen) Start();
                else if (_playback == PixelGlyphPlayback.Loop) Stop(rest: true);
            });
            _visibility.observe(_container);

            DomObserver.WhenRemoved(_container, () =>
            {
                _mounted = false;
                _seen    = false;
                Stop(rest: true);
                _visibility?.disconnect();
                _visibility = null;
                DomObserver.WhenMounted(_container, OnMounted);
            });
        }

        private void Restart()
        {
            Stop(rest: true);
            if (_mounted && _seen) Start();
        }

        private void Start()
        {
            if (!IsAnimated) { Stop(rest: true); return; }
            if (_timer >= 0) return;
            if (_playback == PixelGlyphPlayback.Once && _playedOnce) return;

            if (_playback == PixelGlyphPlayback.Loop)
            {
                // Hold the rest frame for the stagger, then Advance starts the loop from the first frame.
                _frame = _frames.Count - 1;
                Paint(_frames[_frame]);
                Schedule(_order * 260);
            }
            else
            {
                _frame = 0;
                Paint(_frames[0]);
                Schedule(250 + _order * 180 + _frames[0].Duration);
            }
        }

        private void Schedule(int ms)
        {
            _timer = window.setTimeout(_ => Advance(), ms);
        }

        private void Advance()
        {
            _timer = -1;
            if (!_mounted || !IsAnimated) return;

            if (_frame >= _frames.Count - 1)
            {
                // Back to the start after the rest hold, or stay put for a play-once.
                if (_playback == PixelGlyphPlayback.Once) { _playedOnce = true; return; }
                if (!_seen) return;
                _frame = 0;
                Paint(_frames[0]);
                Schedule(_frames[0].Duration);
                return;
            }

            _frame++;
            Paint(_frames[_frame]);
            var last = _frame == _frames.Count - 1;
            Schedule(last ? (_playback == PixelGlyphPlayback.Loop ? _restHold : 0) : _frames[_frame].Duration);
        }

        private void Stop(bool rest)
        {
            if (_timer >= 0) { window.clearTimeout(_timer); _timer = -1; }
            if (rest && _frames is object)
            {
                _frame = _frames.Count - 1;
                Paint(_frames[_frame]);
            }
        }
    }
}
