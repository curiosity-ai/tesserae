using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A macOS-Downloads-style stack of page thumbnails: a few overlapping, slightly rotated pages that
    /// fan out when the pointer is over them, with a "+N" badge covering whatever the stack doesn't show.
    /// <para>
    /// It is meant as the preview rail of a search result (see <see cref="OmniResult{T}"/>), so it sizes
    /// itself to the width it needs when <em>fanned</em> and pins the stack to the right edge of that
    /// reserved rail: opening the fan never widens the row it lives in, and the lifted pages draw on top of
    /// whatever is beside them instead of pushing it around.
    /// </para>
    /// <para>
    /// Pages are either image thumbnails (<see cref="PagesStack(string[])"/>) or blank ruled placeholders
    /// (<see cref="PagesStack(int)"/>), for a document whose thumbnails haven't been generated - or aren't
    /// worth generating - yet.
    /// </para>
    /// <para>
    /// Pages are drawn portrait until a thumbnail says otherwise: the first one that loads wider than it is
    /// tall turns the whole stack landscape, so a deck of slides isn't previewed as a pile of A4. See
    /// <see cref="MatchThumbnailShape(bool)"/>.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.PagesStack")]
    public sealed class PagesStack : ComponentBase<PagesStack, HTMLElement>
    {
        // A stack reads as "several pages" well before this; past it the pages are only pixels of each other.
        private const int DEFAULT_MAX_VISIBLE = 5;

        private const int   DEFAULT_PAGE_WIDTH  = 48;
        private const int   DEFAULT_PAGE_HEIGHT = 62;
        private const int   REST_STEP           = 14;  // visible sliver of each page at rest
        private const int   FAN_STEP            = 25;  // visible sliver of each page when fanned
        private const float REST_ROTATION       = 1.2f; // degrees added per page at rest
        private const float FAN_ROTATION        = 7f;   // degrees the outermost pages reach when fanned

        // A landscape page is drawn as wide as a portrait one is tall, so past this the pages would be
        // slivers - a panorama thumbnail should not flatten the stack out of being read as pages.
        private const float MAX_LANDSCAPE_ASPECT = 3f;

        private readonly HTMLElement       _stack;
        private readonly HTMLElement       _more;
        private readonly List<HTMLElement> _pages;

        private List<string> _urls;
        private int          _pageCount;
        private int          _maxVisible = DEFAULT_MAX_VISIBLE;
        private int          _pageWidth  = DEFAULT_PAGE_WIDTH;
        private int          _pageHeight = DEFAULT_PAGE_HEIGHT;
        private Action<int>  _pageClickHandler;
        private bool         _matchThumbnailShape = true;
        private bool         _thumbnailMeasured;
        private float        _landscapeAspect; // 0 until a landscape thumbnail has reported its size

        /// <summary>
        /// Initializes a new instance of this class showing the given thumbnails, at most
        /// <see cref="MaxVisible(int)"/> of them, each scaled to the same page size.
        /// </summary>
        public PagesStack(params string[] imageUrls) : this()
        {
            SetPages(imageUrls);
        }

        /// <summary>
        /// Initializes a new instance of this class showing the given number of blank ruled pages, for a
        /// document with no thumbnails to show. Pass <see cref="TotalPages(int)"/> as well when the document
        /// has more pages than the stack should draw.
        /// </summary>
        public PagesStack(int pages) : this()
        {
            SetPages(pages);
        }

        private PagesStack()
        {
            _more  = Span(Att("tss-pagesstack-more"));
            _stack = Div(Att("tss-pagesstack"), _more);

            // The holder is the reserved rail; the stack inside it is absolutely positioned, so fanning
            // draws outside the rail's flow instead of resizing it.
            InnerElement = Div(Att("tss-pagesstack-holder"), _stack);

            _urls      = new List<string>();
            _pages     = new List<HTMLElement>();
            _pageCount = 0;

            _more.style.display = "none";
        }

        /// <summary>
        /// Gets the number of pages the document has, which is what the "+N" badge counts from - the same
        /// as the number of thumbnails unless <see cref="TotalPages(int)"/> said otherwise.
        /// </summary>
        public int TotalPageCount => _pageCount;

        /// <summary>
        /// Gets the number of pages actually drawn in the stack. A stack given thumbnails draws only those
        /// (the pages past them are counted by the badge rather than faked as blank ones).
        /// </summary>
        public int VisiblePageCount => Math.Min(_urls.Count > 0 ? _urls.Count : _pageCount, _maxVisible);

        /// <summary>
        /// Returns a value indicating whether the stack is held open, i.e. fanned without the pointer
        /// being over it.
        /// </summary>
        public bool IsFanned => InnerElement.classList.contains("tss-pagesstack-fanned");

        /// <summary>
        /// Returns a value indicating whether the pages are drawn landscape, which they are once a
        /// thumbnail has loaded that is wider than it is tall - see
        /// <see cref="MatchThumbnailShape(bool)"/>.
        /// </summary>
        public bool IsLandscape => _landscapeAspect > 0;

        /// <summary>
        /// Gets the width every page is currently drawn at, which is <see cref="PageSize(int,int)"/>'s
        /// width unless a landscape thumbnail has reshaped the pages.
        /// </summary>
        public int DrawnPageWidth => IsLandscape ? LongSide : _pageWidth;

        /// <summary>
        /// Gets the height every page is currently drawn at, which is <see cref="PageSize(int,int)"/>'s
        /// height unless a landscape thumbnail has reshaped the pages.
        /// </summary>
        public int DrawnPageHeight => IsLandscape
            ? Math.Max(1, (int)Math.Round((double)LongSide / _landscapeAspect))
            : _pageHeight;

        // A landscape page keeps the long side of the configured page size and takes its short side from
        // the thumbnail, so a page of a landscape document is the portrait page turned on its side.
        private int LongSide => Math.Max(_pageWidth, _pageHeight);

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        /// <summary>
        /// Replaces the pages with the given thumbnails. The page count - and so the "+N" badge - follows
        /// the number of urls unless <see cref="TotalPages(int)"/> is called afterwards.
        /// </summary>
        public PagesStack SetPages(params string[] imageUrls)
        {
            _urls = new List<string>();

            if (imageUrls != null)
            {
                foreach (var url in imageUrls)
                {
                    if (!string.IsNullOrEmpty(url)) _urls.Add(url);
                }
            }

            _pageCount = _urls.Count;

            ForgetThumbnailShape();

            return Rebuild();
        }

        /// <summary>
        /// Replaces the pages with the given number of blank ruled pages.
        /// </summary>
        public PagesStack SetPages(int pages)
        {
            _urls      = new List<string>();
            _pageCount = pages < 0 ? 0 : pages;

            ForgetThumbnailShape();

            return Rebuild();
        }

        /// <summary>
        /// Sets how many pages the document has in total, for a stack given fewer thumbnails than that: the
        /// pages past the ones drawn are counted by the "+N" badge over the top-right of the stack.
        /// </summary>
        public PagesStack TotalPages(int pages)
        {
            _pageCount = pages < 0 ? 0 : pages;

            return Rebuild();
        }

        /// <summary>
        /// Sets how many pages are drawn before the rest collapse into the "+N" badge. Five by default -
        /// enough for the stack to read as a stack, few enough that the fan stays narrow.
        /// </summary>
        public PagesStack MaxVisible(int count)
        {
            _maxVisible = count < 1 ? 1 : count;

            return Rebuild();
        }

        /// <summary>
        /// Sets the size every page is drawn at, portrait. All pages share one size, whatever their
        /// individual thumbnails' aspect ratios are (a thumbnail is cropped to fill), so the stack reads
        /// as one document - though the shape the pages share follows the document's own, see
        /// <see cref="MatchThumbnailShape(bool)"/>.
        /// </summary>
        public PagesStack PageSize(int width, int height)
        {
            _pageWidth  = width  < 1 ? 1 : width;
            _pageHeight = height < 1 ? 1 : height;

            return Rebuild();
        }

        /// <summary>
        /// Sets whether the pages take their shape from the thumbnails, which they do by default: the
        /// first thumbnail to load that is wider than it is tall turns the pages landscape - keeping the
        /// long side of <see cref="PageSize(int,int)"/> and taking the short one from the thumbnail's
        /// aspect ratio - so a deck of slides isn't drawn as a stack of portrait pages. Pass false to keep
        /// the configured page size whatever the thumbnails turn out to be.
        /// </summary>
        public PagesStack MatchThumbnailShape(bool value = true)
        {
            _matchThumbnailShape = value;

            ForgetThumbnailShape();

            // Rebuilt rather than just resized: the pages that were measured are replaced by ones whose
            // load fires again, so switching this back on re-measures instead of waiting for new urls.
            return Rebuild();
        }

        /// <summary>
        /// Holds the stack open (or lets it close again), for a host that wants the fan to follow hovering
        /// something larger than the stack itself - the result row it sits in, say, which is what
        /// <see cref="OmniResult{T}"/> does by default.
        /// </summary>
        public PagesStack Fanned(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-pagesstack-fanned");

            return this;
        }

        /// <summary>
        /// Makes each drawn page clickable, handing the handler the page's index (0-based) - so opening a
        /// document at the page the user pointed at is one call. The click is the page's alone: it does not
        /// also count as a click on the row the stack sits in. Each page takes a tab stop of its own and
        /// answers Enter and Space. Pass null to make the pages plain again.
        /// </summary>
        public PagesStack OnPageClick(Action<int> onPageClick)
        {
            _pageClickHandler = onPageClick;

            return Rebuild();
        }

        private PagesStack Rebuild()
        {
            ClearChildren(_stack);

            _pages.Clear();

            var shown = VisiblePageCount;

            for (int i = 0; i < shown; i++)
            {
                var page = BuildPage(i, shown);

                _pages.Add(page);
                _stack.appendChild(page);
            }

            ApplyPageMetrics();

            var hidden = _pageCount - shown;

            _more.innerText      = $"+{hidden}";
            _more.style.display  = hidden > 0 ? "" : "none";

            _stack.appendChild(_more);

            InnerElement.UpdateClassIf(shown < 1, "tss-pagesstack-empty");

            return this;
        }

        /// <summary>
        /// Writes the page size and everything measured from it: the rail the fan opens into, and how far
        /// each page is pulled back over the one in front of it. Kept out of <see cref="BuildPage"/> so a
        /// thumbnail that turns out to be landscape can reshape a stack that is already on screen without
        /// the pages - and the thumbnails inside them - being built again.
        /// </summary>
        private void ApplyPageMetrics()
        {
            var shown  = _pages.Count;
            var width  = DrawnPageWidth;
            var height = DrawnPageHeight;

            // The rail is as wide as the fan gets, so the pages have room to open into without the row
            // they sit in reflowing. The few extra pixels are for the rotation of the outermost pages.
            var railWidth = shown < 1 ? 0 : width + (shown - 1) * FAN_STEP + 4;

            // No flex-basis: the rail is a stack item itself now, and flex-basis follows the stack's main
            // axis — the rail width became the rail *height* in a vertical stack. `flex-shrink: 0` in the
            // stylesheet is what keeps a horizontal stack from squeezing it, and with basis left at auto
            // the width and height below are the base size on whichever axis the stack runs.
            InnerElement.style.width  = $"{railWidth}px";
            InnerElement.style.height = $"{height + 12}px";

            for (int i = 0; i < shown; i++)
            {
                var page = _pages[i];

                page.style.width  = $"{width}px";
                page.style.height = $"{height}px";

                // At rest: the first page in place, every other one pulled back over it. The margin is the
                // resting layout and never animates - see the fan shift in BuildPage.
                page.style.setProperty("--tss-pagesstack-rest-offset", i == 0 ? "0px" : $"-{width - REST_STEP}px");
            }
        }

        /// <summary>
        /// Takes the page shape from the first thumbnail to report a natural size. The pages of one
        /// document share one shape - and a stack that reshaped itself once per thumbnail would rewrite
        /// the layout of the row it sits in on every image that arrived - so the first one that loads
        /// speaks for all of them.
        /// </summary>
        private void MeasureThumbnail(HTMLImageElement image)
        {
            if (!_matchThumbnailShape || _thumbnailMeasured) return;

            var w = image.naturalWidth;
            var h = image.naturalHeight;

            // Not decoded yet, or an image that never will be - either way there is nothing to measure.
            if (w <= 0 || h <= 0) return;

            _thumbnailMeasured = true;

            // Portrait and square thumbnails already match the shape the pages are drawn at by default.
            if (w <= h) return;

            var aspect = (float)w / h;

            _landscapeAspect = aspect > MAX_LANDSCAPE_ASPECT ? MAX_LANDSCAPE_ASPECT : aspect;

            ApplyPageMetrics();
        }

        private void ForgetThumbnailShape()
        {
            _thumbnailMeasured = false;
            _landscapeAspect   = 0;
        }

        private HTMLElement BuildPage(int index, int shown)
        {
            var page = Div(Att("tss-pagesstack-page"));

            if (index < _urls.Count)
            {
                var image = Image(Att("tss-pagesstack-image", src: _urls[index]));

                // A thumbnail only knows its own shape once it has loaded, hence measuring on the event
                // rather than here - and hence the second check, for one served from cache and already
                // decoded by the time this runs, whose load event has been and gone.
                image.onload = _ => MeasureThumbnail(image);

                if (image.complete) MeasureThumbnail(image);

                page.appendChild(image);
            }
            else
            {
                page.appendChild(Div(Att("tss-pagesstack-lines")));
            }

            // Page 1 stays on top, so the stack is read from the front - and the sliver of each page
            // behind it shows on the right, the way a pile of paper nudged sideways looks.
            page.style.zIndex = $"{shown - index}";

            // At rest, the pages tilt a little more the further back they are. How far each is pulled back
            // over the one in front depends on the page size, so ApplyPageMetrics writes that part.
            page.style.setProperty("--tss-pagesstack-rest-rotation", $"{REST_ROTATION * index}deg");

            // Fanned: wider gaps, and the pages lifted along a shallow arc, tilting out from the middle.
            var t = shown == 1 ? 0.5f : (float)index / (shown - 1);

            // The wider gaps are a translation rather than a bigger margin. Animating margin-left would
            // relayout the whole row on every frame of the fan - and a row of these sits in every search
            // result - where a transform is handed to the compositor and costs the main thread nothing.
            // The row is pinned to the right, so opening it moves each page left by the gap it gains,
            // times the number of pages between it and the anchored last one.
            page.style.setProperty("--tss-pagesstack-fan-shift",    $"-{(shown - 1 - index) * (FAN_STEP - REST_STEP)}px");
            page.style.setProperty("--tss-pagesstack-fan-rotation", $"{-FAN_ROTATION + 2 * FAN_ROTATION * t:0.##}deg");
            page.style.setProperty("--tss-pagesstack-fan-lift",     $"{-2 - 4 * Math.Sin(Math.PI * t):0.##}px");

            if (_pageClickHandler is object) MakePageClickable(page, index);

            return page;
        }

        private void MakePageClickable(HTMLElement page, int index)
        {
            page.classList.add("tss-pagesstack-page-clickable");
            page.setAttribute("role",       "button");
            page.setAttribute("tabindex",   "0");
            page.setAttribute("aria-label", $"Page {index + 1}");

            page.addEventListener("click", e =>
            {
                StopEvent(e);

                _pageClickHandler?.Invoke(index);
            });

            page.addEventListener("keydown", e =>
            {
                var keyboardEvent = e.As<KeyboardEvent>();

                if (keyboardEvent.key != "Enter" && keyboardEvent.key != " ") return;

                StopEvent(keyboardEvent);

                _pageClickHandler?.Invoke(index);
            });
        }
    }
}
