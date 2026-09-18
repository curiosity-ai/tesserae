using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Navigation, Order = 120, Icon = UIcons.Duplicate, Description = "Page through a set of results")]
    public class PaginationSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public PaginationSample()
        {
            var status = TextBlock("Showing page 1").Medium();

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(PaginationSample), UIcons.AngleRight, "A component to navigate through pages")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Pagination allows users to navigate through a large set of data by breaking it into smaller, manageable chunks called pages."),
                    TextBlock("It provides controls to move between pages, jump to specific pages, and see the current position within the total set."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use pagination when you have a large amount of content that would be overwhelming or slow to load all at once. Clearly show the total number of items and the current page. Provide 'Previous' and 'Next' controls for sequential navigation. If the number of pages is high, consider using a simplified view or allowing the user to jump to the first/last page. Keep the pagination controls in a consistent location, typically at the bottom of the content area."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic Pagination"),
                    Card(status).MB(16),
                    Pagination(totalItems: 120, pageSize: 10, currentPage: 1)
                       .OnPageChange(p => status.Text = $"Showing page {p.CurrentPage}"),
                    SampleSubTitle("Small Result Set"),
                    Pagination(totalItems: 25, pageSize: 10, currentPage: 1)
                       .OnPageChange(p => Toast().Information($"Selected page {p.CurrentPage}")),
                    SampleSubTitle("Large Result Set"),
                    TextBlock("With WithFirstLastButtons(), which adds the jump-to-end chevrons the strip leaves out by default.").Small().Secondary(),
                    Pagination(totalItems: 1000, pageSize: 20, currentPage: 5)
                       .WithFirstLastButtons()
                       .OnPageChange(p => Toast().Information($"Selected page {p.CurrentPage}")),
                    SampleSubTitle("Fits On One Page"),
                    TextBlock("Nothing renders between here and the next heading: 8 items at a page size of 25 is one page, and a lone '1' button says only that there is nothing to navigate.").Small().Secondary(),
                    Pagination(totalItems: 8, pageSize: 25),
                    SampleSubTitle("One Page, Holding Its Place"),
                    TextBlock("The same set with AlwaysVisible(), for a footer whose height must not change as the list is filtered.").Small().Secondary(),
                    Pagination(totalItems: 8, pageSize: 25).AlwaysVisible()
                )).SetTitle("Usage")))
               .SeeAlso(typeof(DetailsListSample), typeof(VirtualizedListSample), typeof(InfiniteScrollingListSample), typeof(ItemsListSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
