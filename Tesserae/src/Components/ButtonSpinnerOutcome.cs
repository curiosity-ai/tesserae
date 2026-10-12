namespace Tesserae
{
    /// <summary>
    /// How a button's spinner ends (<see cref="Button.UndoSpinner(ButtonSpinnerOutcome)"/>).
    /// </summary>
    public enum ButtonSpinnerOutcome
    {
        /// <summary>The spinner fades back to the button's content.</summary>
        None,
        /// <summary>The ring closes around a check, then the button's content returns.</summary>
        Success,
        /// <summary>The ring closes around an x, then the button's content returns.</summary>
        Error
    }
}
