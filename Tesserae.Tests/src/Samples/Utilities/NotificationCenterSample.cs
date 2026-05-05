using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 12, Icon = UIcons.Bell)]
    public class NotificationCenterSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public NotificationCenterSample()
        {
            var unreadCount = new SettableObservable<int>(3);

            var center = NotificationCenter()
                .LoadItems(async () =>
                {
                    await Task.Delay(500); // simulate network delay
                    return new[]
                    {
                        new NotificationCenter.NotificationItem
                        {
                            Id        = "1",
                            Title     = "Deployment completed",
                            Message   = "Production release v2.4.1 was deployed successfully.",
                            Timestamp = DateTime.Now.AddMinutes(-5),
                            Tone      = NotificationCenter.NotificationTone.Success,
                            IsRead    = false
                        },
                        new NotificationCenter.NotificationItem
                        {
                            Id        = "2",
                            Title     = "High memory usage",
                            Message   = "Server eu-west-1 is at 92% memory. Consider scaling.",
                            Timestamp = DateTime.Now.AddMinutes(-38),
                            Tone      = NotificationCenter.NotificationTone.Warning,
                            IsRead    = false
                        },
                        new NotificationCenter.NotificationItem
                        {
                            Id        = "3",
                            Title     = "New team member",
                            Message   = "Alice joined the Engineering team.",
                            Timestamp = DateTime.Now.AddHours(-2),
                            Tone      = NotificationCenter.NotificationTone.Info,
                            IsRead    = false
                        },
                        new NotificationCenter.NotificationItem
                        {
                            Id        = "4",
                            Title     = "Backup failed",
                            Message   = "Nightly backup for db-prod failed. Check logs.",
                            Timestamp = DateTime.Now.AddDays(-1).AddHours(-3),
                            Tone      = NotificationCenter.NotificationTone.Danger,
                            IsRead    = true
                        },
                        new NotificationCenter.NotificationItem
                        {
                            Id        = "5",
                            Title     = "Report ready",
                            Message   = "Monthly usage report for April is ready to download.",
                            Timestamp = DateTime.Now.AddDays(-2),
                            Tone      = NotificationCenter.NotificationTone.Info,
                            IsRead    = true
                        }
                    };
                })
                .BadgeCount(unreadCount)
                .OnMarkRead(id =>
                {
                    var current = unreadCount.Value;
                    if (current > 0) unreadCount.Value = current - 1;
                });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(NotificationCenterSample), UIcons.Bell, "A bell button that opens a panel of recent notifications")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("NotificationCenter provides a bell icon with an unread count badge. Clicking it opens a side panel listing recent notifications grouped by date (Today, Yesterday, Earlier), with read/unread state, tone-coded dots, and a mark-all-read action."),
                    TextBlock("Notifications are loaded asynchronously via the LoadItems callback, so the panel always shows the most recent state."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Keep notification messages concise — title + one sentence. Use the Tone to convey severity (Info, Success, Warning, Danger). Decrement the badge count as the user reads individual notifications. Use OnClearAll to reset the store when the user clears everything."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Live Demo"),
                    TextBlock("Click the bell icon to open the notification panel.").Small().MB(12),
                    HStack().AlignItems(ItemAlign.Center).Gap(16).Children(
                        center,
                        TextBlock("← Click the bell").Small().Foreground(Theme.Secondary.Foreground)
                    ),
                    SampleSubTitle("Badge Control"),
                    HStack().AlignItems(ItemAlign.Center).Gap(8).Children(
                        Button("Add notification").OnClick(() => unreadCount.Value++),
                        Button("Clear all").OnClick(() => unreadCount.Value = 0),
                        DeferSync(unreadCount, v => TextBlock($"Unread: {v}").Small().ML(8))
                    )
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
