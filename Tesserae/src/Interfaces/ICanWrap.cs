namespace Tesserae
{
    /// <summary>
    /// Defines a component that can wrap its items.
    /// </summary>
    [H5.Name("tss.ICW")]
    public interface ICanWrap
    {
        /// <summary>Gets or sets whether the component should wrap its items.</summary>
        bool CanWrap { get; set; }
    }
}