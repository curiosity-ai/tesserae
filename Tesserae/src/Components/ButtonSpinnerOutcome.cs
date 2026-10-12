namespace Tesserae
{
    /// <summary>
    /// How a button's spinner ends (<see cref="Button.UndoSpinner(ButtonSpinnerOutcome)"/>). Only an icon-only button
    /// spinning in place shows an outcome; every other spinner just returns to the button.
    /// </summary>
    public enum ButtonSpinnerOutcome
    {
        /// <summary>The spinner fades back to the icon.</summary>
        None,
        /// <summary>The ring closes around a check, then the icon returns.</summary>
        Success,
        /// <summary>The ring closes around an x, then the icon returns.</summary>
        Error
    }
}
