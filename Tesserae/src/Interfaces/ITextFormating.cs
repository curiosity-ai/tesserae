using System;
using System.Linq;
using static H5.Core.dom;
using H5;

namespace Tesserae
{
    public interface ITextFormating
    {
        TextSize Size { get; set; }
        TextWeight Weight { get; set; }
        TextAlign TextAlign { get; set; }
    }

    [Enum(Emit.Value)]
    public enum TextSize
    {
        [Name("tss-fontsize-tiny")]       Tiny,
        [Name("tss-fontsize-xsmall")]     XSmall,
        [Name("tss-fontsize-small")]      Small,
        [Name("tss-fontsize-smallplus")]  SmallPlus,
        [Name("tss-fontsize-medium")]     Medium,
        [Name("tss-fontsize-mediumplus")] MediumPlus,
        [Name("tss-fontsize-large")]      Large,
        [Name("tss-fontsize-xlarge")]     XLarge,
        [Name("tss-fontsize-xxlarge")]    XXLarge,
        [Name("tss-fontsize-mega")]       Mega
    }
    
    [Enum(Emit.Value)]
    public enum TextWeight
    {
        [Name("tss-fontweight-regular")]  Regular,
        [Name("tss-fontweight-semibold")] SemiBold,
        [Name("tss-fontweight-bold")]     Bold
    }

    [Enum(Emit.Value)]
    public enum TextAlign
    {
        [Name("tss-textalign-left")]   Left,
        [Name("tss-textalign-center")] Center,
        [Name("tss-textalign-right")]  Right
    }
}
