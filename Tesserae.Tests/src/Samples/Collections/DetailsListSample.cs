using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 20, Icon = UIcons.TableRows)]
    public class DetailsListSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public DetailsListSample()
        {
            var query = Router.GetQueryParameters();
            int page  = query.ContainsKey("page") && query.TryGetValue("page", out var queryPageStr) && int.TryParse(queryPageStr, out var queryPage) ? queryPage : 2;

            _content = SectionStack()
                   .Title(SampleHeader(nameof(DetailsListSample)))
                   .Section(Stack().Children(
                        SampleTitle("Overview"),
                        TextBlock("DetailsList is a robust way to display an information-rich collection of items. It supports sorting, grouping, filtering, and pagination."),
                        TextBlock("It is classically used for file explorers, database record views, or any scenario where information density is critical.")))
                   .Section(Stack().Children(
                        SampleTitle("Best Practices"),
                        TextBlock("Use DetailsList when users need to compare items across multiple metadata fields. Display columns in order of importance from left to right. Provide ample default width for each column to avoid unnecessary truncation. Use compact mode when vertical space is limited or when displaying very large datasets. Always provide a clear empty state message if the list contains no items.")))
                   .Section(Stack().Children(
                        SampleTitle("Usage"),
                        SampleSubTitle("Standard File List"),
                        TextBlock("A list with textual rows, supporting sorting and custom column widths."),
                        DetailsList<DetailsListSampleFileItem>(
                                IconColumn(Icon(UIcons.File), width: 32.px()),
                                DetailsListColumn(title: "File Name", width: 350.px(), enableColumnSorting: true, sortingKey: "FileName", isRowHeader: true),
                                DetailsListColumn(title: "Date Modified", width: 170.px(), enableColumnSorting: true, sortingKey: "DateModified"),
                                DetailsListColumn(title: "Modified By", width: 150.px(), enableColumnSorting: true, sortingKey: "ModifiedBy"),
                                DetailsListColumn(title: "File Size", width: 120.px(), enableColumnSorting: true, sortingKey: "FileSize"))
                           .Height(400.px())
                           .WithListItems(GetDetailsListItems())
                           .SortedBy("FileName")
                           .MB(32),
                        SampleSubTitle("Responsive Widths"),
                        TextBlock("Using percentage-based widths with max-width constraints."),
                        DetailsList<DetailsListSampleFileItem>(
                                IconColumn(Icon(UIcons.File), width: 64.px()),
                                DetailsListColumn(title: "File Name", width: 40.percent(), enableColumnSorting: true, sortingKey: "FileName", isRowHeader: true),
                                DetailsListColumn(title: "Date Modified", width: 30.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: "DateModified"),
                                DetailsListColumn(title: "Modified By", width: 30.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: "ModifiedBy"),
                                DetailsListColumn(title: "File Size", width: 10.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: "FileSize"))
                           .Height(400.px())
                           .WidthStretch()
                           .WithListItems(GetDetailsListItems())
                           .MB(32),
                        SampleSubTitle("Interactive Components in Rows"),
                        TextBlock("DetailsList can host any Tesserae component within its cells."),
                        DetailsList<DetailsListSampleItemWithComponents>(
                                IconColumn(Icon(UIcons.AppleWhole), width: 32.px()),
                                DetailsListColumn(title: "Status", width: 120.px()),
                                DetailsListColumn(title: "Name", width: 250.px(), isRowHeader: true),
                                DetailsListColumn(title: "Action", width: 150.px()),
                                DetailsListColumn(title: "Rating", width: 400.px()))
                           .Compact()
                           .Height(400.px())
                           .WithListItems(GetComponentDetailsListItems())
                           .MB(32),
                        SampleSubTitle("Paginated and Empty States"),
                        SplitView().Resizable().SplitInMiddle().Left(
                            VStack().WS().Children(
                                TextBlock("Infinite scrolling").SemiBold(),
                                DetailsList<DetailsListSampleFileItem>(
                                        IconColumn(Icon(UIcons.File), width: 64.px()),
                                        DetailsListColumn(title: "File Name", width: 40.percent(), enableColumnSorting: true, sortingKey: "FileName", isRowHeader: true),
                                        DetailsListColumn(title: "Date Modified", width: 30.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: "DateModified"),
                                        DetailsListColumn(title: "Modified By", width: 30.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: "ModifiedBy"),
                                        DetailsListColumn(title: "File Size", width: 10.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: "FileSize"))
                                   .Height(300.px())
                                   .WithListItems(GetDetailsListItems(0, page))
                                   .WithPaginatedItems(async () =>
                                    {
                                        page++;
                                        Router.ReplaceQueryParameters(p => p.With("page", page.ToString()));
                                        return await GetDetailsListItemsAsync(page, 5);
                                    })
                            )).Right(
                            VStack().WS().Children(
                                TextBlock("Empty State").SemiBold(),
                                DetailsList<DetailsListSampleFileItem>(
                                        IconColumn(Icon(UIcons.File), width: 64.px()),
                                        DetailsListColumn(title: "File Name", width: 40.percent(), enableColumnSorting: true, sortingKey: "FileName", isRowHeader: true),
                                        DetailsListColumn(title: "Date Modified", width: 30.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: "DateModified"),
                                        DetailsListColumn(title: "Modified By", width: 30.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: "ModifiedBy"),
                                        DetailsListColumn(title: "File Size", width: 10.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: "FileSize"))
                                   .WithEmptyMessage(() => BackgroundArea(Card(TextBlock("No items found").Padding(16.px()))).WS().HS())
                                   .Height(300.px())
                                   .WithListItems(new DetailsListSampleFileItem[0])
                            )
                        )
                    ));
        }

        private async Task<DetailsListSampleFileItem[]> GetDetailsListItemsAsync(int start = 1, int count = 100)
        {
            await Task.Delay(1000);
            return GetDetailsListItems(start, count);
        }

        private DetailsListSampleFileItem[] GetDetailsListItems(int start = 1, int count = 100)
        {
            return Enumerable.Range(start, count).Select(n => new DetailsListSampleFileItem(UIcons.FileWord, $"Document_{n}.docx", DateTime.Today.AddDays(-n), "System", n * 1.5)).ToArray();
        }

        private DetailsListSampleItemWithComponents[] GetComponentDetailsListItems()
        {
            return Enumerable.Range(1, 20).Select(n => new DetailsListSampleItemWithComponents()
                       .WithIcon(UIcons.Database)
                       .WithCheckBox(CheckBox("Active").Checked())
                       .WithName($"Record {n}")
                       .WithButton(Button("Edit").Primary().OnClick((s, e) => Toast().Information($"Editing {n}")))
                       .WithChoiceGroup(ChoiceGroup().Horizontal().Choices(Choice("A"), Choice("B"), Choice("C")))
                ).ToArray();
        }

        public HTMLElement Render() => _content.Render();
    }

    public class DetailsListSampleFileItem : IDetailsListItem<DetailsListSampleFileItem>
    {
        public DetailsListSampleFileItem(UIcons icon, string name, DateTime modified, string by, double size) { FileIcon = icon; FileName = name; DateModified = modified; ModifiedBy = by; FileSize = size; }
        public UIcons FileIcon { get; }
        public string FileName { get; }
        public DateTime DateModified { get; }
        public string ModifiedBy { get; }
        public double FileSize { get; }
        public bool EnableOnListItemClickEvent => true;
        public void OnListItemClick(int index) => Toast().Information($"Clicked: {FileName}");
        public int CompareTo(DetailsListSampleFileItem other, string key)
        {
            switch (key)
            {
                case "FileName":     return string.Compare(FileName, other.FileName, StringComparison.OrdinalIgnoreCase);
                case "DateModified": return DateModified.CompareTo(other.DateModified);
                case "ModifiedBy":   return string.Compare(ModifiedBy, other.ModifiedBy, StringComparison.OrdinalIgnoreCase);
                case "FileSize":     return FileSize.CompareTo(other.FileSize);
                default:             return 0;
            }
        }
        public IEnumerable<IComponent> Render(IList<IDetailsListColumn> columns, Func<IDetailsListColumn, Func<IComponent>, IComponent> cell)
        {
            yield return cell(columns[0], () => Icon(FileIcon));
            yield return cell(columns[1], () => TextBlock(FileName));
            yield return cell(columns[2], () => TextBlock(DateModified.ToShortDateString()));
            yield return cell(columns[3], () => TextBlock(ModifiedBy));
            yield return cell(columns[4], () => TextBlock(FileSize.ToString("N1") + " KB"));
        }
    }

    public class DetailsListSampleItemWithComponents : IDetailsListItem<DetailsListSampleItemWithComponents>
    {
        public UIcons Icon { get; private set; }
        public CheckBox CheckBox { get; private set; }
        public string Name { get; private set; }
        public Button Button { get; private set; }
        public ChoiceGroup ChoiceGroup { get; private set; }
        public bool EnableOnListItemClickEvent => false;
        public void OnListItemClick(int index) {}
        public int CompareTo(DetailsListSampleItemWithComponents other, string key) => 0;
        public DetailsListSampleItemWithComponents WithIcon(UIcons icon) { Icon = icon; return this; }
        public DetailsListSampleItemWithComponents WithCheckBox(CheckBox cb) { CheckBox = cb; return this; }
        public DetailsListSampleItemWithComponents WithName(string name) { Name = name; return this; }
        public DetailsListSampleItemWithComponents WithButton(Button btn) { Button = btn; return this; }
        public DetailsListSampleItemWithComponents WithChoiceGroup(ChoiceGroup cg) { ChoiceGroup = cg; return this; }
        public IEnumerable<IComponent> Render(IList<IDetailsListColumn> columns, Func<IDetailsListColumn, Func<IComponent>, IComponent> cell)
        {
            yield return cell(columns[0], () => Icon(Icon));
            yield return cell(columns[1], () => CheckBox);
            yield return cell(columns[2], () => TextBlock(Name));
            yield return cell(columns[3], () => Button);
            yield return cell(columns[4], () => ChoiceGroup);
        }
    }
}
