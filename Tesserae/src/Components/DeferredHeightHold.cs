using System;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Keeps a deferred container at its current height while its children are replaced, so the page below it
    /// does not jump up to the loading message and back down once the new content is in.
    /// </summary>
    [Transpose.Name("tss.DHH")]
    internal sealed class DeferredHeightHold
    {
        private readonly HTMLElement _element;
        private          bool        _holding;
        private          int         _version;

        public DeferredHeightHold(HTMLElement element)
        {
            _element = element ?? throw new ArgumentNullException(nameof(element));
        }

        /// <summary>Floors the element at its rendered height. A floor the caller set with MinHeight is theirs and is left alone.</summary>
        public void Hold()
        {
            _version++;

            if (!_holding && _element.hasAttribute(Stack.ExplicitMinHeight)) return;

            // Fractional, so the floor is the height the content really had: offsetHeight rounds, and a rounded-up floor moves what sits below by a pixel.
            var height = ((DOMRect)_element.getBoundingClientRect()).height;

            if (height <= 0) return;

            _element.style.minHeight = height + "px";
            _holding                 = true;
        }

        /// <summary>Drops the floor once the new content has had a frame to lay out and a second for what renders on mount to claim its space.</summary>
        public void Release()
        {
            if (!_holding) return;

            var version = _version;

            window.requestAnimationFrame(_ => window.requestAnimationFrame(__ =>
            {
                if (!_holding || version != _version) return;

                _element.style.minHeight = "";
                _holding                 = false;
            }));
        }
    }
}
