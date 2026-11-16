using System;
using TNT;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The "AI" badge inside a <see cref="TextBox"/> or <see cref="TextArea"/> (<c>AI(badge: true)</c>), shared by both. It remembers the
    /// text the model wrote - the baseline - and shows <c>AI</c> while the field still holds it and <c>Edited</c> once it does not.
    /// The baseline is whatever the field held when the variant was switched on, and again after every programmatic text change
    /// (<c>Text = …</c> / <c>SetText</c>): the application putting a value in is the model handing over a new one. A change the user
    /// makes - typing, or dictation - never moves it, so putting the text back as it was returns the badge to <c>AI</c>.
    /// </summary>
    internal sealed class AIFieldBadge
    {
        private readonly HTMLElement  _host;
        private readonly Func<string> _getText;
        private readonly HTMLElement  _holder;
        private readonly Badge        _badge;

        private string _baseline;
        private bool   _edited;
        private bool   _suspended;
        private string _editedText;

        /// <param name="host">The field's root element, which carries the classes and holds the badge.</param>
        /// <param name="getText">Reads the field's current text.</param>
        public AIFieldBadge(HTMLElement host, Func<string> getText)
        {
            _host    = host;
            _getText = getText;
            _badge   = AIBadge();
            _holder  = Div(Att("tss-textbox-ai-badge"), _badge.Render());
        }

        /// <summary>Shows the badge, taking the field's current text as the baseline.</summary>
        public void Enable(string editedText, bool strongEffect)
        {
            _editedText = string.IsNullOrEmpty(editedText) ? "Edited".t() : editedText;

            _badge.AI(strongEffect);
            _host.classList.add("tss-textbox-ai-badged");

            if (_holder.parentElement != _host) _host.appendChild(_holder);

            _baseline = _getText();
            Apply(false);
        }

        /// <summary>Takes the badge away again, leaving the field's own AI look to its owner.</summary>
        public void Disable()
        {
            _holder.remove();
            _host.classList.remove("tss-textbox-ai-badged");
            _host.classList.remove("tss-ai-edited");
        }

        /// <summary>The text was set by the application: this is now what the model wrote.</summary>
        public void Rebase()
        {
            if (_suspended) return;

            _baseline = _getText();
            Apply(false);
        }

        /// <summary>The user may have changed the text: compare it with the baseline.</summary>
        public void Refresh() => Apply(_getText() != _baseline);

        /// <summary>Runs <paramref name="change"/>, a change that is the user's even though it goes through the text setter (dictation).</summary>
        public void AsUserEdit(Action change)
        {
            var before = _suspended;

            _suspended = true;

            try
            {
                change();
            }
            finally
            {
                _suspended = before;
            }

            Refresh();
        }

        private void Apply(bool edited)
        {
            _edited = edited;

            _badge.SetText(edited ? _editedText : "AI");
            _badge.Outline(edited);

            _host.UpdateClassIf(edited, "tss-ai-edited");
            _holder.setAttribute("title", edited ? "AI-generated, then edited".t() : "AI-generated".t());

            // An empty field holds nothing a model wrote (or nothing is left of it), so there is nothing to badge.
            _holder.style.display = string.IsNullOrEmpty(_getText()) ? "none" : string.Empty;
        }

        /// <summary>Whether the text now differs from what the model wrote.</summary>
        public bool IsEdited => _edited;
    }
}
