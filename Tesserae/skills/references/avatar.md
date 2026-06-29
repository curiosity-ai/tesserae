---
name: avatar
description: A circular user representation showing an image or initials with an optional presence dot; Persona pairs it with name/role text. Use when representing users, teams, or contacts in a Tesserae (C#/h5) app.
---

# Avatar / Persona

`Avatar` shows a user's image, or initials with a deterministic gradient background when
no image is set. It supports five sizes and a presence indicator. `Persona` wraps an
`Avatar` with name plus two lines of descriptive text — ideal for contact lists and
profile cards.

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

- Full docs & API: `/tesserae/components/avatar`
- Used by Chat messages — `chat.md`
