using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 31, Icon = UIcons.Cat)]
    public class PixelAvatarSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public PixelAvatarSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(PixelAvatarSample), UIcons.Cat, "An animated pixel-art avatar built out of one div per pixel")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("PixelAvatar renders a small animated sprite as a grid of absolutely positioned square divs. The artwork is stored once, as a byte grid of palette indices, and each of the twelve designs is nothing more than a palette of colors for those indices - so recoloring an avatar costs eleven CSS variable writes and no repaint of the sprite."),
                        TextBlock("Thirteen animations are available. The four *Idle animations loop forever, while the rest play once and hand over to a follow-up animation: Sit settles into SitIdle, Stretch finishes by sitting down, JumpUp is followed by JumpDown, and so on. Idle, SitIdle and CrouchIdle hold their first frame for a random 5-10 seconds rather than cycling continuously, so a resting cat looks still rather than fidgety - and AutoIdle drifts between those three poses on its own."),
                        TextBlock("Avatars can be attached to any other component, which perches them on one of its edges without affecting its layout."),
                        TextBlock("The extracted palettes are the source artwork's own colors, which means some of them are pure white and others near-black. A hairline halo in the theme's contrasting color is drawn by default so every design stays legible in both light and dark mode; Outline(false) turns it off.")))
                       .SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleDo("Use an avatar as a small piece of personality - an empty-state illustration, a companion on a primary action, a mascot in a sidebar."),
                        SampleDo("Keep the pixel size a whole number so the sprite grid stays aligned to device pixels."),
                        SampleDont("Don't animate dozens of avatars at once on a dense screen; each one repaints on its own timer."),
                        SampleDont("Don't rely on an avatar to convey information - it is decorative, and its state is not announced beyond its ARIA label.")))
                       .SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("The twelve designs, attached to buttons"),
                        TextBlock("Every avatar below is attached to the top edge of a button. Click a button to switch the animation its cat is playing. Eight designs come from the source sprite sheets; Grey, Sparkle, Lynx and Sudo are authored against the same palette indices. Sudo also carries an accent - an extra half-size pixel on each ear tip, which is not a palette index but an overlay."),
                        DesignGallery(),
                        SampleSubTitle("Every animation"),
                        TextBlock("Pick an animation to play it on a larger avatar. Non-looping animations chain into their follow-up, so the label updates on its own once they finish. The three resting poses hold their first frame for 5-10 seconds rather than cycling, and AutoIdle drifts between them."),
                        AnimationPicker(),
                        SampleSubTitle("Anchors"),
                        TextBlock("An avatar can be anchored to any edge of the component it is attached to. By default the wrapper reserves room for it, so it can never be clipped by a scrolling ancestor."),
                        AnchorGallery(),
                        SampleSubTitle("Overlap mode"),
                        TextBlock("Overlap() drops the reserved room and lets the avatar hang outside the wrapper, which keeps the target's footprint identical to the bare component - useful when the surrounding layout must not shift."),
                        OverlapGallery(),
                        SampleSubTitle("Pixel size and facing"),
                        TextBlock("The sprite is 10x8 pixels; PixelSize sets how many CSS pixels each of them takes. Facing mirrors the artwork, which is drawn facing right."),
                        SizeGallery(),
                        SampleSubTitle("As a button"),
                        TextBlock("AsButton() wraps the avatar in a Button with no background, border, padding or minimum size, so the button hugs the cat exactly instead of the usual button chrome - the avatar becomes the clickable surface via ReplaceContent. Click a cat to play an animation."),
                        AsButtonGallery(),
                        SampleSubTitle("Turning around"),
                        TextBlock("Turn() changes direction by pivoting the sprite about its vertical axis, under a perspective scaled to the avatar's own width, so it reads as the cat physically turning rather than its pixels swapping sides. Facing() does the same change instantly."),
                        TurnGallery(),
                        SampleSubTitle("Contrast halo"),
                        TextBlock("White on a light theme and black on a dark one would otherwise vanish. Compare the same two designs with the halo on and off."),
                        OutlineGallery(),
                        SampleSubTitle("Palettes"),
                        TextBlock("The colors each design maps onto palette indices 1 to 11."),
                        PaletteTable()))
                       .SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("PixelAvatarBadge dresses a cat as a round profile picture, sized with the same AvatarSize presets as the regular Avatar so the two can sit side by side. It holds SitIdle on its first frame - a badge is an identity, not an animation, and a transcript full of moving cats is unreadable - and derives its background from the coat, so it always belongs to the cat in front of it."),
                        BadgeGallery(),
                        SampleSubTitle("In a chat"),
                        TextBlock("Passing a PixelAvatar straight to ChatMessage wraps it in a badge for you."),
                        ChatGallery()))
                       .SetTitle("As a chat avatar")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Attaching an avatar to the top of an OmniBox gives it a life of its own: left alone it wanders along the top edge and plays the odd animation, typing settles it back down, and about ten seconds after your last keystroke it pads over to the text caret to watch you type. Resting and sleeping belong to the avatar's own AutoIdle, so after a minute of silence it curls up until you come back - and waking it plays a stretch and then a startle."),
                        TextBlock("Every delay is configurable (IdleDelay, RestDelay, CursorDelay, SleepAfter, WalkSpeed) and jittered on use, so nothing the cat does lands on a stopwatch. The buttons below poke the same companion the OmniBox drives, so you don't have to wait for the timers.").Tiny().Secondary(),
                        CompanionGallery()))
                       .SetTitle("As an OmniBox companion")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A modal is the one target that is not wrapped. It centers itself inside its own full-screen container and is put on screen by Show(), so a wrapper around it would simply be dropped - it lends its own box to the avatar instead, and the cat perches on the outside of the dialog, sitting on its roof rather than inside it."),
                        TextBlock("A cat on top of a modal roams the same way an OmniBox one does. There is nothing to type into, so it only wanders and plays the odd animation rather than following a caret."),
                        ModalGallery()))
                       .SetTitle("On a modal")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Every index below is one color of the sprite. Indices 1 to 3 are the artwork's highlight shade, 4 to 9 the base shade and 10 to 11 the shadow shade, which is why the single-hue designs are just three colors repeated - and why pasting three colors is enough to build a whole coat."),
                        PaletteEditor()))
                       .SetTitle("Edit the palette")));
        }

        private static IComponent DesignGallery()
        {
            var grid = Grid(4.fr(), 4.fr(), 4.fr(), 4.fr()).Gap(24.px()).RowGap(40.px());

            foreach (var design in PixelAvatarPalettes.All)
            {
                grid.Add(DesignCard(design));
            }

            return grid;
        }

        private static IComponent DesignCard(PixelAvatarDesign design)
        {
            var animations = PixelAvatarSprites.All;
            var next       = 0;

            var avatar = PixelAvatar(42, design, animations[0]).PixelSize(5);
            var button = Button($"{design}: {animations[0]}").SetIcon(UIcons.Paw).WS();

            // Clicking the button walks through the animation list. The avatar reports back through
            // OnAnimationStarted so the label also follows the automatic hand-overs (Sit -> SitIdle).
            avatar.OnAnimationStarted((_, animation) => button.SetText($"{design}: {animation}"));
            button.OnClick(() =>
            {
                next = (next + 1) % animations.Length;
                avatar.Play(animations[next]);
            });

            return avatar.AttachTo(button, PixelAvatarAnchor.TopCenter);
        }

        private static IComponent AnimationPicker()
        {
            var avatar  = PixelAvatar(42, PixelAvatarDesign.SpottedOrange).PixelSize(10);
            var current = TextBlock("Idle").SemiBold();
            var buttons = HStack().WS().Wrap().Children();

            avatar.OnAnimationStarted((_, animation) => current.Text = avatar.IsAutoIdling ? $"AutoIdle -> {animation}" : $"{animation}");

            // AutoIdle has no frames of its own, so it is not in PixelAvatarSprites.All.
            buttons.Add(Button("AutoIdle")
               .Compact()
               .Primary()
               .Tooltip("Rests, and every 5-10s either stays put or drifts to another resting pose")
               .OnClick(() => avatar.Play(PixelAvatarAnimation.AutoIdle)));

            foreach (var animation in PixelAvatarSprites.All)
            {
                var sprites = PixelAvatarSprites.Get(animation);
                var a       = animation;

                var timing = sprites.Rests
                    ? $"{sprites.Frames.Length} frame(s), {sprites.FrameDurationMs}ms each, resting {sprites.RestMinMs / 1000}-{sprites.RestMaxMs / 1000}s on the first"
                    : $"{sprites.Frames.Length} frame(s), {sprites.FrameDurationMs}ms each";

                buttons.Add(Button($"{animation}")
                   .Compact()
                   .Tooltip(timing)
                   .OnClick(() => avatar.Play(a)));
            }

            return HStack().WS().AlignItemsCenter().Children(
                VStack().Width(140.px()).AlignItemsCenter().Children(
                    avatar,
                    current.PT(12)),
                buttons.Grow());
        }

        private static IComponent AnchorGallery()
        {
            // PixelAvatarAnchor stringifies to its CSS class, so the labels are spelled out here.
            var anchors = new[]
            {
                PixelAvatarAnchor.TopLeft,
                PixelAvatarAnchor.TopCenter,
                PixelAvatarAnchor.TopRight,
                PixelAvatarAnchor.LeftCenter,
                PixelAvatarAnchor.RightCenter,
                PixelAvatarAnchor.BottomLeft,
                PixelAvatarAnchor.BottomCenter,
                PixelAvatarAnchor.BottomRight
            };

            var labels = new[] { "TopLeft", "TopCenter", "TopRight", "LeftCenter", "RightCenter", "BottomLeft", "BottomCenter", "BottomRight" };

            var grid = Grid(4.fr(), 4.fr(), 4.fr(), 4.fr()).Gap(24.px()).RowGap(48.px());

            for (var i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                var facing = anchor == PixelAvatarAnchor.LeftCenter ? PixelAvatarFacing.Right : PixelAvatarFacing.Left;

                grid.Add(PixelAvatar(42, PixelAvatarDesign.Tuxedo, PixelAvatarAnimation.SitIdle)
                   .Facing(facing)
                   .AttachTo(Button(labels[i]).WS(), anchor));
            }

            return grid;
        }

        private static IComponent OverlapGallery()
        {
            return HStack().WS().AlignItems(ItemAlign.End).PT(40).Children(
                PixelAvatar(42, PixelAvatarDesign.Black, PixelAvatarAnimation.SitIdle)
                   .AttachTo(Button("Reserved room").WS(), PixelAvatarAnchor.TopRight)
                   .Width(200.px()),
                PixelAvatar(42, PixelAvatarDesign.Tuxedo, PixelAvatarAnimation.SitIdle)
                   .AttachTo(Button("Overlapping").WS(), PixelAvatarAnchor.TopRight)
                   .Overlap()
                   .Width(200.px()).ML(24));
        }

        private static IComponent SizeGallery()
        {
            var sizes = HStack().AlignItems(ItemAlign.End).Children();

            foreach (var size in new[] { 2, 3, 4, 6, 8, 12 })
            {
                sizes.Add(VStack().AlignItemsCenter().PR(24).Children(
                    PixelAvatar(42, PixelAvatarDesign.Siamese, PixelAvatarAnimation.Move).PixelSize(size),
                    TextBlock($"{size}px").Tiny().Secondary().PT(8)));
            }

            var facing = HStack().AlignItems(ItemAlign.End).Children(
                VStack().AlignItemsCenter().PR(24).Children(
                    PixelAvatar(42, PixelAvatarDesign.Orange, PixelAvatarAnimation.Move).PixelSize(8),
                    TextBlock("Right").Tiny().Secondary().PT(8)),
                VStack().AlignItemsCenter().PR(24).Children(
                    PixelAvatar(42, PixelAvatarDesign.Orange, PixelAvatarAnimation.Move).PixelSize(8).Facing(PixelAvatarFacing.Left),
                    TextBlock("Left").Tiny().Secondary().PT(8)),
                VStack().AlignItemsCenter().PR(24).Children(
                    PixelAvatar(42, PixelAvatarDesign.Orange, PixelAvatarAnimation.Move).PixelSize(8).Speed(0.35),
                    TextBlock("Speed 0.35").Tiny().Secondary().PT(8)),
                VStack().AlignItemsCenter().PR(24).Children(
                    PixelAvatar(42, PixelAvatarDesign.Orange, PixelAvatarAnimation.Move).PixelSize(8).Speed(3),
                    TextBlock("Speed 3").Tiny().Secondary().PT(8)));

            return VStack().WS().Children(sizes, facing.PT(24));
        }

        private static IComponent AsButtonGallery()
        {
            var row = HStack().AlignItemsCenter().Children();

            foreach (var design in new[] { PixelAvatarDesign.Black, PixelAvatarDesign.Orange, PixelAvatarDesign.Tuxedo, PixelAvatarDesign.Sudo })
            {
                var avatar = PixelAvatar(42, design, PixelAvatarAnimation.SitIdle).PixelSize(8);

                row.Add(avatar.AsButton().Tooltip($"{design}: click to play Stretch").MR(16).OnClick(() => avatar.Play(PixelAvatarAnimation.Stretch)));
            }

            return row;
        }

        private static IComponent OutlineGallery()
        {
            var row = HStack().AlignItems(ItemAlign.End).Children();

            foreach (var design in new[] { PixelAvatarDesign.White, PixelAvatarDesign.SpottedGrey, PixelAvatarDesign.Black, PixelAvatarDesign.Tuxedo })
            {
                row.Add(VStack().AlignItemsCenter().PR(32).Children(
                    HStack().AlignItems(ItemAlign.End).Children(
                        PixelAvatar(42, design, PixelAvatarAnimation.SitIdle).PixelSize(6).PR(16),
                        PixelAvatar(42, design, PixelAvatarAnimation.SitIdle).PixelSize(6).Outline(false)),
                    TextBlock($"{design}: halo / flat").Tiny().Secondary().PT(8)));
            }

            return row;
        }

        private static IComponent BadgeGallery()
        {
            var row = HStack().WS().Wrap().AlignItems(ItemAlign.End).Children();

            foreach (var design in PixelAvatarPalettes.All)
            {
                row.Add(VStack().AlignItemsCenter().PR(20).PB(12).Children(
                    PixelAvatarBadge(42, design, AvatarSize.Large),
                    TextBlock($"{design}").Tiny().Secondary().PT(6)));
            }

            var sizes = HStack().AlignItems(ItemAlign.End).PT(12).Children();

            foreach (var size in new[] { AvatarSize.XSmall, AvatarSize.Small, AvatarSize.Medium, AvatarSize.Large, AvatarSize.XLarge })
            {
                var s = size;
                sizes.Add(VStack().AlignItemsCenter().PR(20).Children(
                    PixelAvatarBadge(42, PixelAvatarDesign.Lynx, s),
                    TextBlock($"{s}").Tiny().Secondary().PT(6)));
            }

            return VStack().WS().Children(row, sizes);
        }

        private static IComponent ChatGallery()
        {
            return VStack().WS().MaxWidth(620.px()).Children(
                ChatMessage(TextBlock("Has anyone seen the build logs?"), 42, PixelAvatarDesign.Tuxedo).MaxWidth(),
                ChatMessage(TextBlock("They're on the shelf. I knocked them off."), 42, PixelAvatarDesign.SpottedOrange).MaxWidth(),
                ChatMessage(TextBlock("I sat on them, actually."), 42, PixelAvatarDesign.Sparkle).MaxWidth(),
                ChatMessage(TextBlock("Classic."), PixelAvatar(42, PixelAvatarDesign.Grey)).RightAligned().MaxWidth());
        }

        private static IComponent CompanionGallery()
        {
            var omni = OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch = "Type here and the cat will settle down..."
            });

            var perched = PixelAvatar(42, PixelAvatarDesign.Orange)
               .PixelSize(4)
               .AttachTo(omni, PixelAvatarAnchor.TopLeft)
               .WS();

            var companion = perched.Companion;
            var status    = TextBlock("Idle").Tiny().Secondary();

            companion.Avatar.OnAnimationStarted((_, animation) => status.Text = $"{animation}");

            return VStack().WS().Children(
                perched,
                HStack().WS().AlignItemsCenter().PT(12).Children(
                    Button("Fidget now").Compact().OnClick(() => companion.Fidget()),
                    Button("Follow the caret").Compact().ML(8).OnClick(() => companion.FollowCursor()),
                    Button("Wake up").Compact().ML(8).OnClick(() => companion.WakeUp()),
                    Button("Sleep in 2s").Compact().ML(8).OnClick(() => companion.SleepAfter(2000)),
                    Button("Sleep after 60s").Compact().ML(8).OnClick(() => companion.SleepAfter(60000)),
                    status.PL(16)));
        }

        private static IComponent ModalGallery()
        {
            var anchors = new[] { PixelAvatarAnchor.TopLeft, PixelAvatarAnchor.TopCenter, PixelAvatarAnchor.TopRight };
            var labels  = new[] { "TopLeft", "TopCenter", "TopRight" };
            var row     = HStack().WS().Wrap().Children();

            for (var i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];

                row.Add(Button($"Open a modal ({labels[i]})").Compact().MR(8).OnClick(() =>
                    Modal("Sudo has opinions about this dialog")
                       .Content(VStack().Children(
                            TextBlock("The cat is perched on the outside of the modal's top edge, and moves with the dialog when you drag it."),
                            TextBlock("Give it a few seconds and it will wander along the header.").Tiny().Secondary().PT(8)))
                       .WithPixelAvatar(42, PixelAvatarDesign.Sudo, anchor)
                       .Width(460.px())
                       .Show()));
            }

            return row;
        }

        private static IComponent TurnGallery()
        {
            var row = HStack().WS().AlignItems(ItemAlign.End).Children();

            foreach (var duration in new[] { 200, 320, 700 })
            {
                var ms     = duration;
                var avatar = PixelAvatar(42, PixelAvatarDesign.Lynx, PixelAvatarAnimation.Move).PixelSize(9);

                row.Add(VStack().AlignItemsCenter().PR(40).Children(
                    avatar,
                    Button($"Turn ({ms}ms)").Compact().MT(12).OnClick(() => avatar.TurnAround(ms))));
            }

            var instant = PixelAvatar(42, PixelAvatarDesign.Sparkle, PixelAvatarAnimation.Move).PixelSize(9);

            row.Add(VStack().AlignItemsCenter().Children(
                instant,
                Button("Facing (instant)").Compact().MT(12).OnClick(() =>
                    instant.Facing(instant.FacingValue == PixelAvatarFacing.Right ? PixelAvatarFacing.Left : PixelAvatarFacing.Right))));

            return row;
        }

        // Shifting every color together in HSL keeps the shading relationships that make the
        // sprite read as one coat, which recoloring indices by hand does not. The deltas are
        // relative, so all-zero returns the same colors and the editor can re-apply from the
        // unshifted palette on every slider move instead of accumulating rounding drift.
        private static PixelAvatarPalette Adjust(PixelAvatarPalette palette, int hueDelta, int saturationDelta, int lightnessDelta)
        {
            if (hueDelta == 0 && saturationDelta == 0 && lightnessDelta == 0) return palette;

            var colors = new Color[palette.Colors.Length];

            for (var i = 0; i < colors.Length; i++)
            {
                var color = palette.Colors[i];

                colors[i] = Color.FromHsl(
                    color.GetHue() + hueDelta,
                    color.GetSaturation() + saturationDelta / 100f,
                    color.GetBrightness() + lightnessDelta / 100f);
            }

            return new PixelAvatarPalette(palette.Name, colors, palette.Background);
        }

        // Reads a pasted palette: all eleven colors, or three taken as highlight / base / shadow.
        // Returns null for anything else so the editor can complain instead of rendering a broken
        // cat - which is also why this lives here rather than in the library, where a half-parsed
        // palette has no sensible meaning.
        private static PixelAvatarPalette ParsePalette(string text, Color background)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var parts = text.Split(new[] { ',', ';', ' ', '\t', '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            var parsed = new Color[parts.Length];

            for (var i = 0; i < parts.Length; i++)
            {
                parsed[i] = Color.FromString(parts[i].Trim());
            }

            if (parsed.Length == 3) return PixelAvatarPalette.FromShades("Custom", background, parsed[0], parsed[1], parsed[2]);
            if (parsed.Length == PixelAvatarSprites.PaletteSize) return PixelAvatarPalette.FromColors("Custom", background, parsed);

            return null;
        }

        private static IComponent PaletteEditor()
        {
            var start   = PixelAvatarDesign.SpottedOrange;
            var palette = PixelAvatarPalettes.Get(start);

            var previews = new[]
            {
                PixelAvatar(42, start, PixelAvatarAnimation.SitIdle).PixelSize(12),
                PixelAvatar(42, start, PixelAvatarAnimation.Move).PixelSize(7),
                PixelAvatar(42, start, PixelAvatarAnimation.Sleep).PixelSize(4)
            };

            var badges = new[]
            {
                PixelAvatarBadge(new PixelAvatar(42, start), AvatarSize.Large),
                PixelAvatarBadge(new PixelAvatar(42, start), AvatarSize.Medium),
                PixelAvatarBadge(new PixelAvatar(42, start), AvatarSize.Small)
            };

            var badgePreviews = HStack().AlignItems(ItemAlign.Center).Children(
                badges[0].PR(12), badges[1].PR(12), badges[2]);

            var pickers    = new ColorPicker[PixelAvatarSprites.PaletteSize];
            var background = ColorPicker(palette.Background).Width(52.px());
            var colors     = TextBox(palette.ToString()).WS().ReadOnly();
            var import  = TextBox().WS().SetPlaceholder("Paste 11 colors, or 3 for highlight / base / shadow");

            // The sliders are a non-destructive layer on top of `palette`: they hold a delta that
            // is re-applied from the unshifted colors on every move, so dragging back to 0 restores
            // exactly what was loaded rather than accumulating rounding drift.
            var hue        = Slider(0, -180, 180, 5).W(220.px());
            var saturation = Slider(0, -100, 100, 5).W(220.px());
            var lightness  = Slider(0, -100, 100, 5).W(220.px());
            var hueValue   = TextBlock("0°").Tiny().Secondary().Width(46.px());
            var satValue   = TextBlock("0").Tiny().Secondary().Width(46.px());
            var lightValue = TextBlock("0").Tiny().Secondary().Width(46.px());

            // Writing ColorPicker.Text raises its input event, so pushing a whole palette into the
            // pickers would otherwise bounce straight back in as eleven separate edits.
            var syncing = false;

            PixelAvatarPalette Shifted() => Adjust(palette, hue.Value, saturation.Value, lightness.Value);

            void Show(PixelAvatarPalette shown)
            {
                syncing = true;

                for (byte index = 1; index <= PixelAvatarSprites.PaletteSize; index++)
                {
                    pickers[index - 1].Text = shown.CssAt(index);
                }

                background.Text = shown.Background.ToHex();
                syncing         = false;
                colors.Text     = shown.ToString();

                foreach (var preview in previews)
                {
                    preview.SetPalette(shown);
                }

                foreach (var badge in badges)
                {
                    badge.SetPalette(shown);
                }
            }

            void ResetShift()
            {
                hue.Value        = 0;
                saturation.Value = 0;
                lightness.Value  = 0;
                hueValue.Text    = "0°";
                satValue.Text    = "0";
                lightValue.Text  = "0";
            }

            void Load(PixelAvatarPalette loaded)
            {
                if (loaded == null)
                {
                    Toast().Warning("That doesn't look like a palette - expected 11 colors, or 3.");
                    return;
                }

                palette = loaded;
                ResetShift();
                Show(palette);
            }

            void ShiftChanged()
            {
                hueValue.Text   = $"{hue.Value:+#;-#;0}°";
                satValue.Text   = $"{saturation.Value:+#;-#;0}";
                lightValue.Text = $"{lightness.Value:+#;-#;0}";
                Show(Shifted());
            }

            background.OnInput((_, __) =>
            {
                if (syncing) return;

                palette = Shifted().WithBackground(background.Color);
                ResetShift();
                Show(palette);
            });

            hue.OnInput((_, __) => ShiftChanged());
            saturation.OnInput((_, __) => ShiftChanged());
            lightness.OnInput((_, __) => ShiftChanged());

            var grid = Grid(1.fr(), 1.fr(), 1.fr(), 1.fr(), 1.fr(), 1.fr()).Gap(12.px()).RowGap(16.px());

            for (byte index = 1; index <= PixelAvatarSprites.PaletteSize; index++)
            {
                var i      = index;
                var picker = ColorPicker(palette.ColorAt(i)).Width(52.px());

                pickers[i - 1] = picker;

                picker.OnInput((_, __) =>
                {
                    if (syncing) return;

                    // Recoloring one index while a shift is active would be ambiguous, so the shift
                    // is baked into the palette first. Nothing changes on screen; the sliders just
                    // go back to 0 and the shifted colors become the new starting point.
                    palette = Shifted().WithColor(i, picker.Color);
                    ResetShift();

                    foreach (var preview in previews)
                    {
                        preview.SetColor(i, picker.Color);
                    }

                    foreach (var badge in badges)
                    {
                        badge.SetPalette(palette);
                    }

                    colors.Text = palette.ToString();
                });

                grid.Add(VStack().AlignItemsCenter().Children(
                    picker,
                    TextBlock($"{i} · {PixelAvatarSprites.ShadeOf(i)}").Tiny().Secondary().PT(4)));
            }

            var designs = HStack().WS().Wrap().Children();

            foreach (var design in PixelAvatarPalettes.All)
            {
                var d = design;
                designs.Add(Button($"{design}").Compact().OnClick(() => Load(PixelAvatarPalettes.Get(d))));
            }

            // Two shade-only palettes, to show that three colors are enough.
            // Two shade-only palettes with their own avatar background, to show that three colors
            // plus a background are enough for a whole coat.
            designs.Add(Button("Mint").Compact().OnClick(() => Load(PixelAvatarPalette.FromShades(
                "Mint", Color.FromString("#B5762E"), Color.FromString("#D6F5E3"), Color.FromString("#8FD9B6"), Color.FromString("#3F8F6E")))));
            designs.Add(Button("Lavender").Compact().OnClick(() => Load(PixelAvatarPalette.FromShades(
                "Lavender", Color.FromString("#8FB52E"), Color.FromString("#EBE1FA"), Color.FromString("#B9A0E3"), Color.FromString("#6B4E9B")))));

            IComponent ShiftRow(string label, Slider slider, IComponent value) =>
                HStack().WS().AlignItemsCenter().PB(4).Children(
                    TextBlock(label).Width(90.px()),
                    slider.NoShrink(),
                    value.PL(12));

            return HStack().WS().AlignItems(ItemAlign.Start).Children(
                VStack().Width(170.px()).AlignItemsCenter().Children(
                    previews[0],
                    HStack().AlignItems(ItemAlign.End).PT(16).Children(previews[1].PR(16), previews[2]),
                    TextBlock("Live preview").Tiny().Secondary().PT(12)),
                VStack().Grow().Children(
                    TextBlock("Start from").SemiBold().PB(4),
                    designs,
                    grid.PT(16),
                    HStack().WS().AlignItemsCenter().PT(20).PB(4).Children(
                        TextBlock("Shift every color").SemiBold().Grow(),
                        Button("Reset shift").Compact().NoShrink().OnClick(() =>
                        {
                            ResetShift();
                            Show(palette);
                        })),
                    TextBlock("Hue, saturation and lightness deltas applied to all eleven colors at once, so the shading relationships survive. Editing a color below commits the current shift.").Tiny().Secondary().PB(8),
                    ShiftRow("Hue", hue, hueValue),
                    ShiftRow("Saturation", saturation, satValue),
                    ShiftRow("Lightness", lightness, lightValue),
                    HStack().WS().AlignItemsCenter().PT(20).Children(
                        TextBlock("Avatar background").SemiBold().Width(160.px()),
                        background,
                        TextBlock("The color a PixelAvatarBadge sits this coat on. Only its hue is used - Avatar.GradientForHue turns it into the same two-stop gradient a regular Avatar uses.").Tiny().Secondary().PL(12).Grow()),
                    HStack().WS().AlignItemsCenter().PT(12).Children(badgePreviews),
                    TextBlock("Current palette").SemiBold().PT(20).PB(4),
                    HStack().WS().AlignItemsCenter().Children(
                        colors.Grow(),
                        Button("Copy colors").SetIcon(UIcons.Copy).Compact().NoShrink().ML(8).OnClick(() => Copy(Shifted().ToString(), "Colors")),
                        Button("Copy C#").SetIcon(UIcons.BracketsCurly).Compact().NoShrink().ML(8).OnClick(() => Copy(Shifted().ToCode(), "C# snippet"))),
                    TextBlock("Import").SemiBold().PT(20).PB(4),
                    HStack().WS().AlignItemsCenter().Children(
                        import.Grow(),
                        Button("Load").SetIcon(UIcons.Download).Compact().NoShrink().ML(8).OnClick(() => Load(ParsePalette(import.Text, background.Color))))));
        }

        private static void Copy(string text, string what)
        {
            navigator.clipboard.writeText(text);
            Toast().Information($"{what} copied to the clipboard.");
        }

        private static IComponent Swatch(string hex)
        {
            return Raw(Div(Att("", styles: s =>
            {
                s.width           = "18px";
                s.height          = "18px";
                s.marginRight     = "4px";
                s.borderRadius    = "3px";
                s.backgroundColor = hex;
                s.border          = "1px solid var(--tss-default-border-color)";
            }))).Tooltip(hex);
        }

        private static IComponent PaletteTable()
        {
            var rows = VStack().WS().Children();

            foreach (var design in PixelAvatarPalettes.All)
            {
                var palette   = PixelAvatarPalettes.Get(design);
                var swatches  = HStack().AlignItemsCenter().Children();

                foreach (var color in palette.Colors)
                {
                    swatches.Add(Swatch(color.ToHex()));
                }

                rows.Add(HStack().WS().AlignItemsCenter().PB(8).Children(
                    TextBlock($"{design}").Width(140.px()),
                    swatches,
                    TextBlock("background").Tiny().Secondary().PL(16).PR(6),
                    Swatch(palette.Background.ToHex()),
                    palette.Accent == null
                        ? (IComponent)TextBlock("")
                        : HStack().AlignItemsCenter().Children(
                            TextBlock("accent").Tiny().Secondary().PL(16).PR(6),
                            Swatch(palette.Accent.ToHex()))));
            }

            return rows;
        }

        public HTMLElement Render() => _content.Render();
    }
}
