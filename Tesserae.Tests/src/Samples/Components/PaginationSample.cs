using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 14, Icon = UIcons.List)]
    public class PaginationSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public PaginationSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(PaginationSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Pagination is used to navigate through multiple pages of content.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Pagination(totalItems: 100, itemsPerPage: 10, currentPage: 1)
                        .Do(p => p.PageChanged += (s, page) => Toast().Information($"Navigated to page {page}"))
                ));
        }

        public H5.Core.dom.HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
