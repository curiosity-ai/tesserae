using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 103, Icon = UIcons.Comment)]
    public class QuestionnaireSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public QuestionnaireSample()
        {
            _content = SectionStack().Secondary()
                .SampleTitle(typeof(QuestionnaireSample), UIcons.Comment, "Inline question + answer-options widget for AI chats")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Questionnaire renders a single question and a list of selectable answer options. Once the user picks an option (or one is set programmatically) the component switches to a \"response selected\" mode that disables further input and highlights the chosen answer.")
                    )).SetTitle("Overview")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Unanswered"),
                        Questionnaire("Do you want to delete this file?", "Yes", "No", "Show me the diff first")
                            .OnAnswered(q => console.log("Answered: " + q.Answer)),

                        SampleSubTitle("Pre-answered (response-selected mode)"),
                        Questionnaire("Which framework should we use?", "React", "Vue", "Svelte", "Solid")
                            .SetAnswer("Svelte")
                    )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
