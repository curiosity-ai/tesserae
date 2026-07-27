using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A single quiet line of live progress text ("Reading documents · Encoding 57%") meant to be
    /// updated many times a second while a long-running task streams its progress.
    /// <para>
    /// The line, and the tooltip carrying its untruncated text, are built once and never rebuilt:
    /// an update writes the new text into the elements already on screen. Nothing fades in or out,
    /// so a stream of updates reads as one line changing rather than a component being replaced.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.LiveProgress")]
    public sealed class LiveProgress : ComponentBase<LiveProgress, HTMLElement>
    {
        private readonly HTMLElement _tooltipContent;
        private          string      _text;
        private          bool        _hasTooltip = true;
        private          bool        _tooltipAttached;
        private          bool        _cleanupRegistered;
        private          IDisposable _subscription;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public LiveProgress(string text = null)
        {
            InnerElement    = Div(Att("tss-liveprogress"));
            _tooltipContent = Div(Att("tss-liveprogress-tooltip"));

            InnerElement.addEventListener("mouseenter", _ => AttachTooltipIfNeeded());

            SetText(text);
        }

        /// <summary>
        /// Gets the text currently shown by the component.
        /// </summary>
        public string Text    => _text;

        /// <summary>
        /// Returns a value indicating whether the component currently shows any progress.
        /// </summary>
        public bool   IsEmpty => string.IsNullOrEmpty(_text);

        /// <summary>
        /// Writes the given progress into the line already on screen. An empty text hides the line
        /// without removing it, so the next update brings back the same element.
        /// </summary>
        public LiveProgress SetText(string text)
        {
            text = text ?? string.Empty;

            if (text == _text) return this;

            _text = text;

            InnerElement.innerText    = _text;
            _tooltipContent.innerText = _text;
            InnerElement.UpdateClassIf(_text.Length == 0, "tss-liveprogress-empty");

            return this;
        }

        /// <summary>
        /// Clears the progress, hiding the line until the next update.
        /// </summary>
        public LiveProgress Clear() => SetText(null);

        /// <summary>
        /// Streams the progress from the given observable: every value it publishes is written into
        /// the line. The subscription is released when the component leaves the DOM, and a second
        /// call replaces the source this component follows.
        /// </summary>
        public LiveProgress Stream(IObservable<string> source)
        {
            _subscription?.Dispose();
            _subscription = null;

            if (source is null) return this;

            _subscription = source.Subscribe(v => SetText(v), fireImmediately: true);

            RegisterCleanup();

            return this;
        }

        /// <summary>
        /// Stops following the observable passed to <see cref="Stream"/>, keeping the text it last wrote.
        /// </summary>
        public LiveProgress StopStreaming()
        {
            _subscription?.Dispose();
            _subscription = null;
            return this;
        }

        /// <summary>
        /// Configures whether hovering the line shows its full text as a tooltip. On by default, as the
        /// line is ellipsized to the width it is given.
        /// </summary>
        public LiveProgress WithTooltip(bool value = true)
        {
            _hasTooltip = value;

            if (!value && _tooltipAttached) DestroyTooltip();

            return this;
        }

        /// <summary>
        /// Configures the component to show no tooltip when hovered.
        /// </summary>
        public LiveProgress NoTooltip() => WithTooltip(false);

        // Attached on the first hover rather than up front: an off-screen progress line in a long
        // transcript never pays for a tippy instance. Once attached it stays, and text updates reach
        // it through the content element it was given - including while the tooltip is open.
        private void AttachTooltipIfNeeded()
        {
            if (!_hasTooltip || _tooltipAttached || IsEmpty) return;

            _tooltipAttached = true;

            document.body.appendChild(_tooltipContent);

            if (!int.TryParse(Layers.AboveCurrent(), out var zIndex)) zIndex = 9999;

            Transpose.Script.Write("tippy({0}, { content: {1}, placement: 'top', delay: [{2},{3}], appendTo: {4}, maxWidth: {5}, arrow: {6}, zIndex: {7} });",
                                   InnerElement, _tooltipContent, 250, 0, document.body, 420, false, zIndex);

            // tippy binds its own hover handlers, and the mouseenter that got us here has already been
            // dispatched - without this the first hover would attach a tooltip nobody sees.
            Transpose.Script.Write("{0}._tippy.show();", InnerElement);

            RegisterCleanup();
        }

        private void DestroyTooltip()
        {
            if (InnerElement.HasOwnProperty("_tippy"))
            {
                Transpose.Script.Write("{0}._tippy.destroy();", InnerElement);
            }

            _tooltipAttached = false;
        }

        private void RegisterCleanup()
        {
            if (_cleanupRegistered) return;

            _cleanupRegistered = true;

            this.WhenRemoved(() =>
            {
                DestroyTooltip();
                StopStreaming();
            });
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
