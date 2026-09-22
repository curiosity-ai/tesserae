using System;

namespace Tesserae
{
    /// <summary>
    /// Implemented by a component that can swap out the element it renders, so that anything applied
    /// to the component from outside can be put back on whatever element it renders next.
    /// </summary>
    /// <remarks>
    /// A component's element is normally the same one for its whole life, and the fluent helpers can
    /// write straight to it. <see cref="DeltaComponent"/> is the exception: it hands out whatever its
    /// content rendered, and replaces that node when the content becomes a different component. A
    /// <c>.Class()</c> written to the old node goes out with it.
    ///
    /// <para>What is recorded is the call, not its result - a closure that applies the same thing
    /// again. Replaying in order gives the right end state for free: a class added and later removed
    /// replays as an add and a remove, and two <c>.Id()</c> calls replay as the second one. It is also
    /// what makes <c>.Style()</c> and <c>.Tooltip()</c> work, where copying the result is either
    /// lossy or impossible - a tooltip is a listener and a tippy instance, not an attribute.</para>
    ///
    /// <para>This is deliberately not <see cref="ISpecialCaseStyling"/>: that one redirects a write to
    /// a different element, and here there is no other element to write to. The write still goes to
    /// the element on screen, and is only remembered in case that element is replaced.</para>
    /// </remarks>
    internal interface IReappliesStyling
    {
        /// <summary>
        /// Records something just applied to this component, so it can be applied again to the
        /// element the component renders after a swap. Ignored while a replay is in progress, so a
        /// replayed call does not record itself a second time.
        /// </summary>
        void RememberStyling(Action reapply);
    }
}
