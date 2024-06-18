using System;
using System.Linq;
using static H5.Core.dom;
using H5;

namespace Tesserae
{
    [H5.Name("tss.ITF")]
    public interface ITextFormating
    {
        TextSize   Size      { get; set; }
        TextWeight Weight    { get; set; }
        TextAlign  TextAlign { get; set; }
    }

    [Enum(Emit.StringName)] //Don't change the emit type without updating the FromClassList method
    [H5.Name("tss.TS")]
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

    [Enum(Emit.StringName)] //Don't change the emit type without updating the FromClassList method
    [H5.Name("tss.TC")]
    public enum TextColor
    {
        [Name("text-dark")]      Dark,
        [Name("text-warning")]   Warning,
        [Name("text-danger")]    Danger,
        [Name("text-muted")]     Muted,
        [Name("text-success")]   Success,
        [Name("text-primary")]   Primary,
        [Name("text-secondary")] Secondary,
    }


    [Enum(Emit.StringName)] //Don't change the emit type without updating the FromClassList method
    [H5.Name("tss.TW")]
    public enum TextWeight
    {
        [Name("tss-fontweight-regular")]  Regular,
        [Name("tss-fontweight-semibold")] SemiBold,
        [Name("tss-fontweight-bold")]     Bold
    }

    [Enum(Emit.StringName)] //Don't change the emit type without updating the FromClassList method
    [H5.Name("tss.TA")]
    public enum TextAlign
    {
        [Name("tss-textalign-left")]   Left,
        [Name("tss-textalign-center")] Center,
        [Name("tss-textalign-right")]  Right
    }
}