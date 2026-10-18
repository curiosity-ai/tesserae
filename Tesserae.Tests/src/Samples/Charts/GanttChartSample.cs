using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Charts, Order = 42, Icon = UIcons.ChartGantt, Description = "Labelled rows of bars on a shared timeline")]
    public class GanttChartSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public GanttChartSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(GanttChartSample), UIcons.ChartGantt, "Labelled rows of bars laid out on a shared timeline")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A GanttChart lists rows in a label column and draws each row's bars on a horizontal axis shared by every row, so when things happen - and what overlaps what - can be read at a glance."),
                        TextBlock("The axis is either a set of named, equal-width columns (days, sprints, phases) or a numeric range with evenly spaced ticks (seconds, milliseconds, any unit you pick)."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Named columns"),
                        TextBlock("Columns(...) splits the axis into equal columns: column i spans i to i + 1, so a bar from 0 to 2 covers the first two."),
                        GanttChart()
                           .Columns("Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday")
                           .Add(GanttRow("Wireframing", GanttBar(0.05, 1.95).Color(GanttBarColor.Blue).Tooltip("Monday to Tuesday")))
                           .Add(GanttRow("Development", GanttBar(1.05, 2.95).Color(GanttBarColor.Green).Tooltip("Tuesday to Wednesday")))
                           .Add(GanttRow("Design",      GanttBar(2.05, 3.95).Color(GanttBarColor.Purple).Tooltip("Wednesday to Thursday")))
                           .Add(GanttRow("Testing",     GanttBar(3.05, 4.95).Color(GanttBarColor.Orange).Tooltip("Thursday to Friday"))))).SetTitle("A week")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("A numeric axis"),
                        TextBlock("Without columns the axis spans the bars, or the Range(...) you pin it to. TickFormat(...) writes the ticks; Text(...) puts a label beside a bar; Indent(...) nests a row under the one above; Striped() marks a span that was waiting rather than working. Click a bar to get it back through OnBarClick."),
                        BuildTrace())).SetTitle("Steps of a process")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Several bars on one row, custom colors"),
                        TextBlock("A row can hold any number of bars, and Color(string) takes any CSS color - the fill and the outline are derived from it."),
                        GanttChart()
                           .Title("Resource")
                           .Range(0, 24)
                           .Ticks(8)
                           .TickFormat(h => h.ToString("00") + ":00")
                           .LabelWidth(160.px())
                           .Add(GanttRow("Build agent 1", GanttBar(1, 4).Color(Theme.Colors.Teal500).Text("nightly"), GanttBar(9, 11).Color(Theme.Colors.Teal500), GanttBar(14, 18).Color(Theme.Colors.Magenta500).Text("release")))
                           .Add(GanttRow("Build agent 2", GanttBar(0, 2).Color(Theme.Colors.Teal500), GanttBar(10, 16).Color(Theme.Colors.Teal500).Text("tests")))
                           .Add(GanttRow("Build agent 3", GanttBar(20, 24).Color(Theme.Colors.Neutral500).Striped().Text("maintenance"))))).SetTitle("Shifts")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Empty"),
                        GanttChart().Columns("Q1", "Q2", "Q3", "Q4").EmptyMessage("No milestones planned yet."))).SetTitle("Empty state")))
               .SeeAlso(typeof(ChartsSample), typeof(ContributionBarSample), typeof(ActivityCalendarSample), typeof(TimelineSample));
        }

        private static IComponent BuildTrace()
        {
            string Seconds(double s) => s.ToString("0.0") + " s";

            GanttBar Step(double start, double end, GanttBarColor color) => GanttBar(start, end).Color(color).Text(Seconds(end - start)).Tooltip($"{Seconds(start)} → {Seconds(end)}");

            return GanttChart()
               .Title("Step")
               .TickFormat(Seconds)
               .OnBarClick(bar => Toast().Information("Bar clicked", $"{bar.Start:0.0} s → {bar.End:0.0} s"))
               .Add(GanttRow("Request",           Step(0,   9.4, GanttBarColor.Neutral)))
               .Add(GanttRow("Waiting for model", GanttBar(0, 0.8).Color(GanttBarColor.Neutral).Striped().Text("0.8 s")).Indent())
               .Add(GanttRow("Thinking",          Step(0.8, 2.1, GanttBarColor.Purple)).Indent())
               .Add(GanttRow("Search documents",  Step(2.1, 4.6, GanttBarColor.Green)).Indent())
               .Add(GanttRow("Read file",         Step(2.2, 3.4, GanttBarColor.Green)).Indent())
               .Add(GanttRow("Summarize",         Step(4.9, 7.8, GanttBarColor.Teal)).Indent())
               .Add(GanttRow("Rank results",      Step(5.1, 6.2, GanttBarColor.Green)).Indent(2))
               .Add(GanttRow("Failed lookup",     Step(6.3, 6.5, GanttBarColor.Red)).Indent(2))
               .Add(GanttRow("Answer",            Step(7.9, 9.4, GanttBarColor.Blue)).Indent());
        }

        public HTMLElement Render() => _content.Render();
    }
}
