namespace Tesserae
{
    /// <summary>What an <see cref="Input{TInput}.CheckAsync"/> check says about the text in the box.</summary>
    [Transpose.Name("tss.InputCheckState")]
    public enum InputCheckState
    {
        /// <summary>Nothing to say; no indicator is shown.</summary>
        None,
        /// <summary>The value is known to be good.</summary>
        Valid,
        /// <summary>The value may be wrong, but is allowed.</summary>
        Warning,
        /// <summary>The value is wrong.</summary>
        Invalid,
    }

    /// <summary>The answer of an <see cref="Input{TInput}.CheckAsync"/> callback.</summary>
    [Transpose.Name("tss.InputCheck")]
    public sealed class InputCheck
    {
        private InputCheck(InputCheckState state, string message, string tooltip)
        {
            State   = state;
            Message = message;
            Tooltip = tooltip;
        }

        /// <summary>The verdict.</summary>
        public InputCheckState State   { get; }
        /// <summary>The short label shown next to the indicator's icon and announced to screen readers, or <c>null</c> for the icon alone.</summary>
        public string          Message { get; }
        /// <summary>The explanation shown in a tooltip over the indicator, or <c>null</c> for none.</summary>
        public string          Tooltip { get; }

        /// <summary>No verdict: clears the indicator.</summary>
        public static InputCheck None() => new InputCheck(InputCheckState.None, null, null);
        /// <summary>The value is good. Labelled "Verified" unless <paramref name="message"/> says otherwise.</summary>
        public static InputCheck Valid(string message = "Verified", string tooltip = null) => new InputCheck(InputCheckState.Valid, message, tooltip);
        /// <summary>The value may be wrong, but is allowed.</summary>
        public static InputCheck Warning(string message, string tooltip = null) => new InputCheck(InputCheckState.Warning, message, tooltip);
        /// <summary>The value is wrong; the box's border turns red. Labelled "Not verified" unless <paramref name="message"/> says otherwise.</summary>
        public static InputCheck Invalid(string message = "Not verified", string tooltip = null) => new InputCheck(InputCheckState.Invalid, message, tooltip);
    }
}
