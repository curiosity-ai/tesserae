using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H5.Core;
using Tesserae;
using Newtonsoft.Json;
using static H5.Core.dom;
using static Tesserae.UI;

namespace FinanceTracker
{
    internal static class App
    {
        private const string StorageKey = "tss-finance-tracker-v2";

        private static void Main()
        {
            document.body.style.overflow = "hidden";
            Theme.Light();

            var data = LoadData();
            var observableTransactions = new ObservableList<Transaction>(data.Transactions.ToArray());

            observableTransactions.Observe(_ => {
                data.Transactions = observableTransactions.ToList();
                SaveData(data);
            });

            var descBox = TextBox().SetPlaceholder("What did you spend on?");
            var amountBox = NumberPicker(0).W(100);
            var categoryDropdown = Dropdown().Items(
                DropdownItem("Food"),
                DropdownItem("Rent"),
                DropdownItem("Salary"),
                DropdownItem("Entertainment"),
                DropdownItem("Other").Selected()
            ).W(150);

            var typeToggle = Toggle("Income", "Expense");

            var addButton = Button("Add").Primary();

            addButton.OnClick((_, __) => {
                if (!string.IsNullOrWhiteSpace(descBox.Text) && amountBox.Value != 0)
                {
                    var amount = (double)amountBox.Value;
                    if (!typeToggle.IsChecked) amount = -Math.Abs(amount);
                    else amount = Math.Abs(amount);

                    var transaction = new Transaction {
                        Id = Guid.NewGuid().ToString(),
                        Description = descBox.Text,
                        Amount = amount,
                        Category = categoryDropdown.SelectedItems.First().Text,
                        Date = DateTime.Now
                    };
                    observableTransactions.Insert(0, transaction);
                    descBox.SetText("");
                    amountBox.SetText("0");
                }
            });

            var dashboard = Defer(observableTransactions, ts => Task.FromResult(RenderDashboard(ts)));
            var listArea = Defer(observableTransactions, ts => Task.FromResult(RenderList(ts, observableTransactions)));

            var page = VStack().S().Children(
                VStack().Class("finance-header").P(32).Children(
                    TextBlock("Wealth Watch").XXLarge().Bold().Class("finance-title"),
                    dashboard.MT(16)
                ),
                VStack().S().ScrollY().P(32).Children(
                    HStack().AlignCenter().Children(
                        descBox.W(1).Grow(),
                        categoryDropdown.ML(8),
                        amountBox.ML(8),
                        typeToggle.ML(16),
                        addButton.ML(16)
                    ),
                    listArea.MT(32)
                )
            );

            document.body.appendChild(page.Render());
        }

        private static IComponent RenderDashboard(IReadOnlyList<Transaction> ts)
        {
            var totalBalance = ts.Sum(t => t.Amount);
            var income = ts.Where(t => t.Amount > 0).Sum(t => t.Amount);
            var expenses = ts.Where(t => t.Amount < 0).Sum(t => t.Amount);

            return HStack().Children(
                DashboardCard("Total Balance", totalBalance, totalBalance >= 0 ? "green" : "red"),
                DashboardCard("Monthly Income", income, "green").ML(16),
                DashboardCard("Monthly Expenses", expenses, "red").ML(16)
            );
        }

        private static IComponent DashboardCard(string title, double value, string colorClass)
        {
            return Card(VStack().Children(
                TextBlock(title).Small().Secondary().Bold(),
                TextBlock($"${value:N2}").Large().SemiBold().Class("text-" + colorClass)
            )).P(16).W(200);
        }

        private static IComponent RenderList(IReadOnlyList<Transaction> ts, ObservableList<Transaction> observableList)
        {
            if (!ts.Any()) return TextBlock("No transactions yet.").Medium().Secondary().AlignCenter().MT(32);

            var stack = VStack().W(1).Grow();
            foreach (var t in ts)
            {
                stack.Add(new TransactionComponent(t, () => observableList.Remove(t)));
            }
            return stack;
        }

        private static FinanceData LoadData()
        {
            var json = localStorage.getItem(StorageKey);
            if (string.IsNullOrEmpty(json)) return new FinanceData { Transactions = new List<Transaction>() };
            try { return JsonConvert.DeserializeObject<FinanceData>(json); }
            catch { return new FinanceData { Transactions = new List<Transaction>() }; }
        }

        private static void SaveData(FinanceData data)
        {
            localStorage.setItem(StorageKey, JsonConvert.SerializeObject(data));
        }
    }

    public class FinanceData
    {
        public List<Transaction> Transactions { get; set; }
    }

    public class Transaction
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
    }

    public class TransactionComponent : IComponent
    {
        private readonly Transaction _t;
        private readonly Action _onDelete;

        public TransactionComponent(Transaction t, Action onDelete)
        {
            _t = t;
            _onDelete = onDelete;
        }

        public HTMLElement Render() =>
            Card(HStack().AlignItemsCenter().Children(
                VStack().W(1).Grow().Children(
                    TextBlock(_t.Description).SemiBold(),
                    HStack().Children(
                        TextBlock(_t.Category).Small().Secondary(),
                        TextBlock(_t.Date.ToString("MMM dd, yyyy HH:mm")).Small().Secondary().ML(8)
                    )
                ),
                TextBlock($"{(_t.Amount >= 0 ? "+" : "")}{_t.Amount:N2}")
                    .Large().Bold()
                    .Class(_t.Amount >= 0 ? "text-green" : "text-red"),
                Button().SetIcon(UIcons.Trash).Class("delete-btn").ML(16).OnClick((_, __) => _onDelete())
            )).MB(8).Render();
    }
}
