namespace Tesserae.Components
{
    public interface IHasTextSize
    {
        TextSize Size { get; set; }
        TextWeight Weight { get; set; }
    }

    public enum TextSize
    {
        Tiny,
        XSmall,
        Small,
        SmallPlus,
        Medium,
        MediumPlus,
        Large,
        XLarge,
        XXLarge,
        Mega
    }

    public enum TextWeight
    {
        Regular,
        SemiBold,
        Bold
    }
}
