using System;
using System.Linq;
using System.Threading.Tasks;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A single-line text input component.
    /// </summary>
    [Transpose.Name("tss.TextBox")]
    public class TextBox : Input<TextBox>, ITextFormating, IHasBackgroundColor, IHasForegroundColor, IRoundedStyle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextBox"/> class.
        /// </summary>
        /// <param name="text">The initial text.</param>
        public TextBox(string text = string.Empty) : base("text", text)
        {
            InnerElement.classList.add("tss-fontsize-small");
            InnerElement.classList.add("tss-fontweight-regular");
        }

        /// <summary>Gets or sets the placeholder text.</summary>
        public string Placeholder
        {
            get => InnerElement.placeholder;
            set => InnerElement.placeholder = value;
        }

        /// <summary>Gets or sets whether the text box is read-only.</summary>
        public bool IsReadOnly
        {
            get => InnerElement.hasAttribute("readonly");
            set
            {
                if (value)
                {
                    InnerElement.setAttribute("readonly", string.Empty);
                }
                else
                {
                    InnerElement.removeAttribute("readonly");
                }
            }
        }

        /// <summary>Gets or sets the maximum length of the text.</summary>
        public int MaxLength
        {
            get => InnerElement.maxLength;
            set => InnerElement.maxLength = value;
        }

        /// <summary>Gets or sets whether the text box is in password mode.</summary>
        public bool IsPassword
        {
            get => InnerElement.type == "password";
            set
            {
                if (value)
                {
                    InnerElement.type = "password";
                }
                else
                {
                    InnerElement.type = string.Empty;
                }
            }
        }

        /// <summary>Gets or sets the text size.</summary>
        public virtual TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the text weight.</summary>
        public virtual TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the text alignment.</summary>
        public TextAlign TextAlign
        {
            get
            {
                return ITextFormatingExtensions.FromClassList(InnerElement, TextAlign.Left);
            }
            set
            {
                InnerElement.classList.remove(TextAlign.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the background color.</summary>
        public string Background
        {
            get => InnerElement.style.background;
            set => InnerElement.style.background = value;
        }

        /// <summary>Gets or sets the foreground color.</summary>
        public string Foreground
        {
            get => InnerElement.style.color;
            set => InnerElement.style.color = value;
        }

        /// <summary>
        /// Sets the placeholder text.
        /// </summary>
        /// <param name="placeholder">The placeholder text.</param>
        /// <returns>The current instance.</returns>
        public TextBox SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;
            return this;
        }

        /// <summary>Sets the text box as read-only.</summary>
        public TextBox ReadOnly()
        {
            IsReadOnly = true;
            return this;
        }

        /// <summary>Sets the text box to password mode.</summary>
        public TextBox Password()
        {
            IsPassword = true;
            return this;
        }

        /// <summary>Sets the text box input type to <c>email</c>.</summary>
        public TextBox Email()
        {
            InnerElement.type = "email";
            return this;
        }

        /// <summary>
        /// Sets the <c>name</c> attribute on the underlying input. Password managers and
        /// browser autofill heuristics use this to identify the field's role (e.g. <c>username</c>,
        /// <c>password</c>, <c>email</c>) and to scope saved credentials to the current origin.
        /// </summary>
        public TextBox Name(string name)
        {
            InnerElement.name = name ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the <c>autocomplete</c> attribute on the underlying input. Use values from the
        /// WHATWG autofill spec (<c>username</c>, <c>email</c>, <c>current-password</c>,
        /// <c>new-password</c>, <c>one-time-code</c>, <c>off</c>, ...). Password managers rely on
        /// this hint to distinguish sign-in flows from password-creation flows.
        /// </summary>
        public TextBox AutoComplete(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                InnerElement.removeAttribute("autocomplete");
            }
            else
            {
                InnerElement.setAttribute("autocomplete", value);
            }
            return this;
        }

        /// <summary>Removes the border from the text box, in every state: no hover edge and no focus ring, for an input set inside a box that draws its own.</summary>
        public TextBox NoBorder()
        {
            InnerElement.classList.add("tss-textbox-noborder");
            return this;
        }

        /// <summary>Removes the default minimum width.</summary>
        public TextBox NoMinWidth()
        {
            InnerElement.size = 1; //Remove the input min-width derived from the size attribute, see https://stackoverflow.com/questions/29470676/why-doesnt-the-input-element-respect-min-width
            return this;
        }

        /// <summary>Unlocks the height restriction.</summary>
        public TextBox UnlockHeight()
        {
            InnerElement.classList.add("tss-textbox-h100");
            return this;
        }

        /// <summary>
        /// Marks the text box as holding what a model wrote - a generated title, a suggested value: the field takes the AI
        /// tint and an accent edge, and a spinner shown by <see cref="SpinWhileAsync(Func{Task})"/> takes the AI arc. Turn it
        /// off again (<c>AI(false)</c>) when the value stops being the model's, e.g. once the user types over it.
        /// </summary>
        /// <param name="value">Whether the AI look is on.</param>
        /// <param name="strongEffect">Asks for the louder form of the AI styling (<c>tss-ai-strong</c>); see the ai-variants reference.</param>
        public TextBox AI(bool value = true, bool strongEffect = false)
        {
            InnerElement.UpdateClassIf(value, "tss-ai");
            InnerElement.UpdateClassIf(value && strongEffect, "tss-ai-strong");
            return this;
        }

        /// <summary>Whether <see cref="AI"/> is on.</summary>
        public bool IsAI => InnerElement.classList.contains("tss-ai");

        private DictationButton _dictation;

        /// <summary>
        /// Adds a microphone button at the end of the field, as an <see cref="OmniBox"/> has with
        /// <see cref="OmniBox.Config.DictationButton"/>: pressing it dictates after the text already in the field, pressing it
        /// again stops, and what was heard stays. The words come from <paramref name="provider"/>, else
        /// <see cref="Tesserae.Dictation.Provider"/>, else the browser's speech recognition; the button is not shown when none is available.
        /// </summary>
        /// <param name="provider">The source of this field's dictation, instead of the application-wide one.</param>
        public TextBox WithDictation(DictationProvider provider = null)
        {
            if (_dictation is null) _dictation = new DictationButton(Render(), InnerElement, () => Text, text => Text = text, () => MaxLength, provider);
            return this;
        }

        /// <summary>True while the microphone button is listening.</summary>
        public bool IsDictating => _dictation?.IsDictating ?? false;

        /// <summary>Starts dictating, as pressing the microphone button does. Does nothing without <see cref="WithDictation"/>, when already dictating, or when nothing can supply words.</summary>
        public TextBox StartDictation()
        {
            _dictation?.Start();
            return this;
        }

        /// <summary>Stops dictating; what was heard stays in the field.</summary>
        public TextBox StopDictation()
        {
            _dictation?.Stop();
            return this;
        }

        private int         _spinning;
        private HTMLElement _spinnerHost;

        /// <summary>
        /// Shows a spinner inside the text box, at its end, while <paramref name="action"/> runs - the text box's
        /// equivalent of <see cref="Button.OnClickSpinWhile(Func{Task}, string, Action{Button, Exception})"/>. The spinner
        /// goes when the action ends, whether it finished or threw (the exception is not swallowed). Overlapping calls
        /// share one spinner, which stays until the last of them ends. The field stays editable meanwhile.
        /// </summary>
        public async Task SpinWhileAsync(Func<Task> action)
        {
            StartSpinner();

            try
            {
                await action();
            }
            finally
            {
                StopSpinner();
            }
        }

        /// <summary>
        /// <see cref="SpinWhileAsync(Func{Task})"/> for an action with a result, which is handed back.
        /// </summary>
        public async Task<T> SpinWhileAsync<T>(Func<Task<T>> action)
        {
            StartSpinner();

            try
            {
                return await action();
            }
            finally
            {
                StopSpinner();
            }
        }

        /// <summary>Whether a <see cref="SpinWhileAsync(Func{Task})"/> action is running.</summary>
        public bool IsSpinning => _spinning > 0;

        private void StartSpinner()
        {
            _spinning++;

            if (_spinning > 1) return;

            //Built per spin rather than kept, so it takes the AI arc only while the field is AI.
            var spinner = Spinner().XSmall();

            if (IsAI) spinner.AI();

            _spinnerHost = Div(Att("tss-textbox-spinner"), spinner.Render());

            var container = Render();

            container.appendChild(_spinnerHost);
            container.classList.add("tss-textbox-spinning");
        }

        private void StopSpinner()
        {
            if (_spinning == 0) return;

            _spinning--;

            if (_spinning > 0) return;

            _spinnerHost?.remove();
            _spinnerHost = null;

            Render().classList.remove("tss-textbox-spinning");
        }
    }
}