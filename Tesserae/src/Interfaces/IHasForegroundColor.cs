namespace Tesserae
{
    /// <summary>
    /// Defines a component that has a foreground color property.
    /// </summary>
    [H5.Name("tss.IHFG")]
    public interface IHasForegroundColor
    {
        /// <summary>Gets or sets the foreground color.</summary>
        string Foreground { get; set; }
    }
}