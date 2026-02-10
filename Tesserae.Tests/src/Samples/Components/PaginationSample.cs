using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 30, Icon = UIcons.List)]
    public class PaginationSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public PaginationSample()
        {
            var status = TextBlock("Showing page 1").Medium();

            _content = SectionStack()
               .Title(SampleHeader(nameof(PaginationSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Pagination allows users to navigate through a large set of data by breaking it into smaller, manageable chunks called pages."),
                    TextBlock("It provides controls to move between pages, jump to specific pages, and see the current position within the total set.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use pagination when you have a large amount of content that would be overwhelming or slow to load all at once. Clearly show the total number of items and the current page. Provide 'Previous' and 'Next' controls for sequential navigation. If the number of pages is high, consider using a simplified view or allowing the user to jump to the first/last page. Keep the pagination controls in a consistent location, typically at the bottom of the content area.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic Pagination"),
                    Card(status).MB(16),
                    Pagination(totalItems: 120, pageSize: 10, currentPage: 1)
                       .OnPageChange(p => status.Text = $"Showing page {p.CurrentPage}"),
                    SampleSubTitle("Small Result Set"),
                    Pagination(totalItems: 25, pageSize: 10, currentPage: 1)
                       .OnPageChange(p => Toast().Information($"Selected page {p.CurrentPage}")),
                    SampleSubTitle("Large Result Set"),
                    Pagination(totalItems: 1000, pageSize: 20, currentPage: 5)
                       .OnPageChange(p => Toast().Information($"Selected page {p.CurrentPage}"))
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
