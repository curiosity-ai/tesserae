namespace Tesserae
{
    /// <summary>
    /// A text field that dictation can be started into: the <see cref="TextBox"/>, the <see cref="TextArea"/> and the
    /// <see cref="OmniBox"/> are, wherever they are on the page. Whatever transcribes speech for the application - a
    /// keyboard shortcut, a push-to-talk button outside the field, a voice command - does not need to hold on to the
    /// fields: it asks <see cref="DictationTargets.Current"/> for the one in use and starts dictating into it.
    /// </summary>
    /// <remarks>
    /// The queries are on <see cref="DictationTargets"/> rather than static members of this interface, which the
    /// netstandard2.0 target does not allow (CS8701). Every rendered target carries itself in a hidden field of its root
    /// element, so a lookup needs nothing from the application's own code. A component of the application that
    /// implements this interface joins the toolkit's by calling <see cref="DictationTargets.Register"/> once with its
    /// root element.
    /// </remarks>
    [Transpose.Name("tss.IDictationTarget")]
    public interface IDictationTarget
    {
        /// <summary>The text in the field. For an <see cref="OmniBox"/>, the text of the input that is showing.</summary>
        string Text { get; set; }

        /// <summary>True when the field takes text: it is enabled and not read-only.</summary>
        bool IsEditable { get; }

        /// <summary>True when something can supply words for the field (<see cref="Dictation"/>); an <see cref="OmniBox"/> also needs <see cref="OmniBox.Config.DictationButton"/>.</summary>
        bool CanDictate { get; }

        /// <summary>True while dictation is running into the field.</summary>
        bool IsDictating { get; }

        /// <summary>
        /// Starts dictating after the text already in the field, as pressing its microphone button does, whether or not the
        /// field shows one. Does nothing when already dictating or when <see cref="CanDictate"/> is false.
        /// </summary>
        void BeginDictation();

        /// <summary>Stops dictating; what was heard stays in the field.</summary>
        void EndDictation();
    }
}
