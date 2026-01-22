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

            var pagination = Pagination(totalItems: 120, pageSize: 10, currentPage: 1)
               .OnPageChange(p => status.Text = $"Showing page {p.CurrentPage}");

            _content = SectionStack()
               .Title(SampleHeader(nameof(PaginationSample)))
               .Section(Stack().Children(
                    SampleTitle("Pagination"),
                    Card(status).PB(16),
                    pagination));
        }

        public HTMLElement Render() => _content.Render();
    }
}
