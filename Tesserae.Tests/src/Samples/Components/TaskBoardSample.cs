using System;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 31, Icon = UIcons.Dashboard)]
    public class TaskBoardSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TaskBoardSample()
        {
            var board = TaskBoard()
                .Columns(
                    TaskBoardColumn("To Do")
                        .Cards(
                            TaskBoardCard(TextBlock("Research user needs")),
                            TaskBoardCard(TextBlock("Draft UI mockups")),
                            TaskBoardCard(TextBlock("Setup project repo"))
                        ),
                    TaskBoardColumn("In Progress")
                        .Cards(
                            TaskBoardCard(TextBlock("Implement TaskBoard component").SemiBold()),
                            TaskBoardCard(TextBlock("Add Playwright tests"))
                        ),
                    TaskBoardColumn("Done")
                        .Cards(
                            TaskBoardCard(TextBlock("Create project plan")),
                            TaskBoardCard(TextBlock("Initial meeting"))
                        )
                );

            var toggle = Toggle("Card Mode").OnChange((s, e) =>
            {
                board.CardMode(s.IsChecked);
            });

            _content = SectionStack()
               .Title(SampleHeader(nameof(TaskBoardSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("TaskBoard provides a Trello-like interface with draggable columns and cards. Use it for Kanban boards and task management."),
                    toggle,
                    board.Height(600.px())
               ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
