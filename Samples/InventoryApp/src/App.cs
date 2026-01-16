using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H5.Core;
using Tesserae;
using Newtonsoft.Json;
using static H5.Core.dom;
using static Tesserae.UI;

namespace InventoryApp
{
    internal static class App
    {
        private const string StorageKey = "tss-inventory-app-data";

        private static void Main()
        {
            document.body.style.overflow = "hidden";
            Theme.Light();

            var items = LoadItems();
            var observableItems = new ObservableList<InventoryItem>(items.ToArray());

            observableItems.Observe(_ => SaveItems(observableItems.ToList()));

            var filterObservable = new SettableObservable<string>("");
            var searchBox = SearchBox("Search products...").SearchAsYouType();
            searchBox.OnSearch((_, val) => filterObservable.Value = val);

            var listArea = Defer(observableItems, filterObservable, (list, filter) =>
                Task.FromResult(RenderItemList(observableItems, filter)));

            var page = VStack().S().Padding(32.px()).Children(
                HStack().AlignItemsCenter().Children(
                    TextBlock("Inventory Management").XLarge().SemiBold().W(1).Grow(),
                    Button("Add Product").Primary().OnClick((_, __) => ShowAddDialog(observableItems))
                ),
                searchBox.MarginTop(16.px()),
                listArea.W(1).Grow().MarginTop(16.px())
            );

            document.body.appendChild(page.Render());
        }

        private static IComponent RenderItemList(ObservableList<InventoryItem> allItems, string filter)
        {
            var filtered = allItems.Where(i => string.IsNullOrEmpty(filter) ||
                                              i.Name.ToLower().Contains(filter.ToLower()) ||
                                              i.Category.ToLower().Contains(filter.ToLower())).ToList();

            if (!filtered.Any()) return TextBlock("No products found").Medium().Secondary().MarginTop(32.px());

            var table = VStack().W(1).Grow();

            // Header
            table.Add(HStack().Padding(8.px()).Children(
                TextBlock("Product").SemiBold().W(1).Grow(),
                TextBlock("Category").SemiBold().W(150.px()),
                TextBlock("Stock").SemiBold().W(100.px()),
                TextBlock("Actions").SemiBold().W(100.px())
            ).Style(s => s.borderBottom = "1px solid #edebe9"));

            foreach (var item in filtered)
            {
                var row = HStack().AlignItemsCenter().Padding(8.px()).Children(
                    TextBlock(item.Name).W(1).Grow(),
                    TextBlock(item.Category).W(150.px()).Secondary(),
                    TextBlock(item.Quantity.ToString()).W(100.px()).SemiBold().Foreground(item.Quantity < 5 ? "red" : "inherit"),
                    HStack().W(100.px()).Children(
                        Button().SetIcon(UIcons.Plus).Small().OnClick((_, __) => { item.Quantity++; allItems.NotifyObservers(); }),
                        Button().SetIcon(UIcons.Minus).Small().MarginLeft(4.px()).OnClick((_, __) => { if (item.Quantity > 0) { item.Quantity--; allItems.NotifyObservers(); } })
                    )
                ).Style(s => s.borderBottom = "1px solid #f3f2f1");
                table.Add(row);
            }

            return table.ScrollY();
        }

        private static void ShowAddDialog(ObservableList<InventoryItem> items)
        {
            var nameBox = TextBox().SetPlaceholder("Product Name");
            var catBox = TextBox().SetPlaceholder("Category");
            var qtyBox = NumberPicker(0);

            var dialog = Dialog("Add New Product");
            dialog.Content(VStack().Children(
                Label("Name").SetContent(nameBox),
                Label("Category").MarginTop(8.px()).SetContent(catBox),
                Label("Initial Stock").MarginTop(8.px()).SetContent(qtyBox)
            ));

            dialog.OkCancel(() => {
                if (!string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    items.Add(new InventoryItem {
                        Name = nameBox.Text,
                        Category = catBox.Text,
                        Quantity = qtyBox.Value
                    });
                }
            });
        }

        private static List<InventoryItem> LoadItems()
        {
            var json = localStorage.getItem(StorageKey);
            if (string.IsNullOrEmpty(json)) return new List<InventoryItem> {
                new InventoryItem { Name = "Laptop", Category = "Electronics", Quantity = 10 },
                new InventoryItem { Name = "Coffee Mug", Category = "Kitchenware", Quantity = 50 }
            };
            try { return JsonConvert.DeserializeObject<List<InventoryItem>>(json); }
            catch { return new List<InventoryItem>(); }
        }

        private static void SaveItems(List<InventoryItem> items)
        {
            localStorage.setItem(StorageKey, JsonConvert.SerializeObject(items));
        }
    }

    public class InventoryItem
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
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
