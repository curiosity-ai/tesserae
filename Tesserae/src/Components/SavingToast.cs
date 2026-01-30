using H5;
using static H5.Core.dom;
using static Tesserae.UI;
using System;
using System.Threading.Tasks;

namespace Tesserae
{
    [Name("tss.SavingToast")]
    public class SavingToast
    {
        private Toast _toast;
        private string _initialMessage;
        public TimeSpan MinimumDisplayTime { get; set; } = TimeSpan.FromSeconds(5);

        public SavingToast(string initialMessage)
        {
            _initialMessage = initialMessage ?? "Saving...";
            _toast = new Toast();
        }

        public SavingToast Saving(string message = null, string title = "Saving...")
        {
            _toast.Duration(TimeSpan.FromDays(1)); // Indefinite

            var t = HStack().MinWidth(200.px()).Children(
                Icon(UIcons.Disk).Foreground(Theme.Primary.Background),
                TextBlock(title).SemiBold().SmallPlus().Primary()
            ).AlignItems(ItemAlign.Center).Gap(8.px()).PL(8);

            var msg = VStack().MinWidth(200.px()).PL(8).Children(
                TextBlock(message ?? _initialMessage).WS().BreakSpaces().PB(16).PT(8),
                ProgressIndicator().WS().Indeterminated());

            _toast.Information(t, msg);
            return this;
        }

        public SavingToast Saved(string message = null, string title = "Saved")
        {
            _toast.Duration(MinimumDisplayTime);

            var t = HStack().MinWidth(200.px()).Children(
                Icon(UIcons.Check).Foreground(Theme.Success.Background),
                TextBlock(title).SemiBold().SmallPlus().Success()
            ).AlignItems(ItemAlign.Center).Gap(8.px()).PL(8);

            _toast.Success(t, TextBlock(message).PL(8).PT(8).WS().BreakSpaces());
            return this;
        }

        public SavingToast Error(string message = null, string title = "Error")
        {
            _toast.Duration(MinimumDisplayTime);

            var t = HStack().MinWidth(200.px()).Children(
                Icon(UIcons.Cross).Foreground(Theme.Danger.Background),
                TextBlock(title).SemiBold().SmallPlus().Danger()
            ).AlignItems(ItemAlign.Center).Gap(8.px()).PL(8);

            _toast.Error(t, TextBlock(message).PL(8).PT(8).WS().BreakSpaces());
            return this;
        }
    }

    public static class SavingToastHelper
    {
        public static async Task<T> WithSavingToast<T>(this Task<T> task, string savingMessage = "Saving...", string savedMessage = "Saved", string errorMessage = "Error")
        {
            var toast = new SavingToast(savingMessage);
            toast.Saving();
            try
            {
                var result = await task;
                toast.Saved(savedMessage);
                return result;
            }
            catch (Exception ex)
            {
                toast.Error($"{errorMessage}: {ex.Message}");
                throw;
            }
        }

        public static async Task WithSavingToast(this Task task, string savingMessage = "Saving...", string savedMessage = "Saved", string errorMessage = "Error")
        {
            var toast = new SavingToast(savingMessage);
            toast.Saving();
            try
            {
                await task;
                toast.Saved(savedMessage);
            }
            catch (Exception ex)
            {
                toast.Error($"{errorMessage}: {ex.Message}");
                throw;
            }
        }
    }
}
