using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H5.Core;
using Tesserae;
using Newtonsoft.Json;
using static H5.Core.dom;
using static Tesserae.UI;

namespace TodoApp
{
    internal static class App
    {
        private const string StorageKey = "tss-todo-app-items-v2";

        private static void Main()
        {
            document.body.style.overflow = "hidden";
            Theme.Light();

            var todoItems = LoadItems();
            var observableItems = new ObservableList<TodoItem>(todoItems.ToArray());

            observableItems.Observe(_ => SaveItems(observableItems.ToList()));

            var newTodoText = TextBox().SetPlaceholder("What needs to be done?");
            var priorityDropdown = Dropdown().Items(
                DropdownItem("Low").Selected(),
                DropdownItem("Medium"),
                DropdownItem("High")
            ).W(120);

            var addButton = Button("Add Task").Primary();

            void AddItem()
            {
                if (!string.IsNullOrWhiteSpace(newTodoText.Text))
                {
                    var newItem = new TodoItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Text = newTodoText.Text,
                        IsDone = false,
                        Priority = priorityDropdown.SelectedItems.First().Text,
                        CreatedAt = DateTime.Now
                    };
                    observableItems.Add(newItem);
                    newTodoText.Text = string.Empty;
                }
            }

            addButton.OnClick((_, __) => AddItem());
            newTodoText.OnKeyDown((_, e) => { if (e.key == "Enter") AddItem(); });

            var listArea = Defer(observableItems, items => Task.FromResult(RenderList(observableItems)));

            var page = VStack().S().Children(
                VStack().Class("todo-header").P(32).Children(
                    TextBlock("Todo Master").XXLarge().Bold().Class("todo-title"),
                    HStack().MT(16).Children(
                        newTodoText.W(1).Grow(),
                        priorityDropdown.ML(8),
                        addButton.ML(8)
                    )
                ),
                VStack().S().ScrollY().P(32).Children(
                    listArea.W(1).Grow()
                )
            );

            document.body.appendChild(page.Render());
        }

        private static IComponent RenderList(ObservableList<TodoItem> items)
        {
            if (items.Count == 0)
            {
                return VStack().AlignCenter().JustifyCenter().MT(64).Children(
                    Icon(UIcons.ClipboardList, size: TextSize.Mega).Class("empty-icon"),
                    TextBlock("Nothing to do! Relax.").Medium().Secondary().MT(16)
                );
            }

            var stack = VStack().W(1).Grow();
            foreach (var item in items.OrderByDescending(i => GetPriorityWeight(i.Priority)).ThenByDescending(i => i.CreatedAt))
            {
                stack.Add(new TodoItemComponent(item, i => items.NotifyObservers(), i => items.Remove(i)));
            }
            return stack;
        }

        private static int GetPriorityWeight(string p) => p == "High" ? 3 : (p == "Medium" ? 2 : 1);

        private static List<TodoItem> LoadItems()
        {
            var json = localStorage.getItem(StorageKey);
            if (string.IsNullOrEmpty(json)) return new List<TodoItem>();
            try { return JsonConvert.DeserializeObject<List<TodoItem>>(json); }
            catch { return new List<TodoItem>(); }
        }

        private static void SaveItems(List<TodoItem> items)
        {
            localStorage.setItem(StorageKey, JsonConvert.SerializeObject(items));
        }
    }

    public class TodoItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool IsDone { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TodoItemComponent : IComponent
    {
        private readonly TodoItem _item;
        private readonly Action<TodoItem> _onChanged;
        private readonly Action<TodoItem> _onDelete;

        public TodoItemComponent(TodoItem item, Action<TodoItem> onChanged, Action<TodoItem> onDelete)
        {
            _item = item;
            _onChanged = onChanged;
            _onDelete = onDelete;
        }

        public HTMLElement Render()
        {
            var checkbox = CheckBox().Checked(_item.IsDone);
            var text = TextBlock(_item.Text).W(1).Grow().Class(_item.IsDone ? "todo-done" : "");

            checkbox.OnChange((_, __) => {
                _item.IsDone = checkbox.IsChecked;
                _onChanged(_item);
            });

            var priorityBadge = TextBlock(_item.Priority).Class("priority-badge").Class("priority-" + _item.Priority.ToLower());

            return Card(
                HStack().AlignItemsCenter().Children(
                    checkbox,
                    text.ML(12),
                    priorityBadge.ML(12),
                    Button().SetIcon(UIcons.Trash).Class("delete-btn").ML(12).OnClick((_, __) => _onDelete(_item))
                )
            ).Class("todo-card").MB(8).Render();
        }
    }

    public static class Extensions
    {
        public static void NotifyObservers<T>(this ObservableList<T> list)
        {
            var temp = list.ToList();
            list.ReplaceAll(temp);
        }
    }
}
