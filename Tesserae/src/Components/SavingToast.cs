using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;
using System;
using System.Threading.Tasks;

namespace Tesserae
{
    /// <summary>
    /// A toast variant that shows a "saving…" indicator while a long operation is running, swapping to success /
    /// error feedback when it completes.
    /// </summary>
    [Name("tss.SavingToast")]
    public class SavingToast
    {
        private Toast _toast;
        private string _initialMessage;
        /// <summary>
        /// Gets or sets the minimum display time.
        /// </summary>
        public TimeSpan MinimumDisplayTime { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public SavingToast(string initialMessage)
        {
            _initialMessage = initialMessage ?? "Saving...";
            _toast = new Toast().Class("tss-saving-toast");
        }

        /// <summary>
        /// Configures the component to saving.
        /// </summary>
        public SavingToast Saving(string message = null, string title = "Saving...")
        {
            _toast.Duration(TimeSpan.FromDays(1)); // Indefinite

            _toast.Show(new Banner(title)
               .Primary()
               .SetIcon(UIcons.Disk)
               .SetText(VStack().WS().Gap(8.px()).Children(
                    TextBlock(message ?? _initialMessage).WS().BreakSpaces(),
                    ProgressIndicator().WS().Indeterminated())));

            return this;
        }

        /// <summary>
        /// Configures the component to saved.
        /// </summary>
        public SavingToast Saved(string message = null, string title = "Saved")
        {
            _toast.Duration(MinimumDisplayTime);

            _toast.Show(new Banner(title, message).Success().SetIcon(UIcons.Check));

            return this;
        }

        /// <summary>
        /// Gets or sets the validation error message displayed beneath the component.
        /// </summary>
        public SavingToast Error(string message = null, string title = "Error", bool untilDismissed = false)
        {
            _toast.Duration(untilDismissed ? TimeSpan.FromDays(1) : MinimumDisplayTime);

            _toast.Show(new Banner(title, message).Danger().SetIcon(UIcons.OctagonXmark));

            return this;
        }
    }

    /// <summary>
    /// Extension helpers for awaiting a <see cref="Task"/> while showing a <see cref="SavingToast"/>.
    /// </summary>
    public static class SavingToastHelper
    {
        /// <summary>
        /// Awaits the given task while displaying a <see cref="SavingToast"/>: the toast shows a "saving"
        /// indicator while the task is in flight and swaps to a "saved" or "error" message on completion.
        /// </summary>
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

        /// <summary>
        /// Returns the component configured with the given saving toast.
        /// </summary>
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
