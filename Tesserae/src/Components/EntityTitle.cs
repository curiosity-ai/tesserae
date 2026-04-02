using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.EntityTitle")]
    public class EntityTitle : ComponentBase<EntityTitle, HTMLDivElement>
    {
        private readonly Stack _mainStack;
        private readonly HTMLDivElement _iconContainer;
        private readonly TextBlock _title;
        private readonly TextBlock _subtitle;
        private readonly Stack _textStack;
        private readonly HTMLDivElement _tagsContainer;

        public EntityTitle(string title = null, string subtitle = null, IComponent icon = null)
        {
            _iconContainer = Div(_("tss-entitytitle-icon"));
            _title = TextBlock(title).MediumPlus().SemiBold();
            _subtitle = TextBlock(subtitle).Small().Foreground(Theme.Secondary.Foreground);
            _tagsContainer = Div(_("tss-entitytitle-tags", styles: s => { s.marginLeft = "16px"; s.display = "flex"; s.alignItems = "center"; }));

            _textStack = VStack().Class("tss-entitytitle-text").JustifyContent(ItemJustify.Center);
            if (!string.IsNullOrEmpty(title)) _textStack.Children(_title);
            if (!string.IsNullOrEmpty(subtitle)) _textStack.Children(_subtitle);

            _mainStack = HStack().AlignCenter().Class("tss-entitytitle");

            if (icon != null)
            {
                _iconContainer.appendChild(icon.Render());
                _iconContainer.style.marginRight = "16px";
                _mainStack.Children(Raw(_iconContainer));
            }

            _mainStack.Children(_textStack);

            InnerElement = Div(_("tss-entitytitle-wrapper"), _mainStack.Render());
        }

        public EntityTitle Title(string title)
        {
            _title.Text = title;
            if (string.IsNullOrEmpty(title))
            {
                _textStack.Remove(_title);
            }
            else
            {
                _textStack.Remove(_title);
                _textStack.InsertBefore(_title, _subtitle);
            }
            return this;
        }

        public EntityTitle Subtitle(string subtitle)
        {
            _subtitle.Text = subtitle;
            if (string.IsNullOrEmpty(subtitle))
            {
                _textStack.Remove(_subtitle);
            }
            else
            {
                _textStack.Remove(_subtitle);
                _textStack.Children(_subtitle);
            }
            return this;
        }

        public EntityTitle Icon(IComponent icon)
        {
            _iconContainer.innerHTML = "";
            if (icon != null)
            {
                _iconContainer.appendChild(icon.Render());
                _iconContainer.style.marginRight = "16px";
                _mainStack.Remove(Raw(_iconContainer));
                _mainStack.InsertBefore(Raw(_iconContainer), _textStack);
            }
            else
            {
                _mainStack.Remove(Raw(_iconContainer));
            }
            return this;
        }

        public EntityTitle Icon(UIcons icon, TextSize size = TextSize.Large, string color = null)
        {
            var i = UI.Icon(icon, size: size);
            if (color != null) i.Foreground(color);
            return Icon(i);
        }

        public EntityTitle Tags(params IComponent[] tags)
        {
            _tagsContainer.innerHTML = "";
            if (tags != null && tags.Length > 0)
            {
                foreach (var tag in tags)
                {
                    var tagContainer = Div(_(styles: s => s.marginRight = "8px"));
                    tagContainer.appendChild(tag.Render());
                    _tagsContainer.appendChild(tagContainer);
                }
                _mainStack.Remove(Raw(_tagsContainer));
                _mainStack.Children(Raw(_tagsContainer));
            }
            else
            {
                _mainStack.Remove(Raw(_tagsContainer));
            }
            return this;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}