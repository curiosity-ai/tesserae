# ChatTimeDivider

A quiet divider across a chat transcript: a wavy hairline either side of the time the messages below it were
written. A transcript puts one where the conversation picked up again after a pause.

## Factory

```csharp
public static ChatTimeDivider ChatTimeDivider(DateTimeOffset time, string label = null)
```

`label` replaces the formatted time ("Conversation resumed").

## Where to put one

```csharp
// true when the pause is longer than the gap (an hour when none is given), or the two fall on different days
public static bool IsGap(DateTimeOffset previous, DateTimeOffset next, TimeSpan? gap = null)
public static readonly TimeSpan DefaultGap;   // 1 hour
```

```csharp
for (int i = 0; i < messages.Length; i++)
{
    if (i == 0 || ChatTimeDivider.IsGap(messages[i - 1].Time, messages[i].Time, TimeSpan.FromHours(1)))
    {
        transcript.Add(ChatTimeDivider(messages[i].Time));
    }

    transcript.Add(RenderMessage(messages[i]));
}
```

A gap of `TimeSpan.Zero` (or less) never draws one.

## The text

Formatted in the browser's locale (12- or 24-hour as the user's system has it), relative to today:

| When | Text |
|---|---|
| today | `6:07 PM` |
| yesterday | `Yesterday · 4:20 PM` |
| within the last week | `Tue · 2:30 PM` |
| earlier this year | `Nov 3 · 9:12 AM` |
| another year | `Nov 7, 2025 · 10:05 AM` |

`ChatTimeDivider.Format(time)` returns that text on its own. The divider's `title` (hover) is the full date and time.

## Methods

- `SetTime(DateTimeOffset time, string label = null)` - change a divider already on screen.
- `Time` - the moment it stands for.
- Margin / padding helpers (`IHasMarginPadding`).

It is `role="separator"` with the same text as its `aria-label`. The wave is a CSS mask over
`--tss-default-border-color`, so it follows the theme and dark mode; under `forced-colors` it is a plain rule.

## Related

- [chat](chat.md) - the transcript it sits in.
- [conversation-list](conversation-list.md) - the history beside it.
