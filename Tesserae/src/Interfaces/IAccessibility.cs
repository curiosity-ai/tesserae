namespace Tesserae
{
    /// <summary>
    /// Defines a component that has accessibility properties.
    /// </summary>
    [H5.Name("tss.IACC")]
    public interface IAccessibility
    {
        /// <summary>Sets the ARIA role for the component.</summary>
        string AriaRole { set; }

        /// <summary>Sets the ARIA label for the component.</summary>
        string AriaLabel { set; }

        /// <summary>Sets the ID of the element that labels this component.</summary>
        string AriaLabelledBy { set; }

        /// <summary>Sets the ID of the element that describes this component.</summary>
        string AriaDescribedBy { set; }
    }
}
