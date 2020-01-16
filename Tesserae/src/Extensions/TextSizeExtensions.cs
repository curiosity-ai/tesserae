namespace Tesserae.Components
{
    public static class TextSizeExtensions
    {
        public static T Tiny<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Size = TextSize.Tiny;
            return textBlock;
        }

        public static T XSmall<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Size = TextSize.XSmall;
            return textBlock;
        }
        public static T Small<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Size = TextSize.Small;
            return textBlock;
        }

        public static T SmallPlus<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Size = TextSize.SmallPlus;
            return textBlock;
        }

        public static T Medium<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Size = TextSize.Medium;
            return textBlock;
        }
        public static T MediumPlus<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Size = TextSize.MediumPlus;
            return textBlock;
        }

        public static T Large<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Size = TextSize.Large;
            return textBlock;
        }
        public static T XLarge<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Size = TextSize.XLarge;
            return textBlock;
        }

        public static T XXLarge<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Size = TextSize.XXLarge;
            return textBlock;
        }

        public static T Mega<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Size = TextSize.Mega;
            return textBlock;
        }

        public static T Regular<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Weight = TextWeight.Regular;
            return textBlock;
        }

        public static T SemiBold<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Weight = TextWeight.SemiBold;
            return textBlock;
        }

        public static T Bold<T>(this T textBlock) where T : IHasTextSize
        {
            textBlock.Weight = TextWeight.Bold;
            return textBlock;
        }
    }
}
