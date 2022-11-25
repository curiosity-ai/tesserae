using H5;
using static H5.Core.dom;

namespace Tesserae
{
    [Name("tss.Layers")]
    public static class Layers
    {
        private const int BaseZIndex = 1000;
        public static string PushLayer(HTMLElement element)
        {
            return (CurrentZIndex() + 10).ToString();
        }

        private static int CurrentZIndex()
        {
            int maxIndex = BaseZIndex;

            foreach (HTMLElement htmlElement in document.querySelectorAll(".tss-layer"))
            {
                if (int.TryParse(htmlElement.style.zIndex, out var zIndex) && zIndex > maxIndex) maxIndex = zIndex;
            }
            return maxIndex;
        }

        public static string AboveCurrent()
        {
            return (CurrentZIndex() + 5).ToString();
        }
    }
}