---
name: notification-center
description: A bell button with an unread-count badge that opens a panel of recent notifications grouped by date. Use when adding an in-app notification inbox to a Tesserae (C#/h5) app.
---

# NotificationCenter

A bell icon with an unread badge; clicking it opens a side `Panel` listing notifications grouped by Today / Yesterday / Earlier, with tone-coded dots, read/unread state, and a "Mark all read" action. Items load asynchronously each time the panel opens.

## Create

`UI.NotificationCenter()` — returns a `NotificationCenter`. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.LoadItems(Func<Task<NotificationItem[]>>)` — async loader called when the panel opens.
- `.BadgeCount(IObservable<int>)` — bind the unread badge to an observable.
- `.SetBadgeCount(int)` — set the badge directly.
- `.OnMarkRead(Action<string>)` — fires with an item `Id` when marked read.
- `.OnClearAll(Action)` — fires on clear-all.

`NotificationCenter.NotificationItem`: `Id`, `Title`, `Message`, `Timestamp` (`DateTime`), `Tone` (`NotificationTone.Info`/`Success`/`Warning`/`Danger`), `IsRead`.

## Example

```csharp
using static Tesserae.UI;

var unread = new SettableObservable<int>(3);

var center = NotificationCenter()
    .LoadItems(async () =>
    {
        await Task.Delay(300);
        return new[]
        {
            new NotificationCenter.NotificationItem
            {
                Id = "1", Title = "Deployed", Message = "v2.4.1 is live.",
                Timestamp = DateTime.Now.AddMinutes(-5),
                Tone = NotificationCenter.NotificationTone.Success, IsRead = false
            }
        };
    })
    .BadgeCount(unread)
    .OnMarkRead(id => { if (unread.Value > 0) unread.Value--; });
```

## Related

- Toast (transient notifications) — `../toast/SKILL.md`
- Panel — `/tesserae/surfaces/panel`
- Full docs & API: `/tesserae/utilities/notification-center`
