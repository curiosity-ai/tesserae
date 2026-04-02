using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.NonIdealState")]
    public class NonIdealState : ComponentBase<NonIdealState, HTMLDivElement>
    {
        private readonly Stack _stack;
        private readonly HTMLDivElement _iconContainer;
        private readonly TextBlock _title;
        private readonly TextBlock _description;
        private readonly HTMLDivElement _actionContainer;

        public NonIdealState(IComponent icon = null, string title = null, string description = null, IComponent action = null)
        {
            _iconContainer = Div(_("tss-nonidealstate-icon"));
            _title = TextBlock(title).MediumPlus().SemiBold().AlignCenter().MarginBottom(8.px());
            _title.Render().style.lineHeight = "1.5";
            _description = TextBlock(description).Medium().AlignCenter().Foreground(Theme.Secondary.Foreground);
            _description.Render().style.lineHeight = "1.5";
            _actionContainer = Div(_("tss-nonidealstate-action"));

            if (icon != null)
            {
                _iconContainer.appendChild(icon.Render());
            }

            if (action != null)
            {
                _actionContainer.appendChild(action.Render());
                _actionContainer.style.marginTop = "16px";
            }

            _stack = VStack()
                .AlignCenter()
                .JustifyContent(ItemJustify.Center)
                .Padding(32.px())
                .Class("tss-nonidealstate");

            if (icon != null) _stack.Children(Raw(_iconContainer));
            if (!string.IsNullOrEmpty(title)) _stack.Children(_title);
            if (!string.IsNullOrEmpty(description)) _stack.Children(_description);
            if (action != null) _stack.Children(Raw(_actionContainer));

            InnerElement = Div(_("tss-nonidealstate-wrapper", styles: s =>
            {
                s.display = "flex";
                s.flexDirection = "column";
                s.alignItems = "center";
                s.justifyContent = "center";
                s.height = "100%";
                s.width = "100%";
            }), _stack.Render());
        }

        public NonIdealState Icon(IComponent icon)
        {
            _iconContainer.innerHTML = "";
            if (icon != null)
            {
                _iconContainer.appendChild(icon.Render());
                _stack.Remove(Raw(_iconContainer));
                _stack.InsertBefore(Raw(_iconContainer), _title);
            }
            else
            {
                _stack.Remove(Raw(_iconContainer));
            }
            return this;
        }

        public NonIdealState Icon(UIcons icon, TextSize size = TextSize.Large, string color = null)
        {
            var i = UI.Icon(icon, size: size);
            if (color != null) i.Foreground(color);
            return Icon(i);
        }

        public NonIdealState Title(string title)
        {
            _title.Text = title;
            if (string.IsNullOrEmpty(title))
            {
                _stack.Remove(_title);
            }
            else
            {
                _stack.Remove(_title);
                _stack.InsertBefore(_title, _description);
            }
            return this;
        }

        public NonIdealState Description(string description)
        {
            _description.Text = description;
            if (string.IsNullOrEmpty(description))
            {
                _stack.Remove(_description);
            }
            else
            {
                _stack.Remove(_description);
                _stack.InsertBefore(_description, Raw(_actionContainer));
            }
            return this;
        }

        public NonIdealState Action(IComponent action)
        {
            _actionContainer.innerHTML = "";
            if (action != null)
            {
                _actionContainer.appendChild(action.Render());
                _actionContainer.style.marginTop = "16px";
                _stack.Remove(Raw(_actionContainer));
                _stack.Children(Raw(_actionContainer));
            }
            else
            {
                _stack.Remove(Raw(_actionContainer));
            }
            return this;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}