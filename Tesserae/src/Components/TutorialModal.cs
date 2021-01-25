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

        public TutorialModal(string title, string helpText)
        {
            _footerCommands = HStack().Padding(10.px()).AlignEnd().AlignItems(ItemAlign.End);
            _content        = VStack().S().ScrollY().Padding("38px 57px 0 39px");
            _title          = TextBlock(title).Large().Bold().PaddingTop(10.px()).PaddingBottom(20.px());
            _helpText       = TextBlock(helpText).Padding("20px 32px 0 0");
            _modal = Modal()
               .Width(800.px())
               .Height(60.vh())
               .NoContentPadding()
               .Content(
                    HStack().Children(
                        VStack().HS().OverflowHidden()
                           .Width(255.px())
                           .Padding("40px 13px 0 32px")
                           .Background(Theme.Secondary.Background)
                           .Children(
                                _title,
                                _helpText,
                                Image("./assets/img/box-img.svg").PT(30.px())),
                        VStack().HS().W(545.px()).JustifyContent(ItemJustify.End)
                           .Children(
                                _content,
                                _footerCommands)
                    )
                ).LightDismiss().Dark();
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