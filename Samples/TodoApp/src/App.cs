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
        private const string StorageKey = "tss-todo-app-items";

        private static void Main()
        {
            document.body.style.overflow = "hidden";
            Theme.Light();

            var todoItems = LoadItems();

            var observableItems = new ObservableList<IComponent>();

            void OnItemChanged(TodoItem item)
            {
                SaveItems(observableItems.Cast<TodoItemComponent>().Select(c => c.Item).ToList());
            }

            observableItems.AddRange(todoItems.Select(i => new TodoItemComponent(i, OnItemChanged)));
            observableItems.Observe(_ => SaveItems(observableItems.Cast<TodoItemComponent>().Select(c => c.Item).ToList()));

            var newTodoText = TextBox();
            var addButton = Button("Add").Primary();

            void AddItem()
            {
                if (!string.IsNullOrWhiteSpace(newTodoText.Text))
                {
                    var newItem = new TodoItem { Text = newTodoText.Text, IsDone = false };
                    observableItems.Add(new TodoItemComponent(newItem, OnItemChanged));
                    newTodoText.Text = string.Empty;
                }
            }

            addButton.OnClick((_, __) => AddItem());
            newTodoText.OnKeyDown((_, e) => { if (e.key == "Enter") AddItem(); });

            var list = ItemsList(observableItems, 100.percent());

            var page = VStack().S().Padding(32.px()).Children(
                TextBlock("Todo App").XLarge().SemiBold(),
                HStack().MarginTop(16.px()).Children(
                    newTodoText.W(1).Grow(),
                    addButton.MarginLeft(8.px())
                ),
                SectionStack().Title(TextBlock("Your Tasks").SemiBold()).MarginTop(32.px()).Section(
                    list.MaxHeight(60.vh()).ScrollY()
                )
            );

            document.body.appendChild(page.Render());
        }

        private static List<TodoItem> LoadItems()
        {
            var json = localStorage.getItem(StorageKey);
            if (string.IsNullOrEmpty(json)) return new List<TodoItem>();
            try
            {
                return JsonConvert.DeserializeObject<List<TodoItem>>(json);
            }
            catch
            {
                return new List<TodoItem>();
            }
        }

        private static void SaveItems(List<TodoItem> items)
        {
            var json = JsonConvert.SerializeObject(items);
            localStorage.setItem(StorageKey, json);
        }
    }

    public class TodoItem
    {
        public string Text { get; set; }
        public bool IsDone { get; set; }
    }

    public class TodoItemComponent : IComponent
    {
        public TodoItem Item { get; }
        private readonly Action<TodoItem> _onChanged;

        public TodoItemComponent(TodoItem item, Action<TodoItem> onChanged)
        {
            Item = item;
            _onChanged = onChanged;
        }

        public HTMLElement Render()
        {
            var checkbox = CheckBox(Item.Text).Checked(Item.IsDone);
            checkbox.OnChange((_, __) => {
                Item.IsDone = checkbox.IsChecked;
                _onChanged(Item);
            });

            return HStack().AlignItemsCenter().Children(
                checkbox.W(1).Grow()
            ).Render();
        }
    }
}
