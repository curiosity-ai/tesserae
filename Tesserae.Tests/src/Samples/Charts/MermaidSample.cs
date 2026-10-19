using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Charts, Order = 65, Icon = UIcons.ChartTree, Description = "Mermaid diagram source drawn as themed SVG")]
    public class MermaidSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public MermaidSample()
        {
            const string flowchart =
@"flowchart LR
  A[Draft] --> B{Review}
  B -- approved --> C([Published])
  B -- changes --> D[Rework]
  D --> A";

            const string sequence =
@"sequenceDiagram
  participant U as User
  participant A as App
  participant S as Server
  U->>A: Click save
  A->>S: POST /documents
  S-->>A: 201 Created
  A-->>U: Saved";

            const string classDiagram =
@"classDiagram
  class Component {
    +Render() HTMLElement
  }
  class Mermaid {
    +Source string
    +Sanitize(bool)
  }
  Component <|-- Mermaid";

            const string state =
@"stateDiagram-v2
  [*] --> Idle
  Idle --> Loading: open
  Loading --> Ready: loaded
  Loading --> Failed: error
  Failed --> Loading: retry
  Ready --> [*]";

            const string pie =
@"pie title Where the time goes
  ""Parsing"" : 12
  ""Layout"" : 30
  ""Painting"" : 58";

            const string gantt =
@"gantt
  title Release
  dateFormat YYYY-MM-DD
  section Build
  Design :a1, 2026-01-05, 10d
  Code   :after a1, 12d
  section Ship
  Test   :crit, 2026-01-22, 6d";

            const string startingSource =
@"flowchart TD
  A[Edit the source] --> B{Valid?}
  B -- yes --> C[The diagram updates]
  B -- no --> D[The error is shown]";

            const string risky =
@"flowchart LR
  A[""<img src=x onerror=alert(1)>""] --> B[Diagram text is escaped]
  click B ""https://example.com/tracker"" ""a link""";

            var live       = Mermaid(startingSource);
            var liveError  = TextBlock("No error.").Secondary();
            var sanitize   = Toggle("Sanitize").Checked();

            live.OnError(e => liveError.Text = "Error: " + e);
            sanitize.OnChange((t, _) => live.Sanitize(t.IsChecked));

            var editor = TextArea(startingSource).WS().H(220).OnInput((ta, _) =>
            {
                liveError.Text = "No error.";
                live.Source    = ta.Text;
            });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(MermaidSample), UIcons.ChartTree, "A component that draws Mermaid diagram source as themed SVG")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Mermaid takes diagram source - flowcharts, sequence diagrams, class and state diagrams, charts, timelines and more - and draws it as SVG with Pele, a small renderer for Mermaid syntax. It is not the Mermaid library."),
                        TextBlock("The diagram takes its colors, fonts and radius from the Tesserae theme, so it follows light and dark mode and any custom theme without being redrawn. Pele is loaded the first time a diagram is drawn; until then the source is shown as text. The same renderer draws a mermaid code fence in MarkdownBlock."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Prefer Mermaid over a hand-drawn Diagram when the picture is a description of something (a process, a protocol, a schedule) rather than a canvas people arrange. Keep it in a container whose width does not depend on the diagram: it shrinks to fit and, when it has, can be zoomed and panned."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Flowchart"),
                        Mermaid(flowchart),
                        SampleSubTitle("Sequence diagram"),
                        Mermaid(sequence),
                        SampleSubTitle("Class diagram"),
                        Mermaid(classDiagram),
                        SampleSubTitle("State diagram"),
                        Mermaid(state),
                        SampleSubTitle("Pie chart"),
                        Mermaid(pie),
                        SampleSubTitle("Gantt chart"),
                        Mermaid(gantt))).SetTitle("Diagram types")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Live editing"),
                        TextBlock("Edit the source: the diagram redraws in place. A source that cannot be drawn is reported through Error and OnError, and the diagram shows the reason."),
                        editor,
                        sanitize,
                        liveError,
                        live)).SetTitle("Source")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Sanitization"),
                        TextBlock("Pele escapes the text of a diagram, and by default Mermaid also runs the SVG through DOMPurify before it is inserted: Mermaid(source) is the same as Mermaid(source, sanitize: true). Turn it off with Sanitize(false) only for source you wrote yourself."),
                        Mermaid(risky),
                        TextBlock("For source you do not trust to link anywhere or load a remote picture, take those out of the diagram too:"),
                        Mermaid(risky).Links(false).Images(false))).SetTitle("Untrusted source")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Invalid source"),
                        Mermaid("flowchart LR\n  A --> "),
                        SampleSubTitle("Empty"),
                        TextBlock("An empty source draws nothing."),
                        Mermaid(string.Empty))).SetTitle("Errors")))
               .SeeAlso(typeof(MarkdownBlockSample), typeof(DiagramSample), typeof(ChartsSample), typeof(GanttChartSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
