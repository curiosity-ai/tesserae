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
                            TaskBoardCard(TextBlock("Research user needs and identify key personas for the upcoming sprint."))
                                .Header(HStack().JustifyContent(ItemJustify.Between).Width(100.percent()).Children(
                                    Badge("Design").Primary(),
                                    TextBlock("T-101").Small().Secondary()
                                ))
                                .Footer(HStack().JustifyContent(ItemJustify.Between).Width(100.percent()).Children(
                                    Icon(UIcons.Comment).Small().Class("tss-text-secondary").Tooltip("3 Comments"),
                                    Avatar(initials: "JD").Size(AvatarSize.XSmall)
                                )),
                            TaskBoardCard(TextBlock("Draft UI mockups")),
                            TaskBoardCard(TextBlock("Setup project repo"))
                                .Footer(HStack().JustifyContent(ItemJustify.End).Width(100.percent()).Children(
                                    Avatar(initials: "KL").Size(AvatarSize.XSmall)
                                ))
                        ),
                    TaskBoardColumn("In Progress")
                        .Cards(
                            TaskBoardCard(TextBlock("Implement TaskBoard component").SemiBold())
                                .Header(HStack().JustifyContent(ItemJustify.Between).Width(100.percent()).Children(
                                    Badge("Feature").Danger(),
                                    TextBlock("T-102").Small().Secondary()
                                ))
                                .Footer(HStack().JustifyContent(ItemJustify.Between).Width(100.percent()).Children(
                                    HStack().Children(Icon(UIcons.PaperclipVertical).Small().Class("tss-text-secondary").Tooltip("2 Attachments")),
                                    Avatar(initials: "MW").Size(AvatarSize.XSmall)
                                )),
                            TaskBoardCard(TextBlock("Add Playwright tests"))
                                .Header(Badge("Testing").Success())
                        ),
                    TaskBoardColumn("Done")
                        .Cards(
                            TaskBoardCard(TextBlock("Create project plan"))
                                .Footer(HStack().JustifyContent(ItemJustify.Between).Width(100.percent()).Children(
                                    TextBlock("Approved").Small().Success(),
                                    Avatar(initials: "AS").Size(AvatarSize.XSmall).Presence(AvatarPresence.Online)
                                )),
                            TaskBoardCard(TextBlock("Initial meeting"))
                        )
                );

            var toggle = IconToggle(
                new IconToggle<bool>.Item(UIcons.TableColumns, "Column Mode", false),
                new IconToggle<bool>.Item(UIcons.TableRows, "Row Mode", true)
            );
            toggle.AsObservable().Observe(isRowMode => board.RowMode(isRowMode));

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
