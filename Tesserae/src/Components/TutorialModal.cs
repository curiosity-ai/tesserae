using System;
using H5.Core;
using static Tesserae.UI;

namespace Tesserae
{
    public class TutorialModal : IComponent
    {
        private readonly Modal     _modal;
        private readonly Stack     _footerCommands;
        private readonly Stack     _content;
        private readonly TextBlock _title;
        private readonly TextBlock _helpText;
        private readonly Raw       _illustration;

        public TutorialModal(string title, string helpText, string imageSrc = null)
        {
            _footerCommands = HStack().Padding(10.px()).AlignEnd().AlignItems(ItemAlign.End);
            _content        = VStack().Padding("38px 57px 0 39px");
            _title          = TextBlock(title).Large().Bold().PaddingTop(10.px()).PaddingBottom(20.px());
            _helpText       = TextBlock(helpText).Padding("20px 30px 0 0");
            _illustration   = Raw();


            if (string.IsNullOrWhiteSpace(imageSrc))
            {
                _illustration.Content(Image(imageSrc).PT(40.px()));
            }

            _modal = Modal().Width(800.px()).Height(60.vh())
               .NoContentPadding().LightDismiss().Dark()
               .Content(
                    HStack().Children(
                        VStack().HS().OverflowHidden().JustifyContent(ItemJustify.Between)
                           .Width(255.px()).Padding("40px 13px 0 32px").Background(Theme.Secondary.Background)
                           .Children(
                                VStack().Children(_title, _helpText),
                                _illustration),
                        VStack().HS().W(545.px()).JustifyContent(ItemJustify.End)
                           .Children(
                                _content.H(10.px()).Grow(),
                                _footerCommands)
                    )
                );
        }

        public TutorialModal SetFooterCommands(params IComponent[] commands)
        {
            _footerCommands.Clear();
            _footerCommands.Children(commands);
            return this;
        }
        public TutorialModal SetContent(params IComponent[] content)
        {
            _content.Clear();
            _content.Children(content);
            return this;
        }

        public TutorialModal SetTitle(string title)
        {
            _title.Text = title;
            return this;
        }
        public TutorialModal SetHelpText(string helpText)
        {
            _helpText.Text = helpText;
            return this;
        }

        public TutorialModal SetImageSrc(string imageSrc)
        {
            _illustration.Content(Image(imageSrc).PT(30.px()));
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

        public TutorialModal Show()
        {
            _modal.Show();
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