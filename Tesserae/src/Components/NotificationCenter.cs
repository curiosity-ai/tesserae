using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H5;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A notification bell button that opens a panel listing recent notifications with read/unread state.
    /// </summary>
    [H5.Name("tss.NotificationCenter")]
    public sealed class NotificationCenter : IComponent
    {

        /// <summary>
        /// Specifies the available notification styles.
        /// </summary>
        [Enum(Emit.StringName)] //Don't change the emit type without updating the FromClassList method
        [H5.Name("tss.NCNT")]

        public enum NotificationTone 
        { 
            [Name("tss-notif-info")]       Info, 
            [Name("tss-notif-success")]       Success, 
            [Name("tss-notif-warning")]       Warning, 
            [Name("tss-notif-danger")]       Danger 
        }

        public class NotificationItem
        {
            /// <summary>
            /// Sets the DOM id of the component.
            /// </summary>
            public string           Id        { get; set; }
            /// <summary>
            /// Gets or sets the title of the component.
            /// </summary>
            public string           Title     { get; set; }
            /// <summary>
            /// Gets or sets the message.
            /// </summary>
            public string           Message   { get; set; }
            /// <summary>
            /// Gets or sets the timestamp.
            /// </summary>
            public DateTime         Timestamp { get; set; }
            /// <summary>
            /// Gets or sets the tone.
            /// </summary>
            public NotificationTone Tone      { get; set; }
            /// <summary>
            /// Returns a value indicating whether the component is read.
            /// </summary>
            public bool             IsRead    { get; set; }
        }

        private readonly HTMLElement               _container;
        private readonly HTMLElement               _bellButton;
        private readonly HTMLElement               _badgeSpan;
        private          Func<Task<NotificationItem[]>> _loadItems;
        private          Action<string>            _onMarkRead;
        private          Action                    _onClearAll;
        private          IObservable<int>          _badgeCount;
        private          Panel                     _panel;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public NotificationCenter()
        {
            _badgeSpan = Span(_("tss-notif-badge"));
            _badgeSpan.style.display = "none";

            _bellButton = Div(_("tss-notif-bell"), I(UIcons.Bell), _badgeSpan);
            _bellButton.setAttribute("role",       "button");
            _bellButton.setAttribute("aria-label", "Notifications");
            _bellButton.setAttribute("tabindex",   "0");
            _bellButton.addEventListener("click",   _ => OpenPanel());
            _bellButton.addEventListener("keydown", e =>
            {
                var kb = e.As<KeyboardEvent>();
                if (kb.key == "Enter" || kb.key == " ") { OpenPanel(); e.preventDefault(); }
            });

            _container = Div(_("tss-notif-center"), _bellButton);
        }

        /// <summary>Sets the async function that loads notification items.</summary>
        public NotificationCenter LoadItems(Func<Task<NotificationItem[]>> loader)
        {
            _loadItems = loader;
            return this;
        }

        /// <summary>Registers a callback for when a single notification is marked read.</summary>
        public NotificationCenter OnMarkRead(Action<string> onMarkRead)
        {
            _onMarkRead += onMarkRead;
            return this;
        }

        /// <summary>Registers a callback for when all notifications are cleared.</summary>
        public NotificationCenter OnClearAll(Action onClearAll)
        {
            _onClearAll += onClearAll;
            return this;
        }

        /// <summary>Binds an observable integer to drive the badge count.</summary>
        public NotificationCenter BadgeCount(IObservable<int> count)
        {
            _badgeCount = count;
            _badgeCount.Observe(v => UpdateBadge(v));
            return this;
        }

        /// <summary>Sets the badge count directly.</summary>
        public NotificationCenter SetBadgeCount(int count)
        {
            UpdateBadge(count);
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _container;

        private void UpdateBadge(int count)
        {
            if (count > 0)
            {
                _badgeSpan.innerText       = count > 99 ? "99+" : count.ToString();
                _badgeSpan.style.display   = "block";
                _bellButton.setAttribute("aria-label", $"Notifications ({count} unread)");
            }
            else
            {
                _badgeSpan.style.display = "none";
                _bellButton.setAttribute("aria-label", "Notifications");
            }
        }

        private void OpenPanel()
        {
            if (_panel != null)
            {
                _panel.Show();
                return;
            }

            // Single root stack that we mutate after loading
            var rootStack = VStack().WS().HS().Gap(8.px())
               .Children(VStack().S().AlignItems(ItemAlign.Center).JustifyContent(ItemJustify.Center).Children(Spinner()));

            _panel = Panel("Notifications")
               .Near()
               .Small()
               .LightDismiss()
               .Content(rootStack)
               .OnHide(_ => { _panel = null; });

            _panel.Show();

            if (_loadItems != null)
            {
                Task.Run(async () =>
                {
                    var items = await _loadItems();

                    ClearChildren(rootStack.Render());

                    if (items == null || items.Length == 0)
                    {
                        rootStack.Add(VStack().S().AlignItems(ItemAlign.Center).JustifyContent(ItemJustify.Center).Children(
                            Icon(UIcons.Bell,size: TextSize.XLarge).Foreground(Theme.Secondary.Foreground),
                            TextBlock("No notifications").Medium().MT(16)
                        ));
                    }
                    else
                    {
                        var listContainer = VStack().WS().Gap(8.px());
                        RenderItems(items, listContainer);

                        var toolbar = HStack().WS().AlignItems(ItemAlign.Center).Children(
                            TextBlock($"{items.Length} notification{(items.Length == 1 ? "" : "s")}").Small().Foreground(Theme.Secondary.Foreground).Grow(),
                            Button("Mark all read").Link().Small().OnClick(() =>
                            {
                                foreach (var item in items) _onMarkRead?.Invoke(item.Id);
                                _panel?.Hide();
                            })
                        );
                        rootStack.Add(toolbar);
                        rootStack.Add(listContainer);
                    }
                }).FireAndForget();
            }
        }

        private void RenderItems(NotificationItem[] items, Stack container)
        {
            if (items == null) return;

            var grouped = GroupByDate(items);

            foreach (var (dateLabel, group) in grouped)
            {
                container.Add(TextBlock(dateLabel).SemiBold().Small().Foreground(Theme.Secondary.Foreground));

                foreach (var item in group)
                {
                    container.Add(CreateItemCard(item));
                }
            }
        }

        private IComponent CreateItemCard(NotificationItem item)
        {
            var dot       = Span(_($"tss-notif-dot {item.Tone}"));
            dot.style.display = item.IsRead ? "none" : "inline-block";

            var title     = TextBlock(item.Title ?? string.Empty).SemiBold().Small();
            var message   = TextBlock(item.Message ?? string.Empty).Small().Foreground(Theme.Secondary.Foreground);
            var timeText  = TextBlock(FormatTime(item.Timestamp)).XSmall().Foreground(Theme.Secondary.Foreground);

            var card = Card(
                VStack().WS().Gap(4.px()).Children(
                HStack().WS().AlignItems(ItemAlign.Center).Children(Raw(dot), title.ML(item.IsRead ? 0 : 6).Grow(), timeText),
                message
            ), noAnimation: true);

            if (!item.IsRead)
            {
                card.Render().classList.add("tss-notif-unread");
                card.OnClick(() =>
                {
                    item.IsRead = true;
                    dot.style.display = "none";
                    card.Render().classList.remove("tss-notif-unread");
                    _onMarkRead?.Invoke(item.Id);
                });
            }

            return card;
        }

        private static (string, NotificationItem[])[] GroupByDate(NotificationItem[] items)
        {
            var today     = DateTime.Today;
            var yesterday = today.AddDays(-1);

            var groups = new List<(string, List<NotificationItem>)>
            {
                ("Today",     new List<NotificationItem>()),
                ("Yesterday", new List<NotificationItem>()),
                ("Earlier",   new List<NotificationItem>())
            };

            foreach (var item in items.OrderByDescending(i => i.Timestamp))
            {
                if (item.Timestamp.Date == today)
                    groups[0].Item2.Add(item);
                else if (item.Timestamp.Date == yesterday)
                    groups[1].Item2.Add(item);
                else
                    groups[2].Item2.Add(item);
            }

            return groups
               .Where(g => g.Item2.Count > 0)
               .Select(g => (g.Item1, g.Item2.ToArray()))
               .ToArray();
        }

        private static string FormatTime(DateTime dt)
        {
            var diff = DateTime.Now - dt;
            if (diff.TotalMinutes < 1)  return "just now";
            if (diff.TotalHours   < 1)  return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalDays    < 1)  return $"{(int)diff.TotalHours}h ago";
            return dt.ToString("MMM d");
        }
    }
}
