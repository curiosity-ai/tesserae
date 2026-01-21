using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Autocomplete")]
    public class Autocomplete : ComponentBase<Autocomplete, HTMLDivElement>
    {
        private readonly TextBox _textBox;
        private readonly HTMLDivElement _suggestionsContainer;
        private Func<string, IEnumerable<string>> _getSuggestions;

        public event Action<string> OnItemSelected;

        public Autocomplete()
        {
            _textBox = TextBox().OnInput((s, e) => ShowSuggestions());
            _suggestionsContainer = Div(_("tss-autocomplete-suggestions"));
            _suggestionsContainer.style.display = "none";
            InnerElement = Div(_("tss-autocomplete"), _textBox.Render(), _suggestionsContainer);

            _textBox.OnBlur((s, e) => window.setTimeout((_) => _suggestionsContainer.style.display = "none", 200));
        }

        public Autocomplete Suggestions(Func<string, IEnumerable<string>> getSuggestions)
        {
            _getSuggestions = getSuggestions;
            return this;
        }

        private void ShowSuggestions()
        {
            var text = _textBox.Text;
            if (string.IsNullOrEmpty(text) || _getSuggestions == null)
            {
                _suggestionsContainer.style.display = "none";
                return;
            }

            var suggestions = _getSuggestions(text).ToList();
            if (suggestions.Count == 0)
            {
                _suggestionsContainer.style.display = "none";
                return;
            }

            ClearChildren(_suggestionsContainer);
            foreach (var suggestion in suggestions)
            {
                var suggestionEl = Div(_("tss-autocomplete-suggestion", text: suggestion));
                suggestionEl.onclick = (e) =>
                {
                    _textBox.Text = suggestion;
                    _suggestionsContainer.style.display = "none";
                    OnItemSelected?.Invoke(suggestion);
                };
                _suggestionsContainer.appendChild(suggestionEl);
            }
            _suggestionsContainer.style.display = "block";
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
