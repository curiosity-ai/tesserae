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
            _toast = new Toast().Class("tss-saving-toast");
        }

        public SavingToast Saving(string message = null, string title = "Saving...")
        {
            _toast.Duration(TimeSpan.FromDays(1)); // Indefinite

            var t = HStack().H(20).MinWidth(200.px()).WS().Children(
                Icon(UIcons.Disk).Foreground(Theme.Primary.Background),
                TextBlock(title).SemiBold().SmallPlus().Primary()
            ).AlignItems(ItemAlign.Center).Gap(8.px()).PL(8);

            var msg = VStack().WS().PL(8).Children(
                TextBlock(message ?? _initialMessage).WS().BreakSpaces().PB(16).PT(8),
                ProgressIndicator().WS().Indeterminated());

            _toast.Information(t, msg);
            return this;
        }

        public SavingToast Saved(string message = null, string title = "Saved")
        {
            _toast.Duration(MinimumDisplayTime);

            var t = HStack().H(20).MinWidth(200.px()).WS().Children(
                Icon(UIcons.Check).Foreground(Theme.Success.Background),
                TextBlock(title).SemiBold().SmallPlus().Success()
            ).AlignItems(ItemAlign.Center).Gap(8.px()).PL(8);

            _toast.Success(t, TextBlock(message).PL(8).PT(8).WS().BreakSpaces());
            return this;
        }

        public SavingToast Error(string message = null, string title = "Error", bool untilDismissed = false)
        {
            _toast.Duration(untilDismissed ? TimeSpan.FromDays(1) : MinimumDisplayTime);
            
            var btnHide = Button().SetIcon(UIcons.Cross).OnClick(() => _toast.Hide());

            var t = HStack().H(20).MinWidth(200.px()).WS().Children(
                Icon(UIcons.OctagonXmark).Foreground(Theme.Danger.Background),
                TextBlock(title).Grow().SemiBold().SmallPlus().Danger(),
                If(untilDismissed, btnHide)
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
