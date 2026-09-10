using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Lists, Order = 5, Icon = UIcons.RectangleList, Description = "One list row: avatar, two lines, trailing controls")]
    public class StatefulItemRowSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public StatefulItemRowSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(StatefulItemRowSample), UIcons.RectangleList, "A full-width list row, washed in a tone to say what is happening to it")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("StatefulItemRow is the whole row that ListItemText is only the text of: a leading visual, a title with a quieter line under it, and whatever the row is acted on with pinned to the far end. The text ellipsizes rather than pushing the controls off the edge."),
                        TextBlock("A tone washes the row in one of the theme's colors, which is what lets a list show a change before it has been saved — added reads green, changed blue, on its way out red and struck through."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("A row"),
                        StatefulItemRow(Avatar(initials: "DK").Size(AvatarSize.Medium))
                           .SetTitle("Dana Kaur")
                           .SetSubtitle("dana.kaur@curiosity.ai")
                           .Trailing(Button("Can view").NoBorder().NoBackground(), Button().SetIcon(UIcons.Cross).NoBorder().NoBackground()),

                        SampleSubTitle("Tones"),
                        VStack().WS().Children(
                            StatefulItemRow(Avatar(initials: "SB").Size(AvatarSize.Medium))
                               .SetTitle("Samuel Barros")
                               .SetSubtitle("samuel.barros@curiosity.ai")
                               .Success()
                               .Trailing(Badge("Added").Pill().Outline().Success()),

                            StatefulItemRow(Avatar(initials: "JN").Size(AvatarSize.Medium))
                               .SetTitle("Jonas Neumann")
                               .SetSubtitle("jonas.neumann@curiosity.ai")
                               .Primary()
                               .Trailing(Badge("Changed").Pill().Outline().Primary()),

                            StatefulItemRow(Avatar(initials: "AS").Size(AvatarSize.Medium))
                               .SetTitle("Aiko Sato")
                               .SetSubtitle("aiko.sato@curiosity.ai")
                               .Danger()
                               .Struck()
                               .Trailing(Badge("Removing").Pill().Outline().Danger(), Button("Undo").NoBorder().NoBackground()),

                            StatefulItemRow(Avatar(initials: "TM").Size(AvatarSize.Medium))
                               .SetTitle("Tomas Mendez")
                               .SetSubtitle("Already has access")
                               .Muted()),

                        SampleSubTitle("Compact, clickable, and with no leading visual"),
                        VStack().WS().Children(
                            StatefulItemRow(Avatar(initials: "SA").Size(AvatarSize.Small))
                               .SetTitle("Sales — DACH")
                               .SetSubtitle("24 members")
                               .Compact()
                               .Interactive()
                               .OnClick(() => Toast().Information("Sales — DACH")),

                            StatefulItemRow()
                               .SetTitle("No leading visual")
                               .SetSubtitle("The row starts at its text")
                               .Compact())
                    )).SetTitle("Usage")))
               .SeeAlso(typeof(ListItemTextSample), typeof(AvatarSample), typeof(ItemsListSample), typeof(DetailsListSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
