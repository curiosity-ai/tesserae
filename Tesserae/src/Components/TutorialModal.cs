using System;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class TutorialModal : IComponent, ISpecialCaseStyling
    {
        private readonly Modal     _modal;
        private readonly Stack     _footerCommands;
        private readonly Stack     _content;
        private readonly TextBlock _title;
        private readonly TextBlock _helpText;
        private readonly Raw       _illustration;
        private readonly Stack     _leftStack;
        private readonly Stack     _rightStack;

        public HTMLElement StylingContainer => _modal.StylingContainer;

        public bool PropagateToStackItemParent => _modal.PropagateToStackItemParent;

        public TutorialModal(string title, string helpText, string imageSrc = null)
        {
            _footerCommands = HStack().Padding(10.px()).AlignEnd().AlignItems(ItemAlign.End);
            _content = VStack().Padding("38px 32px 0px 32px");
            _title = TextBlock(title).Large().Bold().PaddingTop(10.px()).PaddingBottom(20.px());
            _helpText = TextBlock(helpText).Padding("20px 30px 0 0");
            _illustration = Raw().W(196).H(196);

            if (!string.IsNullOrWhiteSpace(imageSrc))
            {
                SetImageSrc(imageSrc, 16.px());
            }

            _leftStack = VStack().OverflowHidden()
               .RemovePropagation()
               .Width(300.px()).Padding("40px 32px 32px 32px").Background(Theme.Secondary.Background)
               .Children(VStack().S().JustifyContent(ItemJustify.Between).Children(_title, _helpText, Raw().H(48.px()).Grow(), _illustration));

            _rightStack = VStack().OverflowHidden().HS().W(10.px()).Grow().JustifyContent(ItemJustify.End)
               .Children(_content.H(10.px()).Grow(), _footerCommands);

            _modal = Modal()
               .NoContentPadding().LightDismiss().Dark()
               .Content(
                    HStack().S().Children(
                        _leftStack,
                        _rightStack
                    )
                );

            Height(500.px());
            Width(800.px());
        }

        public TutorialModal SetFooterCommands(params IComponent[] commands)
        {
            _footerCommands.Clear();
            _footerCommands.Children(commands);
            return this;
        }
        public TutorialModal SetContent(IComponent content)
        {
            _content.Clear();
            _content.Children(content);
            return this;
        }

        public TutorialModal Height(UnitSize height)
        {
            _leftStack.Height(height);
            _rightStack.Height(height);
            return this;
        }

        public TutorialModal Width(UnitSize width)
        {
            _modal.Width(width);
            return this;
        }

        public TutorialModal SetTitle(string title)
        {
            _title.Text = title;
            return this;
        }

        public TutorialModal SetHelpText(string helpText, bool treatAsHTML = false)
        {
            if (treatAsHTML)
            {
                _helpText.Text = null;
                _helpText.HTML = helpText;
            }
            else
            {
                _helpText.HTML = null;
                _helpText.Text = helpText;
            }
            return this;
        }

        public TutorialModal SetImageSrc(string imageSrc, UnitSize padding)
        {
            _illustration.Content(Image(imageSrc).Contain());
            _illustration.Padding(padding);
            return this;
        }

        public TutorialModal SetImage(Image image, UnitSize padding)
        {
            _illustration.Content(image);
            _illustration.Padding(padding);
            return this;
        }

        public TutorialModal LightDismiss()
        {
            _modal.LightDismiss();
            return this;
        }

        public TutorialModal NoLightDismiss()
        {
            _modal.NoLightDismiss();
            return this;
        }

        public TutorialModal ContentPadding(string padding)
        {
            _content.Padding(padding);
            return this;
        }

        public IComponent ShowEmbedded()
        {
            return Raw(_modal);
        }

        public TutorialModal Border(string color, UnitSize size = null)
        {
            size = size ?? 1.px();
            _modal.StylingContainer.style.borderColor = color;
            _modal.StylingContainer.style.borderWidth = size.ToString();
            _modal.StylingContainer.style.borderStyle = "solid";
            return this;
        }

        public TutorialModal Show()
        {
            _modal.Show();
            return this;
        }


        public TutorialModal OnHide(Modal.OnHideHandler onHide)
        {
            _modal.OnHide(onHide);
            return this;
        }

        public TutorialModal OnShow(Modal.OnShowHandler onShow)
        {
            _modal.OnShow(onShow);
            return this;
        }

        public void Hide(Action onHidden = null)
        {
            _modal.Hide(onHidden);
        }

        public dom.HTMLElement Render()
        {
            return _modal.Render();
        }
    }
}