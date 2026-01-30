using H5;
using static H5.Core.dom;
using static Tesserae.UI;
using System;

namespace Tesserae
{
    [Name("tss.SavingToast")]
    public class SavingToast : IComponent
    {
        private Toast _toast;
        private string _initialMessage;
        public TimeSpan MinimumDisplayTime { get; set; } = TimeSpan.FromSeconds(5);

        public SavingToast(string initialMessage)
        {
            _initialMessage = initialMessage;
            _toast = new Toast();
        }

        public void Saving(string message = null)
        {
            _toast.Duration(TimeSpan.FromDays(1)); // Indefinite

            var title = HStack().Children(
                Icon(UIcons.Refresh).Foreground(UI.Theme.Primary.Background),
                TextBlock("Saving...").SemiBold().Primary()
            ).AlignItems(ItemAlign.Center).Style(s => s.gap = "8px");

            _toast.Information(title, TextBlock(message ?? _initialMessage));
        }

        public void Saved(string message = null)
        {
            _toast.Duration(MinimumDisplayTime);

            var title = HStack().Children(
                Icon(UIcons.Check).Foreground(UI.Theme.Success.Background),
                TextBlock("Saved").SemiBold().Success()
            ).AlignItems(ItemAlign.Center).Style(s => s.gap = "8px");

            _toast.Success(title, TextBlock(message));
        }

        public void Error(string message = null)
        {
            _toast.Duration(MinimumDisplayTime);

            var title = HStack().Children(
                Icon(UIcons.Cross).Foreground(UI.Theme.Danger.Background),
                TextBlock("Error").SemiBold().Danger()
            ).AlignItems(ItemAlign.Center).Style(s => s.gap = "8px");

            _toast.Error(title, TextBlock(message));
        }

        public HTMLElement Render() => _toast.Render();
    }
}
