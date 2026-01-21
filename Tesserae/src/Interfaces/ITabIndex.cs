namespace Tesserae
{
    /// <summary>
    /// Defines a component that has a tab index property.
    /// </summary>
    [H5.Name("tss.ITAB")]
    public interface ITabIndex
    {
        /// <summary>Sets the tab index for the component.</summary>
        int TabIndex { set; }
    }
}