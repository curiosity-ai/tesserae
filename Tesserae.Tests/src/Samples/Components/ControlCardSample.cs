using Tesserae;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 104, Icon = UIcons.IdBadge)]
    public class ControlCardSample : IComponent, ISample
    {
        private IComponent _content;

        public ControlCardSample()
        {
            var checkbox = CheckBox();
            var toggle = Toggle();

            var controlCard1 = ControlCard(checkbox, "Enable Feature", "Turns on the new feature");
            var controlCard2 = ControlCard(toggle, "Airplane Mode", "Disables all wireless connections");

            _content = SectionStack()
                .Title(SampleHeader(nameof(ControlCardSample)))
                .Section(
                    Stack()
                        .Children(
                            TextBlock("ControlCard").MediumPlus().SemiBold(),
                            controlCard1,
                            controlCard2.MarginTop(16.px())
                        )
                );
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}