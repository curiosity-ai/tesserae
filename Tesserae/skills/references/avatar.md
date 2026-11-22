---
name: avatar
description: A circular user representation showing an image or initials with an optional presence dot; Persona pairs it with name/role text. Use when representing users, teams, or contacts in a Tesserae (C#/Transpose) app.
---

# Avatar / Persona

`Avatar` shows a user's image, or initials with a deterministic gradient background when
no image is set. It supports five sizes and a presence indicator. `Persona` wraps an
`Avatar` with name plus two lines of descriptive text — ideal for contact lists and
profile cards. The initials fill is drawn by the `tss-avatar-generated` class from a
`--tss-avatar-hue` the avatar sets, so a stylesheet or custom theme can restyle it
(`custom-themes.md`); `Background(...)` still overrides it.

## Create

`UI.Avatar(string image = null, string initials = null)`.
`UI.Persona(string name = null, string secondaryText = null, string tertiaryText = null, Avatar avatar = null)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

Avatar:

- `.Size(AvatarSize)` — `XSmall`, `Small` (default), `Medium`, `Large`, `XLarge`.
- `.Presence(AvatarPresence)` — `None`, `Online`, `Away`, `Busy`, `Offline`.
- `.SetImage(string)` / `.SetInitials(string)` — content.
- `.Background(string)` / `.Foreground(string)` — override colors.
- `.Compact(bool = true)` — a 20px bubble with a presence dot in proportion, for an avatar inside a
  line of text, a list row or a chip. Wins over `.Size(...)` while on; `.IsCompact` reads it back.
- `Avatar.InitialsFor(string name)` — the initials for a name: first letters of its first and last words,
  upper-cased (`"Ada Lovelace"` → `"AL"`); an e-mail address is read up to its `@`.

For a person shown as a card (a name, a second line, a remove button, a click), use
`UI.AvatarContextCard(...)` — see [context-card.md](context-card.md).

Persona:

- `.SetName(string)` / `.SetSecondaryText(string)` / `.SetTertiaryText(string)`.
- `.SetAvatar(Avatar)` — replace the avatar.
- `.Compact(bool = true)` — denser layout.

## Example

```csharp
using static Tesserae.UI;

var avatars = HStack().Children(
    Avatar(initials: "JD").Size(AvatarSize.Medium).Presence(AvatarPresence.Online),
    Avatar(image: "https://example.com/u.jpg").Size(AvatarSize.Large));

var person = Persona("Jordan Diaz", "Product Designer", "Available",
    Avatar(initials: "JD").Presence(AvatarPresence.Online));
```

## Related

- StatefulItemRow — a list row that takes an `Avatar` as its leading visual — `stateful-item-row.md`
- Full docs & API: `/tesserae/media/avatar`
- Used by Chat messages — `chat.md`
- For a decorative animated pixel-art character (not a user) — `pixel-avatar.md`
