namespace Tesserae
{
    /// <summary>
    /// Implemented by a component that can tell, at the moment it is hovered, that a tooltip would only
    /// repeat what it is already showing in full - a label whose tooltip is its own text, with none of
    /// that text cut off by an ellipsis. <c>Tooltip(...)</c> asks on every hover, so the same tooltip is
    /// skipped while the text fits and shown once a narrower layout cuts it short.
    /// </summary>
    internal interface ISkipsRedundantTooltip
    {
        /// <summary>
        /// Whether a tooltip saying <paramref name="tooltipText"/> (as plain text) would add nothing to
        /// what the component is showing right now. Called on every hover and every show, so it should be
        /// cheap when nothing has changed since the last call.
        /// </summary>
        bool IsTooltipRedundant(string tooltipText);
    }
}
