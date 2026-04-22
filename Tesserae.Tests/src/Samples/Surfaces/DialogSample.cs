using System;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 10, Icon = UIcons.WindowMinimize)]
    public class DialogSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public DialogSample()
        {
            var dialog   = Dialog("Sample Dialog");
            var response = TextBlock();

            _content = SectionStack()
               .Title(SampleHeader(nameof(DialogSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Dialogs are modal UI overlays that provide contextual information or require user action, such as confirmation or input. They are designed to capture the user's attention and typically block interaction with the rest of the application until they are dismissed.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use Dialogs for critical or short-term tasks that require a decision. Ensure the content is brief and clearly states the purpose. Provide logical action buttons (e.g., 'Confirm' and 'Cancel') and highlight the primary action. Avoid overusing Dialogs for non-essential information to prevent frustrating the user. Consider using non-modal alternatives if the task doesn't require immediate attention.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Button("Open Dialog").OnClick((c, ev) => dialog.Show()),
                    HStack().Children(
                        Button("Open YesNo").OnClick((c,       ev) => Dialog("Sample Dialog").YesNo(() => response.Text("Clicked Yes"), () => response.Text("Clicked No"))),
                        Button("Open YesNoCancel").OnClick((c, ev) => Dialog("Sample Dialog").YesNoCancel(() => response.Text("Clicked Yes"), () => response.Text("Clicked No"), () => response.Text("Clicked Cancel"))),
                        Button("Open Ok").OnClick((c,          ev) => Dialog("Sample Dialog").Ok(() => response.Text("Clicked Ok"))),
                        Button("Open RetryCancel").OnClick((c, ev) => Dialog("Sample Dialog").RetryCancel(() => response.Text("Clicked Retry"), () => response.Text("Clicked Cancel")))),
                    Button("Open YesNo with dark overlay").OnClick((c,       ev) => Dialog("Sample Dialog").Dark().YesNo(() => response.Text("Clicked Yes"), () => response.Text("Clicked No"), y => y.Success().SetText("Yes!"), n => n.Danger().SetText("Nope"))),
                    Button("Open YesNoCancel with dark overlay").OnClick((c, ev) => Dialog("Sample Dialog").Dark().YesNoCancel(() => response.Text("Clicked Yes"), () => response.Text("Clicked No"), () => response.Text("Clicked Cancel"))),
                    Button("Open Ok with dark overlay").OnClick((c,          ev) => Dialog("Sample Dialog").Dark().Ok(() => response.Text("Clicked Ok"))),
                    Button("Open RetryCancel with dark overlay").OnClick((c, ev) => Dialog("Sample Dialog").Dark().RetryCancel(() => response.Text("Clicked Retry"), () => response.Text("Clicked Cancel"))),
                    response));

            dialog.Content(Stack().Children(TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit."),
                    Toggle("Is draggable").OnChange((c,    ev) => dialog.IsDraggable = c.IsChecked),
                    Toggle("Is dark overlay").OnChange((c, ev) => dialog.IsDark      = c.IsChecked).Checked(dialog.IsDark)
                ))
               .Commands(Button("Send").Primary().AlignEnd().OnClick((c, ev) => dialog.Hide()), Button("Don`t send").AlignEnd().OnClick((c, ev) => dialog.Hide()));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}