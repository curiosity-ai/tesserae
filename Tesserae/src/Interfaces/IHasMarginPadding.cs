﻿namespace Tesserae
{
    [H5.Name("tss.IHMP")]
    public interface IHasMarginPadding //TODO: Change interface to match methods for padding/margin on StackExtensions
    {
        string Margin  { get; set; }
        string Padding { get; set; }
    }
}