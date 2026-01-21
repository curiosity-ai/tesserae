namespace Tesserae
{
    /// <summary>
    /// Defines a component that has margin and padding properties.
    /// </summary>
    [H5.Name("tss.IHMP")]
    public interface IHasMarginPadding //TODO: Change interface to match methods for padding/margin on StackExtensions
    {
        /// <summary>Gets or sets the margin.</summary>
        string Margin  { get; set; }
        /// <summary>Gets or sets the padding.</summary>
        string Padding { get; set; }
    }
}