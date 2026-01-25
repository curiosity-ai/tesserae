using Tesserae;
using Tesserae.Tests;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 10, Icon = UIcons.WindowRestore)]
    public class ModalSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ModalSample()
        {
            var container = Raw();

            Modal("Sample Modal")
               .Var(out var modal)
               .LightDismiss()
               .Width(60.vw())
               .Height(60.vh())
               .SetFooter(TextBlock("This is a footer note").SemiBold().MediumPlus())
               .Content(Stack().Children(
                    TextBlock("Modals provide a focused environment for users to complete a task or view important information. They can be configured with various options like dark overlays, non-blocking behavior, and draggable headers."),
                    Label("Light Dismiss").Inline().AutoWidth().SetContent(Toggle().OnChange((s,            e) => modal.CanLightDismiss     = s.IsChecked).Checked(modal.CanLightDismiss)),
                    Label("Is draggable").Inline().AutoWidth().SetContent(Toggle().OnChange((s,             e) => modal.IsDraggable         = s.IsChecked).Checked(modal.IsDraggable)),
                    Label("Is dark overlay").Inline().AutoWidth().SetContent(Toggle().OnChange((s,          e) => modal.IsDark              = s.IsChecked).Checked(modal.IsDark)),
                    Label("Is non-blocking").Inline().AutoWidth().SetContent(Toggle().OnChange((s,          e) => modal.IsNonBlocking       = s.IsChecked).Checked(modal.IsNonBlocking)),
                    Label("Hide close button").Inline().AutoWidth().SetContent(Toggle().OnChange((s,        e) => modal.WillShowCloseButton = !s.IsChecked).Checked(!modal.WillShowCloseButton)),
                    Label("Open a dialog from here").Var(out var lbl).SetContent(Button("Open").OnClick((s, e) => Dialog("Dialog over Modal").Content(TextBlock("Hello World!")).YesNo(() => lbl.Text = "Yes", () => lbl.Text = "No")))));

            _content = SectionStack()
               .Title(SampleHeader(nameof(ModalSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Modals are large overlays used for tasks that require a separate context, such as creating or editing complex entities, or for displaying rich content that shouldn't clutter the main interface. They provide more space than Dialogs and can host a variety of components.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use Modals for multi-step tasks or content-heavy interactions. Ensure that the Modal has a clear title and provide multiple ways to dismiss it (e.g., Close button, clicking outside, or the Escape key). Use 'LightDismiss' for non-critical information and blocking behavior only when user input is essential. Always maintain a clear typographic hierarchy within the Modal content.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Button("Open Modal").OnClick((s,                   e) => modal.Show()),
                    Button("Open Modal from top right").OnClick((s,    e) => modal.ShowAt(fromRight: 16.px(), fromTop: 16.px())),
                    Button("Open Modal with minimum size").OnClick((s, e) => Modal().CenterContent().LightDismiss().Dark().Content(TextBlock("small content").Tiny()).MinHeight(50.vh()).MinWidth(50.vw()).Show()),
                    SampleTitle("Embedded Modal"),
                    Button("Open Modal Below").OnClick((s, e) => container.Content(Modal("Embedded Modal").CenterContent().LightDismiss().Dark().Content(TextBlock("hosted small content").Tiny()).MinHeight(30.vh()).MinWidth(50.vw()).ShowEmbedded())),
                    container
                ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}