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
        private const string StorageKey = "tss-finance-tracker-data";

        private static void Main()
        {
            document.body.style.overflow = "hidden";
            Theme.Light();

            var data = LoadData();
            var observableTransactions = new ObservableList<IComponent>();
            observableTransactions.AddRange(data.Transactions.Select(t => new TransactionComponent(t)));

            var balanceObservable = new SettableObservable<double>(data.Transactions.Sum(t => t.Amount));

            observableTransactions.Observe(_ => {
                var currentTransactions = observableTransactions.Cast<TransactionComponent>().Select(c => c.Transaction).ToList();
                data.Transactions = currentTransactions;
                SaveData(data);
                balanceObservable.Value = currentTransactions.Sum(t => t.Amount);
            });

            var descBox = TextBox();
            var amountBox = NumberPicker(0);
            var typeDropdown = Dropdown().Items(
                DropdownItem("Income").Selected(),
                DropdownItem("Expense")
            ).W(120.px());

            var addButton = Button("Add").Primary();

            addButton.OnClick((_, __) => {
                if (!string.IsNullOrWhiteSpace(descBox.Text))
                {
                    var amount = (double)amountBox.Value;
                    if (typeDropdown.SelectedItems.FirstOrDefault()?.Text == "Expense")
                    {
                        amount = -Math.Abs(amount);
                    }
                    else
                    {
                        amount = Math.Abs(amount);
                    }

                    var transaction = new Transaction {
                        Description = descBox.Text,
                        Amount = amount,
                        Date = DateTime.Now
                    };
                    observableTransactions.Add(new TransactionComponent(transaction));
                    descBox.SetText("");
                    amountBox.SetText("0");
                }
            });

            var page = VStack().S().Padding(32.px()).Children(
                TextBlock("Finance Tracker").XLarge().SemiBold(),
                Card(HStack().AlignItemsCenter().Children(
                    TextBlock("Total Balance:").Large(),
                    Defer(balanceObservable, b => Task.FromResult((IComponent)TextBlock($" ${b:N2}").Large().SemiBold().Foreground(b >= 0 ? "green" : "red")))
                )).Padding(16.px()).MarginTop(16.px()),
                HStack().MarginTop(32.px()).Children(
                    descBox.W(1).Grow().SetPlaceholder("Description"),
                    amountBox.MarginLeft(8.px()),
                    typeDropdown.MarginLeft(8.px()),
                    addButton.MarginLeft(8.px())
                ),
                SectionStack().Title(TextBlock("Recent Transactions").SemiBold()).MarginTop(32.px()).Section(
                    ItemsList(observableTransactions, 100.percent()).MaxHeight(50.vh()).ScrollY()
                )
            );

            document.body.appendChild(page.Render());
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
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
    }

    public class TransactionComponent : IComponent
    {
        public Transaction Transaction { get; }
        public TransactionComponent(Transaction t) => Transaction = t;

        public HTMLElement Render() =>
            HStack().AlignItemsCenter().Padding(8.px()).Children(
                TextBlock(Transaction.Date.ToString("yyyy-MM-dd HH:mm")).W(150.px()).Secondary(),
                TextBlock(Transaction.Description).W(1).Grow(),
                TextBlock($"{(Transaction.Amount >= 0 ? "+" : "")}{Transaction.Amount:N2}")
                    .SemiBold()
                    .Foreground(Transaction.Amount >= 0 ? "green" : "red")
            ).Render();
    }
}
