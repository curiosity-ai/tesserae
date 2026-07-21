namespace Tesserae
{
    /// <summary>
    /// Defines a component that has a background color property.
    /// </summary>
    [Transpose.Name("tss.IHBG")]
    public interface IHasBackgroundColor
    {
        /// <summary>Gets or sets the background color.</summary>
        string Background { get; set; }
    }
}