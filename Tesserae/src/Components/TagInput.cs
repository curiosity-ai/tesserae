using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.TagInput")]
    public class TagInput : ComponentBase<TagInput, HTMLDivElement>
    {
        private readonly Stack _mainStack;
        private readonly Stack _tagsStack;
        private readonly HTMLInputElement _input;
        private readonly List<string> _tags = new List<string>();

        public event ComponentEventHandler<TagInput, string[]> onChange;

        public TagInput()
        {
            _tagsStack = HStack().Wrap().Class("tss-taginput-tags");
            _tagsStack.Render().style.alignItems = "center";
            _tagsStack.Render().style.gap = "4px";
            _tagsStack.Render().style.padding = "4px";

            _input = new HTMLInputElement();
            _input.type = "text";
            _input.className = "tss-taginput-input";
            _input.style.border = "none";
            _input.style.outline = "none";
            _input.style.minWidth = "60px";
            _input.style.flexGrow = "1";
            _input.style.backgroundColor = "transparent";

            _input.addEventListener("keydown", (e) =>
            {
                var ke = e as KeyboardEvent;
                if (ke.key == "Enter" && !string.IsNullOrWhiteSpace(_input.value))
                {
                    AddTag(_input.value.Trim());
                    _input.value = "";
                    ke.preventDefault();
                }
                else if (ke.key == "Backspace" && string.IsNullOrEmpty(_input.value) && _tags.Count > 0)
                {
                    RemoveTag(_tags[_tags.Count - 1]);
                    ke.preventDefault();
                }
            });

            _tagsStack.Children(Raw(_input));

            _mainStack = HStack().Class("tss-taginput");
            _mainStack.Render().style.border = "1px solid var(--tss-default-border-color)";
            _mainStack.Render().style.borderRadius = "4px";
            _mainStack.Render().style.backgroundColor = "var(--tss-default-background-color)";
            _mainStack.Render().style.minHeight = "32px";
            _mainStack.Render().style.cursor = "text";

            _mainStack.Children(_tagsStack);

            _mainStack.Render().onclick = (e) =>
            {
                _input.focus();
            };

            InnerElement = Div(_("tss-taginput-wrapper"), _mainStack.Render());
        }

        public TagInput AddTag(string tag)
        {
            if (!_tags.Contains(tag))
            {
                _tags.Add(tag);
                RenderTags();
                onChange?.Invoke(this, _tags.ToArray());
            }
            return this;
        }

        public TagInput RemoveTag(string tag)
        {
            if (_tags.Contains(tag))
            {
                _tags.Remove(tag);
                RenderTags();
                onChange?.Invoke(this, _tags.ToArray());
            }
            return this;
        }

        public TagInput SetTags(IEnumerable<string> tags)
        {
            _tags.Clear();
            _tags.AddRange(tags);
            RenderTags();
            onChange?.Invoke(this, _tags.ToArray());
            return this;
        }

        private void RenderTags()
        {
            _tagsStack.Clear();
            foreach (var tag in _tags)
            {
                var badge = Badge(tag);
                badge.Render().style.cursor = "pointer";
                badge.Render().title = "Click to remove";

                string currentTag = tag; // Capture for lambda
                badge.OnClick((s, e) =>
                {
                    RemoveTag(currentTag);
                    e.stopPropagation();
                });

                _tagsStack.Children(badge);
            }
            _tagsStack.Children(Raw(_input));
        }

        public TagInput OnChange(ComponentEventHandler<TagInput, string[]> handler)
        {
            onChange += handler;
            return this;
        }

        public TagInput Placeholder(string placeholder)
        {
            _input.placeholder = placeholder;
            return this;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}