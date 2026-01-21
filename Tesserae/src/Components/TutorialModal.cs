using System;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A specialized Modal component designed for showing tutorials or onboarding information,
    /// featuring a split layout with an explanation area and a content area.
    /// </summary>
    [H5.Name("tss.TutorialModal")]
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

        /// <summary>
        /// Gets the styling container for the tutorial modal.
        /// </summary>
        public HTMLElement StylingContainer => _modal.StylingContainer;

        /// <summary>
        /// Gets whether styling should propagate to the stack item parent.
        /// </summary>
        public bool PropagateToStackItemParent => _modal.PropagateToStackItemParent;

        /// <summary>
        /// Initializes a new instance of the TutorialModal class.
        /// </summary>
        /// <param name="title">The title of the tutorial.</param>
        /// <param name="helpText">The help text or description.</param>
        /// <param name="imageSrc">An optional image source URL for the illustration.</param>
        public TutorialModal(string title, string helpText, string imageSrc = null)
        {
            _footerCommands = HStack().Padding(10.px()).WS().AlignItems(ItemAlign.End);
            _content        = VStack().Padding("38px 32px 0px 32px");
            _title          = TextBlock(title).Large().Bold().PaddingTop(10.px()).PaddingBottom(20.px());
            _helpText       = TextBlock(helpText).Padding("20px 30px 0 0");
            _illustration   = Raw().W(196).H(196);

            if (!string.IsNullOrWhiteSpace(imageSrc))
            {
                SetImageSrc(imageSrc, 16.px());
            }

            _leftStack = VStack().OverflowHidden().Class("tss-tutorial-modal-explanation")
               .RemovePropagation()
               .Width(300.px()).Padding("40px 32px 32px 32px").Background(Theme.Secondary.Background)
               .Children(VStack().S().JustifyContent(ItemJustify.Between).Children(_title, _helpText, Raw().H(48.px()).Grow(), _illustration));

            _rightStack = VStack().Class("tss-tutorial-modal-content").OverflowHidden().HS().W(10.px()).Grow().JustifyContent(ItemJustify.End)
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

        /// <summary>
        /// Sets the footer command components.
        /// </summary>
        /// <param name="commands">The command components.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal SetFooterCommands(params IComponent[] commands)
        {
            _footerCommands.Clear();
            _footerCommands.Children(commands);
            return this;
        }

        /// <summary>
        /// Sets the main content of the tutorial modal.
        /// </summary>
        /// <param name="content">The content component.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal SetContent(IComponent content)
        {
            _content.Clear();
            _content.Children(content);
            return this;
        }

        /// <summary>
        /// Sets the height of the tutorial modal.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal Height(UnitSize height)
        {
            _leftStack.Height(height);
            _rightStack.Height(height);
            return this;
        }

        /// <summary>
        /// Sets the height of the tutorial modal.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal H(UnitSize height) => Height(height);

        /// <summary>
        /// Sets the width of the tutorial modal.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal Width(UnitSize width)
        {
            _modal.Width(width);
            return this;
        }

        /// <summary>
        /// Sets the width of the tutorial modal.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal W(UnitSize width) => Width(width);

        /// <summary>
        /// Sets the title of the tutorial modal.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal SetTitle(string title)
        {
            _title.Text = title;
            return this;
        }

        /// <summary>
        /// Sets the help text of the tutorial modal.
        /// </summary>
        /// <param name="helpText">The help text.</param>
        /// <param name="treatAsHTML">Whether to treat the help text as HTML.</param>
        /// <returns>The current instance of the type.</returns>
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

        /// <summary>
        /// Sets the illustration image source URL and padding.
        /// </summary>
        /// <param name="imageSrc">The image source URL.</param>
        /// <param name="padding">The padding around the image.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal SetImageSrc(string imageSrc, UnitSize padding)
        {
            _illustration.Content(Image(imageSrc).Contain().MaxWidth(100.percent()).MaxHeight(100.percent()));
            _illustration.Padding(padding);
            return this;
        }

        /// <summary>
        /// Sets the illustration image and padding.
        /// </summary>
        /// <param name="image">The image component.</param>
        /// <param name="padding">The padding around the image.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal SetImage(Image image, UnitSize padding)
        {
            _illustration.Content(image);
            _illustration.Padding(padding);
            return this;
        }

        /// <summary>
        /// Enables light dismiss (closing the modal when clicking outside of it).
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal LightDismiss()
        {
            _modal.LightDismiss();
            return this;
        }

        /// <summary>
        /// Disables light dismiss.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal NoLightDismiss()
        {
            _modal.NoLightDismiss();
            return this;
        }

        /// <summary>
        /// Sets the padding for the content area.
        /// </summary>
        /// <param name="padding">The padding.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal ContentPadding(string padding)
        {
            _content.Padding(padding);
            return this;
        }

        /// <summary>
        /// Returns the tutorial modal as an embedded component rather than showing it as a modal layer.
        /// </summary>
        /// <returns>An IComponent representing the embedded modal.</returns>
        public IComponent ShowEmbedded()
        {
            return Raw(_modal);
        }

        /// <summary>
        /// Sets the border color and size for the tutorial modal.
        /// </summary>
        /// <param name="color">The border color.</param>
        /// <param name="size">The border size.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal Border(string color, UnitSize size = null)
        {
            size                                      = size ?? 1.px();
            _modal.StylingContainer.style.borderColor = color;
            _modal.StylingContainer.style.borderWidth = size.ToString();
            _modal.StylingContainer.style.borderStyle = "solid";
            return this;
        }

        /// <summary>
        /// Shows the tutorial modal.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal Show()
        {
            _modal.Show();
            return this;
        }

        /// <summary>
        /// Adds a hide event handler to the tutorial modal.
        /// </summary>
        /// <param name="onHide">The hide event handler.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal OnHide(Modal.OnHideHandler onHide)
        {
            _modal.OnHide(onHide);
            return this;
        }

        /// <summary>
        /// Adds a show event handler to the tutorial modal.
        /// </summary>
        /// <param name="onShow">The show event handler.</param>
        /// <returns>The current instance of the type.</returns>
        public TutorialModal OnShow(Modal.OnShowHandler onShow)
        {
            _modal.OnShow(onShow);
            return this;
        }

        /// <summary>
        /// Hides the tutorial modal.
        /// </summary>
        /// <param name="onHidden">An optional action to perform when the modal is hidden.</param>
        public void Hide(Action onHidden = null)
        {
            _modal.Hide(onHidden);
        }

        /// <summary>
        /// Renders the tutorial modal.
        /// </summary>
        /// <returns>The rendered HTMLElement.</returns>
        public dom.HTMLElement Render()
        {
            return _modal.Render();
        }
    }
}