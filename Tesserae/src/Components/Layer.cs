namespace Tesserae
{
    /// <summary>
    /// A Layer is a technical component that does not have specific Design guidance.
    /// 
    /// Layers are used to render content outside of a DOM tree, at the end of the document. This allows content to escape traditional boundaries caused by "overflow: hidden" css rules and keeps it on the top without using z-index rules. This is useful for example in
    /// ContextualMenu and Tooltip scenarios, where the content should always overlay everything else.
    /// 
    /// This non-generic Layer class is appropriate when the core Layer functionality is all that you require and none of its behaviours need to be extended - should you need a Layer base class that CAN be derived from (such as the ContextMenu, for example), use the
    /// generic Layer class. The reason for the two classes is to avoid confusion as this can NOT be derived from and the generic version MUST be derived from. The generic version exists to maintain the type of component in chained calls made on the ComponentBase
    /// class that they both are derived from (when the OnClick method is called on a ContextMenu then you expect a ContextMenu to be returned and not simply a Layer instance).
    /// </summary>
    [H5.Name("tss.Layer")]
    public sealed class Layer : Layer<Layer>
    {
    }
}