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
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(MessageSample), UIcons.Envelope, "A component to display a message")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("The Message component is used to display static messages, alerts, or empty states. It supports an icon, title, text body, and an optional note area."),
                    TextBlock("It comes with variants for standard, info, success, warning, and error states, and can be laid out vertically (default) or horizontally.")
               )).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
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
               )).SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Variants apply a soft tinted background, matching icon, and a rounded border using the TSS color variables."),

                    SampleSubTitle("Info"),
                    Message("Heads up", "Your trial expires in 7 days. Upgrade now to keep access to all features.")
                        .Icon(UIcons.Info)
                        .Variant(MessageVariant.Info),

                    SampleSubTitle("Success"),
                    Message("All set!", "Your changes have been saved successfully.")
                        .Icon(UIcons.CheckCircle)
                        .Variant(MessageVariant.Success),

                    SampleSubTitle("Warning"),
                    Message("Storage almost full", "You're using 95% of your available storage. Consider removing unused files.")
                        .Icon(UIcons.TriangleWarning)
                        .Variant(MessageVariant.Warning)
               )).SetTitle("Variants")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use Horizontal() to place the icon beside the content. This works great for compact inline alerts."),

                    SampleSubTitle("Horizontal Info"),
                    Message("Sync complete", "Your workspace is up to date with the latest changes from the server.")
                        .Icon(UIcons.Info)
                        .Variant(MessageVariant.Info)
                        .Horizontal(),

                    SampleSubTitle("Horizontal Success"),
                    Message("Payment received", "We've received your payment and your subscription is now active.")
                        .Icon(UIcons.CheckCircle)
                        .Variant(MessageVariant.Success)
                        .Horizontal(),

                    SampleSubTitle("Horizontal Warning (with Note)"),
                    Message("Unsaved changes", "You have unsaved changes that will be lost if you leave this page.")
                        .Icon(UIcons.TriangleWarning)
                        .Variant(MessageVariant.Warning)
                        .Horizontal()
                        .Note(
                            HStack().AlignItems(ItemAlign.Center).Children(
                                Icon(UIcons.Bulb, size: TextSize.Small).PR(8),
                                TextBlock("Tip: changes are saved automatically every few minutes.").SemiBold()
                            )
                        )
               )).SetTitle("Horizontal layout")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use Action(...) to attach an action component. On the default layout it renders below the content; on a horizontal message it renders on the right side, beside the content."),

                    SampleSubTitle("Action (vertical)"),
                    Message("No Database Schema Yet", "Start by describing your database requirements in the chat.")
                        .Icon(UIcons.FileCode)
                        .Action(Button("Get started").Primary()),

                    SampleSubTitle("Action (horizontal)"),
                    Message("Update available", "A new version of the app is ready to install.")
                        .Icon(UIcons.Info)
                        .Variant(MessageVariant.Info)
                        .Horizontal()
                        .Action(Button("Update now").Primary())
               )).SetTitle("Action")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
