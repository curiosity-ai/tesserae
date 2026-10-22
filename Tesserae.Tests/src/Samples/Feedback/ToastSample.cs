using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Feedback, Order = 30, Icon = UIcons.BreadSlice, Description = "Transient notifications in a screen corner")]
    public class ToastSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ToastSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ToastSample), UIcons.Exclamation, "A utility to display toast notifications")
               .FlatSection(Stack().WidthStretch().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Toasts are short-lived, non-intrusive notifications that provide feedback about an operation. They appear temporarily on the screen and then disappear automatically, making them ideal for success messages, warnings, or simple information updates."),
                    TextBlock("What a toast shows is a Banner: the same notice strip that renders inline anywhere. The Information / Success / Warning / Error helpers build one for you (Primary, Success, Warning and Danger respectively); Show(banner) takes one you built yourself, with its own icon, badge and action. Either way the banner's dismiss button is hooked to the toast's own hiding.").PT(8))).SetTitle("Overview")))
               .FlatSection(Stack().WidthStretch().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use Toasts for brief, informative messages that don't require user action. Keep the text short and recognizable. Ensure the Toast duration is long enough to be read but short enough not to become an annoyance. Avoid overloading the user with too many simultaneous Toasts. For critical errors that require immediate attention or user interaction, use a Dialog or Modal instead."))).SetTitle("Best Practices")))
               .FlatSection(
                    Stack().WidthStretch().Children(
                        Card(VStack().WS().Children(
                        SampleSubTitle("Toasts top-right (default)"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().Information("Info!")),
                            Button().SetText("Success").OnClick(() => Toast().Success("Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().Warning("Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().Error("Error!"))),
                        SampleSubTitle("A message with nothing to break at"),
                        HStack().Children(
                            Button().SetText("Long path").OnClick(() => Toast().Success("Endpoint updated", "POST to /api/endpoints/run/generation/supersmartassistant/generate-all-the-things/version1"))),
                        SampleSubTitle("Toasts top left"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().TopLeft().Information("Info!")),
                            Button().SetText("Success").OnClick(() => Toast().TopLeft().Success("Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().TopLeft().Warning("Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().TopLeft().Error("Error!"))),
                        SampleSubTitle("Toasts bottom right"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().BottomRight().Information("Info!")),
                            Button().SetText("Success").OnClick(() => Toast().BottomRight().Success("Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().BottomRight().Warning("Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().BottomRight().Error("Error!"))),
                        SampleSubTitle("Toasts bottom left"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().BottomLeft().Information("Info!")),
                            Button().SetText("Success").OnClick(() => Toast().BottomLeft().Success("Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().BottomLeft().Warning("Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().BottomLeft().Error("Error!"))),
                        SampleSubTitle("Toasts top center with title"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().TopCenter().Information("This is a title", "Info!")),
                            Button().SetText("Success").OnClick(() => Toast().TopCenter().Success("This is a title", "Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().TopCenter().Warning("This is a title", "Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().TopCenter().Error("This is a title", "Error!"))),
                        SampleSubTitle("Toasts top full with title"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().TopFull().Information("This is a title", "Info!")),
                            Button().SetText("Success").OnClick(() => Toast().TopFull().Success("This is a title", "Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().TopFull().Warning("This is a title", "Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().TopFull().Error("This is a title", "Error!"))),
                        SampleSubTitle("Toasts bottom center with title"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().BottomCenter().Information("This is a title", "Info!")),
                            Button().SetText("Success").OnClick(() => Toast().BottomCenter().Success("This is a title", "Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().BottomCenter().Warning("This is a title", "Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().BottomCenter().Error("This is a title", "Error!"))),
                        SampleSubTitle("Toasts bottom full with title"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().BottomFull().Information("This is a title", "Info!")),
                            Button().SetText("Success").OnClick(() => Toast().BottomFull().Success("This is a title", "Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().BottomFull().Warning("This is a title", "Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().BottomFull().Error("This is a title", "Error!"))),
                        SampleSubTitle("Toast as banner"),
                        HStack().Children(
                            Button().SetText("Info on top").OnClick(() => Toast().TopFull().Banner().Information("This is a banner", "Info!")),
                            Button().SetText("Success on top").OnClick(() => Toast().TopFull().Banner().Success("This is a banner", "Success!")),
                            Button().SetText("Warning on bottom").OnClick(() => Toast().BottomFull().Banner().Warning("This is a banner", "Warning!")),
                            Button().SetText("Error on bottom").OnClick(() => Toast().BottomFull().Banner().Error("This is a banner", "Error!")))
                    )).SetTitle("Usage")))
               .FlatSection(
                    Stack().WidthStretch().Children(
                        Card(VStack().WS().Children(
                        TextBlock("Toast().Show(banner) floats a Banner you built yourself. Everything it carries comes along — its tone, its icon tile, its badge and its action — and the [x] closes the toast, chained after whatever OnDismiss handler you set. NoDismiss() takes the [x] away, since such a toast cannot be dismissed at all."),
                        SampleSubTitle("A banner of your own"),
                        HStack().Wrap().Gap(8.px()).Children(
                            Button().SetText("With an action").OnClick(() => Toast().Show(
                                Banner("Export finished", "18 documents, 42 MB.").Success().SetIcon(UIcons.Download).Action("Download", () => Toast().Information("Downloading")))),
                            Button().SetText("With a badge").OnClick(() => Toast().Show(
                                Banner("3 items need your review", "They were flagged as high priority.").Danger().SetIcon(UIcons.Flame).SetBadge("Priority"))),
                            Button().SetText("Letters on the tile").OnClick(() => Toast().Show(
                                Banner("Report ready", "Q3-line-review.pdf, 2.4 MB.").Primary().SetIcon("PDF"))),
                            Button().SetText("No icon").OnClick(() => Toast().Show(
                                Banner("Read-only mode", "You are looking at a snapshot from 12 Apr 2024.").Secondary().NoIcon())),
                            Button().SetText("No dismiss").OnClick(() => Toast().NoDismiss().Show(
                                Banner("Indexing", "This one has no [x].").Primary()))),
                        SampleSubTitle("As a page banner"),
                        HStack().Wrap().Gap(8.px()).Children(
                            Button().SetText("Top, with an action").OnClick(() => Toast().TopFull().Banner().Duration(TimeSpan.FromSeconds(15)).Show(
                                Banner("3 items need your review", "They were flagged as high priority and are waiting in your queue — the oldest has been there for two days.")
                                   .Danger().SetIcon(UIcons.Flame).SetBadge("Priority").Flat()
                                   .Action("Review now", () => Toast().Success("Opening the queue")))),
                            Button().SetText("Bottom, no hide button").OnClick(() => Toast().BottomFull().Banner(showHideButton: false).Show(
                                Banner("Scheduled maintenance tonight, 23:00 – 01:00 UTC", "Search stays available; indexing is paused for the window.").Primary().Flat())))
                    )).SetTitle("Showing a Banner", UIcons.Megaphone, Theme.Colors.Purple600)))
               .FlatSection(
                    Stack().WidthStretch().Children(
                        Card(VStack().WS().Children(
                        TextBlock("Time(text) puts a short piece of text at the top right of the toast, level with the title: when the notice was raised. Time(moment, text) does the same as a <time> element carrying the moment, and shows HH:mm when no text is given. It is absent unless asked for, and it is carried onto the banner whichever way that was made."),
                        SampleSubTitle("A toast with a time"),
                        HStack().Wrap().Gap(8.px()).Children(
                            Button().SetText("Info").OnClick(() => Toast().Time("Just now").Information("Sync started", "Your workspace is being indexed.")),
                            Button().SetText("Success").OnClick(() => Toast().Time(SampleDate.Now).Success("Backup complete", "Archived 1,204 files.")),
                            Button().SetText("Warning").OnClick(() => Toast().Time("2 min ago").Warning("Storage almost full", "92% of the quota is used.")),
                            Button().SetText("Error").OnClick(() => Toast().Time(SampleDate.Now, "09:41 UTC").Error("Upload failed", "The file is larger than the 2 GB limit.")),
                            Button().SetText("A banner of your own").OnClick(() => Toast().Time("1 h ago").Show(
                                Banner("Export finished", "18 documents, 42 MB.").Success().SetIcon(UIcons.Download).Action("Download", () => Toast().Information("Downloading")))))
                    )).SetTitle("Showing the time", UIcons.Clock, Theme.Colors.Purple600)))
               .FlatSection(
                    Stack().WidthStretch().Children(
                        Card(VStack().WS().Children(
                        TextBlock("Countdown() draws a bar along the foot of the toast that drains over its Duration, so the time left to act on it can be seen - the undo window of a destructive action. The bar refills while the pointer is over the toast (the toast waits then) and starts over when it leaves. Call it before the method that shows the toast."),
                        SampleSubTitle("Undo with a countdown"),
                        HStack().Wrap().Gap(8.px()).Children(
                            Button().SetText("Archive conversation").SetIcon(UIcons.Archive).OnClick(() =>
                                Toast().BottomCenter().Countdown().Duration(TimeSpan.FromSeconds(6)).Show(
                                    Banner("Conversation archived", "It moved to the Archive folder.").Secondary().SetIcon(UIcons.Archive)
                                       .Action("Undo", () => Toast().Success("Restored")))),
                            Button().SetText("Error, 4 s").OnClick(() =>
                                Toast().Countdown().Duration(TimeSpan.FromSeconds(4)).Error("Sync failed", "Retrying when the bar runs out.")))
                    )).SetTitle("Showing the time left", UIcons.Hourglass, Theme.Colors.Purple600)))
               .SeeAlso(typeof(BannerSample), typeof(NotificationCenterSample), typeof(SavingToastSample), typeof(MessageSample), typeof(TippySample));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}