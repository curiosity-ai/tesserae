using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H5.Core;
using Tesserae;
using Newtonsoft.Json;
using static H5.Core.dom;
using static Tesserae.UI;

namespace TssBoard
{
    internal static class App
    {
        private const string StorageKey = "tss-board-data-v1";
        private static BoardData _data;
        private static ObservableList<ColumnData> _columnsObservable;

        private static void Main()
        {
            document.body.style.overflow = "hidden";
            Theme.Light();

            _data = LoadData();
            _columnsObservable = new ObservableList<ColumnData>(_data.Columns.ToArray());
            _columnsObservable.Observe(_ => SaveData());

            var boardArea = Defer(_columnsObservable, cols => Task.FromResult(RenderBoard(cols)));

            var page = VStack().S().Children(
                HStack().P(24).AlignItemsCenter().Class("board-header").Children(
                    TextBlock("Project Alpha").XXLarge().Bold().Class("board-title"),
                    Button("Add Column").ML(24).OnClick((_, __) => {
                        _columnsObservable.Add(new ColumnData { Id = Guid.NewGuid().ToString(), Title = "New Column", Tasks = new List<TaskData>() });
                    })
                ),
                boardArea.W(1).Grow()
            );

            document.body.appendChild(page.Render());
        }

        private static IComponent RenderBoard(IReadOnlyList<ColumnData> columns)
        {
            var board = HStack().S().ScrollX().Class("board-canvas").P(24);
            foreach (var col in columns)
            {
                board.Add(RenderColumn(col));
            }
            return board;
        }

        private static IComponent RenderColumn(ColumnData col)
        {
            var tasksList = VStack().Class("column-tasks").W(1).Grow().S();
            var tasksObservable = new ObservableList<TaskData>(col.Tasks.ToArray());

            tasksObservable.Observe(_ => {
                col.Tasks = tasksObservable.ToList();
                _columnsObservable.NotifyObservers();
            });

            void RefreshTasks()
            {
                tasksList.Clear();
                foreach (var task in col.Tasks)
                {
                    tasksList.Add(RenderTask(task, col, tasksObservable));
                }
            }

            RefreshTasks();

            var colEl = VStack().W(300).Class("board-column").Children(
                HStack().AlignItemsCenter().P(12).Children(
                    TextBlock(col.Title).SemiBold().W(1).Grow(),
                    Button().SetIcon(UIcons.Plus).NoBackground().OnClick((_, __) => {
                        col.Tasks.Add(new TaskData { Id = Guid.NewGuid().ToString(), Title = "New Task", Description = "" });
                        tasksObservable.NotifyObservers();
                        RefreshTasks();
                    }),
                    Button().SetIcon(UIcons.Trash).NoBackground().OnClick((_, __) => {
                        _columnsObservable.Remove(col);
                    })
                ),
                tasksList
            ).Render();

            // Drag & Drop for Column
            colEl.ondragover = e => {
                e.preventDefault();
                colEl.classList.add("drag-over");
            };

            colEl.ondragleave = _ => colEl.classList.remove("drag-over");

            colEl.ondrop = e => {
                e.preventDefault();
                colEl.classList.remove("drag-over");
                var taskId = e.dataTransfer.getData("task-id");
                var sourceColId = e.dataTransfer.getData("source-col-id");

                if (sourceColId != col.Id)
                {
                    var sourceCol = _columnsObservable.First(c => c.Id == sourceColId);
                    var task = sourceCol.Tasks.First(t => t.Id == taskId);

                    sourceCol.Tasks.Remove(task);
                    col.Tasks.Add(task);

                    _columnsObservable.NotifyObservers();
                }
            };

            return Raw(colEl);
        }

        private static IComponent RenderTask(TaskData task, ColumnData col, ObservableList<TaskData> tasksObservable)
        {
            var card = Card(VStack().Children(
                TextBlock(task.Title).SemiBold(),
                If(!string.IsNullOrEmpty(task.Description), TextBlock(task.Description).Small().Secondary().MT(4))
            )).Class("task-card").MB(8);

            var el = card.Render();
            el.draggable = true;
            el.ondragstart = e => {
                e.dataTransfer.setData("task-id", task.Id);
                e.dataTransfer.setData("source-col-id", col.Id);
                el.classList.add("dragging");
            };
            el.ondragend = _ => el.classList.remove("dragging");

            card.OnClick((_, __) => {
                var dialog = Dialog("Edit Task");
                var titleBox = TextBox(task.Title).W(1).Grow();
                var descBox = TextArea(task.Description).W(1).Grow().H(100);

                dialog.Content(VStack().Children(
                    Label("Title").SetContent(titleBox),
                    Label("Description").MT(8).SetContent(descBox)
                ));

                dialog.OkCancel(() => {
                    task.Title = titleBox.Text;
                    task.Description = descBox.Text;
                    tasksObservable.NotifyObservers();
                });
            });

            return card;
        }

        private static BoardData LoadData()
        {
            var json = localStorage.getItem(StorageKey);
            if (string.IsNullOrEmpty(json)) return new BoardData {
                Columns = new List<ColumnData> {
                    new ColumnData { Id = "1", Title = "To Do", Tasks = new List<TaskData> { new TaskData { Id = "t1", Title = "Design UI", Description = "Create mockups" } } },
                    new ColumnData { Id = "2", Title = "In Progress", Tasks = new List<TaskData>() },
                    new ColumnData { Id = "3", Title = "Done", Tasks = new List<TaskData>() }
                }
            };
            try { return JsonConvert.DeserializeObject<BoardData>(json); }
            catch { return new BoardData { Columns = new List<ColumnData>() }; }
        }

        private static void SaveData()
        {
            _data.Columns = _columnsObservable.ToList();
            localStorage.setItem(StorageKey, JsonConvert.SerializeObject(_data));
        }
    }

    public class BoardData { public List<ColumnData> Columns { get; set; } }
    public class ColumnData { public string Id { get; set; } public string Title { get; set; } public List<TaskData> Tasks { get; set; } }
    public class TaskData { public string Id { get; set; } public string Title { get; set; } public string Description { get; set; } }

    public static class Extensions
    {
        public static void NotifyObservers<T>(this ObservableList<T> list)
        {
            var temp = list.ToList();
            list.ReplaceAll(temp);
        }
    }
}
