using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.ResourceCard")]
    public sealed class ResourceCard : ComponentBase<ResourceCard, HTMLElement>, IHasBackgroundColor, IRoundedStyle
    {
        private readonly Card _card;

        private readonly HTMLElement _iconContainer;
        private readonly HTMLElement _titleContainer;
        private readonly HTMLElement _subtitleContainer;
        private readonly HTMLElement _tagsContainer;
        private readonly HTMLElement _descriptionContainer;
        private readonly HTMLElement _dateContainer;

        private readonly Stack _footerContainer;
        private readonly HTMLElement _footerLeft;
        private readonly HTMLElement _footerRight;

        public ResourceCard()
        {
            _iconContainer = Div(_("tss-default-component-no-margin"));
            _titleContainer = Div(_("tss-default-component-no-margin"));
            var titleRow = HStack().AlignItems(ItemAlign.Center).Children(
                Raw(_iconContainer),
                Raw(_titleContainer).WS().PaddingLeft(8.px())
            );

            _subtitleContainer = Div(_("tss-default-component-no-margin"));
            _tagsContainer = Div(_("tss-default-component-no-margin"));

            var subtitleRow = HStack().AlignItems(ItemAlign.Center).PaddingTop(8.px()).Children(
                Raw(_subtitleContainer),
                Raw(_tagsContainer).WS().PaddingLeft(8.px())
            );

            var headerContainer = VStack().Children(titleRow, subtitleRow);

            _descriptionContainer = Div(_("tss-default-component-no-margin"));
            _dateContainer = Div(_("tss-default-component-no-margin"));

            var bodyContainer = VStack().PaddingTop(16.px()).PaddingBottom(16.px()).Children(
                Raw(_descriptionContainer),
                Raw(_dateContainer).WS().PaddingTop(16.px())
            );

            _footerLeft = Div(_("tss-default-component-no-margin"));
            _footerRight = Div(_("tss-default-component-no-margin"));

            _footerContainer = HStack().AlignItems(ItemAlign.Center).JustifyContent(ItemJustify.Between)
                .PaddingTop(12.px())
                .Children(Raw(_footerLeft), Raw(_footerRight));

            _footerContainer.Render().style.borderTop = "1px solid var(--tss-default-border-color, var(--tss-card-border-color, #e1dfdd))";

            var mainStack = VStack().Padding(16.px()).Children(headerContainer, bodyContainer, _footerContainer);

            _card = Card(mainStack).NoPadding();

            InnerElement = _card.Render();

            // Hide empty containers by default
            _iconContainer.style.display = "none";
            _titleContainer.style.display = "none";
            _subtitleContainer.style.display = "none";
            _tagsContainer.style.display = "none";
            _descriptionContainer.style.display = "none";
            _dateContainer.style.display = "none";
            _footerContainer.Collapse();

            AttachClick();
            AttachContextMenu();
        }

        public ResourceCard SetIcon(IComponent icon)
        {
            ClearChildren(_iconContainer);
            if (icon != null)
            {
                _iconContainer.appendChild(icon.Render());
                _iconContainer.style.display = "";
            }
            else
            {
                _iconContainer.style.display = "none";
            }
            return this;
        }

        public ResourceCard SetTitle(IComponent title)
        {
            ClearChildren(_titleContainer);
            if (title != null)
            {
                _titleContainer.appendChild(title.Render());
                _titleContainer.style.display = "";
            }
            else
            {
                _titleContainer.style.display = "none";
            }
            return this;
        }

        public ResourceCard SetTitle(string title) => SetTitle(TextBlock(title).SemiBold().MediumPlus());

        public ResourceCard SetSubtitle(IComponent subtitle)
        {
            ClearChildren(_subtitleContainer);
            if (subtitle != null)
            {
                _subtitleContainer.appendChild(subtitle.Render());
                _subtitleContainer.style.display = "";
            }
            else
            {
                _subtitleContainer.style.display = "none";
            }
            return this;
        }

        public ResourceCard SetSubtitle(string subtitle) => SetSubtitle(TextBlock(subtitle).SemiBold().Small());

        public ResourceCard SetTags(params IComponent[] tags)
        {
            ClearChildren(_tagsContainer);
            if (tags != null && tags.Length > 0)
            {
                var hstack = HStack().AlignItems(ItemAlign.Center).Gap(4.px());
                foreach (var tag in tags)
                {
                    hstack.Add(tag);
                }
                _tagsContainer.appendChild(hstack.Render());
                _tagsContainer.style.display = "";
            }
            else
            {
                _tagsContainer.style.display = "none";
            }
            return this;
        }

        public ResourceCard SetDescription(IComponent description)
        {
            ClearChildren(_descriptionContainer);
            if (description != null)
            {
                _descriptionContainer.appendChild(description.Render());
                _descriptionContainer.style.display = "";
            }
            else
            {
                _descriptionContainer.style.display = "none";
            }
            return this;
        }

        public ResourceCard SetDescription(string description) => SetDescription(TextBlock(description).Small().Foreground(Theme.Secondary.Foreground));

        public ResourceCard SetDate(IComponent date)
        {
            ClearChildren(_dateContainer);
            if (date != null)
            {
                _dateContainer.appendChild(date.Render());
                _dateContainer.style.display = "";
            }
            else
            {
                _dateContainer.style.display = "none";
            }
            return this;
        }

        public ResourceCard SetDate(string date) => SetDate(TextBlock(date).Small().Foreground(Theme.Secondary.Foreground));

        public ResourceCard SetFooter(IComponent footer)
        {
            ClearChildren(_footerLeft);
            if (footer != null)
            {
                _footerLeft.appendChild(footer.Render());
                _footerContainer.Show();
            }
            else if (_footerRight.childElementCount == 0)
            {
                _footerContainer.Collapse();
            }
            return this;
        }

        public ResourceCard SetFooterCommands(params IComponent[] commands)
        {
            ClearChildren(_footerRight);
            if (commands != null && commands.Length > 0)
            {
                var hstack = HStack().AlignItems(ItemAlign.Center).Gap(4.px());
                foreach (var command in commands)
                {
                    hstack.Add(command);
                }
                _footerRight.appendChild(hstack.Render());
                _footerContainer.Show();
            }
            else if (_footerLeft.childElementCount == 0)
            {
                _footerContainer.Collapse();
            }
            return this;
        }

        public string Background
        {
            get => _card.Background;
            set => _card.Background = value;
        }

        public ResourceCard BackgroundColor(string color)
        {
            _card.BackgroundColor(color);
            return this;
        }

        public ResourceCard Border(string color, UnitSize size = null)
        {
            _card.Border(color, size);
            return this;
        }

        public override HTMLElement Render()
        {
            return _card.Render();
        }
    }
}
