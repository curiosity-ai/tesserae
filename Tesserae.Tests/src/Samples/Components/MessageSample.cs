using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.Comment)]
    public class MessageSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public MessageSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(MessageSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("The Message component is used to display static messages, alerts, or empty states. It supports an icon, title, text body, and an optional note area."),
                    TextBlock("It comes with variants for standard, success, warning, and error states.")
               ))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Standard Message (with Note)"),
                    Message("No Database Schema Yet", "Start by describing your database requirements in the chat. I'll help you design a complete schema with tables, relationships, and best practices.")
                        .Icon(UIcons.FileCode)
                        .Note(
                            HStack().AlignItems(ItemAlign.Center).Children(
                                Icon(UIcons.Bulb, size: TextSize.Small).PR(8),
                                TextBlock("Try saying: \"Create a blog database with users, posts, and comments\"").SemiBold()
                            )
                        ),

                    SampleSubTitle("Error Message"),
                    Message("Something went wrong", "We couldn't save your changes. Please check your internet connection and try again.")
                        .Icon(UIcons.CrossCircle)
                        .Variant(MessageVariant.Error),

                    SampleSubTitle("No Results"),
                    Message("No results found", "We couldn't find any items matching your search criteria.")
                        .Icon(UIcons.Search)
                        .Variant(MessageVariant.Default)
               ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
