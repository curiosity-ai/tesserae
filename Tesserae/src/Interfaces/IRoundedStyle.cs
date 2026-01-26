namespace Tesserae
{
    /// <summary>
    /// Defines a component that supports rounded corners.
    /// </summary>
    [H5.Name("tss.IRS")]
    public interface IRoundedStyle : IComponent
    {
    }

    /// <summary>
    /// Represents the border radius of a component.
    /// </summary>
    [H5.Name("tss.BR")]
    public enum BorderRadius
    {
        Small,
        Medium,
        Full
    }
}
