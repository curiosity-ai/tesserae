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
                .OnColumnDrop(e => console.log($"Column dropped: oldIndex={e.oldIndex}, newIndex={e.newIndex}"))
                .OnColumnUpdate(e => console.log($"Column updated: oldIndex={e.oldIndex}, newIndex={e.newIndex}"))
                .Columns(
                    TaskBoardColumn("To Do")
                        .OnCardAdd(e => console.log("Card added to To Do"))
                        .OnCardRemove(e => console.log("Card removed from To Do"))
                        .OnCardDrop(e => console.log("Card dropped in To Do"))
                        .OnCardUpdate(e => console.log("Card updated in To Do"))
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
                        .OnCardAdd(e => console.log("Card added to In Progress"))
                        .OnCardRemove(e => console.log("Card removed from In Progress"))
                        .OnCardDrop(e => console.log("Card dropped in In Progress"))
                        .OnCardUpdate(e => console.log("Card updated in In Progress"))
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
                        .OnCardAdd(e => console.log("Card added to Done"))
                        .OnCardRemove(e => console.log("Card removed from Done"))
                        .OnCardDrop(e => console.log("Card dropped in Done"))
                        .OnCardUpdate(e => console.log("Card updated in Done"))
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
                IconToggleItem(UIcons.TableColumns, "Column Mode", false),
                IconToggleItem(UIcons.TableRows, "Row Mode", true)
            );
            toggle.AsObservable().Observe(isRowMode => board.RowMode(isRowMode));

            var readOnlyToggle = Toggle("Read Only").OnChange((s, e) => board.ReadOnly(s.IsChecked));

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(TaskBoardSample), UIcons.ClipboardList, "A board for managing tasks")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("TaskBoard provides a Trello-like interface with draggable columns and cards. Use it for Kanban boards and task management."),
                    HStack().Children(toggle, readOnlyToggle),
                    board.Height(600.px())
               )).SetTitle("Overview")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
