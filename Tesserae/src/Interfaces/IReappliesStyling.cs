using System;

namespace Tesserae
{
    /// <summary>
    /// Implemented by a component that can replace the element it renders, so that anything applied
    /// to the component from outside can be put back on whatever element it renders next.
    /// </summary>
    /// <remarks>
    /// A component's element is normally the same one for its whole life, and the fluent helpers can
    /// write straight to it. <see cref="DeltaComponent"/> is the exception: it hands out whatever its
    /// content rendered, and that node is either replaced - when the content becomes a different
    /// component - or patched into the new content's shape. A <c>.Class()</c> written to it is lost
    /// either way: the swap takes the node out, and the patch overwrites its attributes with the
    /// incoming node's.
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
        /// element the component renders next. Ignored while a replay is in progress, so a replayed
        /// call does not record itself a second time.
        /// </summary>
        /// <param name="reapply">Applies the same thing again to whatever the component renders.</param>
        /// <param name="replayAfterPatch">
        /// Whether replaying it after a patch - which happens on every content change, not only on the
        /// rarer swap - is free of side effects. True for a call that writes an attribute and nothing
        /// else, which is exactly what a patch overwrites. False for anything that also attaches
        /// something the patch leaves alone: a tooltip's listener and tippy instance survive a patch,
        /// and calling for another one per frame would stack them up.
        /// </param>
        void RememberStyling(Action reapply, bool replayAfterPatch);
    }
}
