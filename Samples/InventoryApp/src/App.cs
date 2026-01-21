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
        private const string StorageKey = "tss-inventory-app-v3";

        private static void Main()
        {
            document.body.style.overflow = "hidden";
            Theme.Light();

            var items = LoadItems();
            var observableItems = new ObservableList<InventoryItem>(items.ToArray());
            var selectedItem = new SettableObservable<InventoryItem>(null);

            observableItems.Observe(_ => SaveItems(observableItems.ToList()));

            var filterObservable = new SettableObservable<string>("");
            var searchBox = SearchBox("Search products...").SearchAsYouType();
            searchBox.OnSearch((_, val) => filterObservable.Value = val);

            var listArea = Defer(observableItems, filterObservable, (list, filter) =>
                Task.FromResult(RenderItemList(observableItems, selectedItem, filter)));

            var detailsArea = Defer(selectedItem, item => Task.FromResult(RenderDetails(item, () => observableItems.NotifyObservers())));

            var mainContent = VStack().S().Children(
                HStack().P(32).AlignItemsCenter().Children(
                    TextBlock("Inventory Central").XXLarge().Bold().W(1).Grow().Class("inventory-title"),
                    Button("New Product").Primary().OnClick((_, __) => ShowAddDialog(observableItems))
                ),
                VStack().P(32).Children(searchBox),
                listArea.W(1).Grow()
            );

            var page = SplitView().RightIsSmaller(400.px()).Left(mainContent).Right(detailsArea).S();

            document.body.appendChild(page.Render());
        }

        private static IComponent RenderItemList(ObservableList<InventoryItem> allItems, SettableObservable<InventoryItem> selectedItem, string filter)
        {
            var filtered = allItems.Where(i => string.IsNullOrEmpty(filter) ||
                                              i.Name.ToLower().Contains(filter.ToLower()) ||
                                              i.Category.ToLower().Contains(filter.ToLower())).ToList();

            if (!filtered.Any()) return VStack().AlignCenter().MT(64).Children(TextBlock("No products found").Medium().Secondary());

            var table = VStack().W(1).Grow().ScrollY().P(32);

            // Header
            table.Add(HStack().Class("table-header").P(8).Children(
                TextBlock("Product").SemiBold().W(1).Grow(),
                TextBlock("Category").SemiBold().W(150),
                TextBlock("Status").SemiBold().W(120),
                TextBlock("Stock").SemiBold().W(80)
            ));

            foreach (var item in filtered)
            {
                var status = GetStatus(item.Quantity);
                var rowContent = HStack().AlignItemsCenter().P(8).Children(
                        TextBlock(item.Name).W(1).Grow(),
                        TextBlock(item.Category).W(150).Secondary(),
                        TextBlock(status.text).Class("status-badge").Class("status-" + status.css).W(120),
                        TextBlock(item.Quantity.ToString()).W(80).Bold()
                    );

                var row = Button().NoBackground().Class("inventory-row").W(1).Grow().ReplaceContent(rowContent);
                row.OnClick((_, __) => selectedItem.Value = item);
                table.Add(row);
            }

            return table;
        }

        private static IComponent RenderDetails(InventoryItem item, Action onChanged)
        {
            if (item == null) return VStack().S().AlignCenter().JustifyCenter().Class("details-empty").Children(TextBlock("Select a product to view details").Secondary());

            var nameBox = TextBox(item.Name).W(1).Grow();
            var catBox = TextBox(item.Category).W(1).Grow();
            var qtyBox = NumberPicker(item.Quantity).W(1).Grow();

            nameBox.OnInput((_, __) => { item.Name = nameBox.Text; onChanged(); });
            catBox.OnInput((_, __) => { item.Category = catBox.Text; onChanged(); });
            qtyBox.OnChange((_, __) => { item.Quantity = qtyBox.Value; onChanged(); });

            return VStack().S().P(32).Class("details-panel").Children(
                TextBlock("Product Details").Large().Bold().MB(24),
                Label("Product Name").SetContent(nameBox).MB(16),
                Label("Category").SetContent(catBox).MB(16),
                Label("Stock Quantity").SetContent(qtyBox).MB(16),
                VStack().MT(32).Children(
                    TextBlock("Stock Adjustment").SemiBold().MB(16),
                    HStack().Children(
                        Button("Restock +10").Success().W(1).Grow().OnClick((_, __) => { item.Quantity += 10; qtyBox.SetText(item.Quantity.ToString()); onChanged(); }),
                        Button("Reduce -10").Danger().ML(8).W(1).Grow().OnClick((_, __) => { item.Quantity = Math.Max(0, item.Quantity - 10); qtyBox.SetText(item.Quantity.ToString()); onChanged(); })
                    )
                )
            );
        }

        private static (string text, string css) GetStatus(int qty)
        {
            if (qty <= 0) return ("Out of Stock", "out");
            if (qty < 10) return ("Low Stock", "low");
            return ("In Stock", "ok");
        }

        private static void ShowAddDialog(ObservableList<InventoryItem> items)
        {
            var nameBox = TextBox().SetPlaceholder("e.g. Wireless Mouse");
            var catBox = TextBox().SetPlaceholder("e.g. Peripherals");
            var qtyBox = NumberPicker(0);

            var dialog = Dialog("Add New Product");
            dialog.Content(VStack().Children(
                Label("Name").SetContent(nameBox),
                Label("Category").MT(8).SetContent(catBox),
                Label("Initial Stock").MT(8).SetContent(qtyBox)
            ));

            dialog.OkCancel(() => {
                if (!string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    items.Insert(0, new InventoryItem {
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
                new InventoryItem { Name = "MacBook Pro", Category = "Electronics", Quantity = 5 },
                new InventoryItem { Name = "Dell XPS", Category = "Electronics", Quantity = 12 },
                new InventoryItem { Name = "Standing Desk", Category = "Furniture", Quantity = 0 }
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
