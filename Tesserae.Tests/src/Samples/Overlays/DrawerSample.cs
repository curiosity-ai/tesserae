using Tesserae;
using static Transpose.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Overlays, Order = 45, Icon = UIcons.ArrowFromBottom, Description = "A sheet sliding up from the bottom")]
    public class DrawerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public DrawerSample()
        {
            var list = VStack().WS();

            for (var i = 1; i <= 12; i++)
            {
                list.Add(Button("Search the workspace, call " + i).SetIcon(UIcons.Search).WS().TextLeft().NoBorder().NoBackground());
            }

            var drawer = Drawer("Used 12 tools").Content(list);

            var withFooter = Drawer("Share").Content(TextBlock("A drawer can carry a footer below its scrolling content, like a panel does.").Wrap())
               .SetFooter(HStack().JustifyContent(ItemJustify.End).Children(Button("Cancel"), Button("Share").Primary()));

            var modal = Modal(TextBlock("A modal that becomes a drawer").SemiBold())
               .ShowCloseButton()
               .LightDismiss()
               .DrawerOnMobile()
               .Content(TextBlock("On a desktop this is an ordinary modal. With the page in mobile mode - narrow the window below 768px - the same modal slides up from the bottom instead, with its header and close button.").Wrap());

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(DrawerSample), UIcons.ArrowFromBottom, "A drawer component")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("A Drawer is a sheet that slides up from the bottom of the screen over a dimmed page - the phone's answer to a Panel or a Modal. It has a grab handle, a title with a close button, a scrolling content area and an optional footer. It can be pulled down by its handle, or dismissed by tapping the page around it."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use a Drawer for a list or a short task a phone user reaches with their thumb. Keep it no taller than it needs to be: it is as tall as its content, up to 85% of the screen. An existing Modal does not need rebuilding - Modal.DrawerOnMobile() shows the same modal in a drawer while the page is in mobile mode."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(HStack().WS().Children(
                        Button("Open drawer").OnClick(() => drawer.Show()),
                        Button("Open drawer with footer").OnClick(() => withFooter.Show()),
                        Button("Open modal (drawer on mobile)").OnClick(() => modal.Show()))).SetTitle("Usage")))
               .SeeAlso(typeof(PanelSample), typeof(ModalSample), typeof(DialogSample));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
