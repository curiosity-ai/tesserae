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
                        TextBlock("PixelAvatar renders a small animated sprite as a grid of absolutely positioned square divs. The artwork is stored once, as a byte grid of palette indices, and each of the eight designs is nothing more than a palette of colors for those indices - so recoloring an avatar costs eleven CSS variable writes and no repaint of the sprite."),
                        TextBlock("Thirteen animations are available. The four *Idle animations loop forever, while the rest play once and hand over to a follow-up animation: Sit settles into SitIdle, Stretch finishes by sitting down, JumpUp is followed by JumpDown, and so on."),
                        TextBlock("Avatars can be attached to any other component, which perches them on one of its edges without affecting its layout."),
                        TextBlock("The palettes are the source artwork's own colors, which means some of them are pure white and others near-black. A hairline halo in the theme's contrasting color is drawn by default so every design stays legible in both light and dark mode; Outline(false) turns it off.")))
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
                        SampleSubTitle("The eight designs, attached to buttons"),
                        TextBlock("Every avatar below is attached to the top edge of a button. Click a button to switch the animation its cat is playing."),
                        DesignGallery(),
                        SampleSubTitle("Every animation"),
                        TextBlock("Pick an animation to play it on a larger avatar. Non-looping animations chain into their follow-up, so the label updates on its own once they finish."),
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
                        SampleSubTitle("Contrast halo"),
                        TextBlock("White on a light theme and black on a dark one would otherwise vanish. Compare the same two designs with the halo on and off."),
                        OutlineGallery(),
                        SampleSubTitle("Palettes"),
                        TextBlock("The colors each design maps onto palette indices 1 to 11, extracted from the source sprite sheets."),
                        PaletteTable()))
                       .SetTitle("Usage")))
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

            var avatar = PixelAvatar(design, animations[0]).PixelSize(5);
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
            var avatar  = PixelAvatar(PixelAvatarDesign.SpottedOrange).PixelSize(10);
            var current = TextBlock("Idle").SemiBold();
            var buttons = HStack().WS().Wrap().Children();

            avatar.OnAnimationStarted((_, animation) => current.Text = $"{animation}");

            foreach (var animation in PixelAvatarSprites.All)
            {
                var sprites = PixelAvatarSprites.Get(animation);
                var a       = animation;

                buttons.Add(Button($"{animation}")
                   .Compact()
                   .Tooltip($"{sprites.Frames.Length} frame(s), {sprites.FrameDurationMs}ms each")
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

                grid.Add(PixelAvatar(PixelAvatarDesign.Tuxedo, PixelAvatarAnimation.SitIdle)
                   .Facing(facing)
                   .AttachTo(Button(labels[i]).WS(), anchor));
            }

            return grid;
        }

        private static IComponent OverlapGallery()
        {
            return HStack().WS().AlignItems(ItemAlign.End).PT(40).Children(
                PixelAvatar(PixelAvatarDesign.Black, PixelAvatarAnimation.SitIdle)
                   .AttachTo(Button("Reserved room").WS(), PixelAvatarAnchor.TopRight)
                   .Width(200.px()),
                PixelAvatar(PixelAvatarDesign.Tuxedo, PixelAvatarAnimation.SitIdle)
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
                    PixelAvatar(PixelAvatarDesign.Siamese, PixelAvatarAnimation.Move).PixelSize(size),
                    TextBlock($"{size}px").Tiny().Secondary().PT(8)));
            }

            var facing = HStack().AlignItems(ItemAlign.End).Children(
                VStack().AlignItemsCenter().PR(24).Children(
                    PixelAvatar(PixelAvatarDesign.Orange, PixelAvatarAnimation.Move).PixelSize(8),
                    TextBlock("Right").Tiny().Secondary().PT(8)),
                VStack().AlignItemsCenter().PR(24).Children(
                    PixelAvatar(PixelAvatarDesign.Orange, PixelAvatarAnimation.Move).PixelSize(8).Facing(PixelAvatarFacing.Left),
                    TextBlock("Left").Tiny().Secondary().PT(8)),
                VStack().AlignItemsCenter().PR(24).Children(
                    PixelAvatar(PixelAvatarDesign.Orange, PixelAvatarAnimation.Move).PixelSize(8).Speed(0.35),
                    TextBlock("Speed 0.35").Tiny().Secondary().PT(8)),
                VStack().AlignItemsCenter().PR(24).Children(
                    PixelAvatar(PixelAvatarDesign.Orange, PixelAvatarAnimation.Move).PixelSize(8).Speed(3),
                    TextBlock("Speed 3").Tiny().Secondary().PT(8)));

            return VStack().WS().Children(sizes, facing.PT(24));
        }

        private static IComponent OutlineGallery()
        {
            var row = HStack().AlignItems(ItemAlign.End).Children();

            foreach (var design in new[] { PixelAvatarDesign.White, PixelAvatarDesign.SpottedGrey, PixelAvatarDesign.Black, PixelAvatarDesign.Tuxedo })
            {
                row.Add(VStack().AlignItemsCenter().PR(32).Children(
                    HStack().AlignItems(ItemAlign.End).Children(
                        PixelAvatar(design, PixelAvatarAnimation.SitIdle).PixelSize(6).PR(16),
                        PixelAvatar(design, PixelAvatarAnimation.SitIdle).PixelSize(6).Outline(false)),
                    TextBlock($"{design}: halo / flat").Tiny().Secondary().PT(8)));
            }

            return row;
        }

        private static IComponent PaletteEditor()
        {
            var start   = PixelAvatarDesign.SpottedOrange;
            var palette = PixelAvatarPalettes.Get(start);

            var previews = new[]
            {
                PixelAvatar(start, PixelAvatarAnimation.SitIdle).PixelSize(12),
                PixelAvatar(start, PixelAvatarAnimation.Move).PixelSize(7),
                PixelAvatar(start, PixelAvatarAnimation.Sleep).PixelSize(4)
            };

            var pickers = new ColorPicker[PixelAvatarSprites.PaletteSize];
            var colors  = TextBox(palette.ToString()).WS().ReadOnly();
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

            PixelAvatarPalette Shifted() => palette.Adjust(hue.Value, saturation.Value, lightness.Value);

            void Show(PixelAvatarPalette shown)
            {
                syncing = true;

                for (byte index = 1; index <= PixelAvatarSprites.PaletteSize; index++)
                {
                    pickers[index - 1].Text = shown.ColorAt(index);
                }

                syncing     = false;
                colors.Text = shown.ToString();

                foreach (var preview in previews)
                {
                    preview.SetPalette(shown);
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

            hue.OnInput((_, __) => ShiftChanged());
            saturation.OnInput((_, __) => ShiftChanged());
            lightness.OnInput((_, __) => ShiftChanged());

            var grid = Grid(1.fr(), 1.fr(), 1.fr(), 1.fr(), 1.fr(), 1.fr()).Gap(12.px()).RowGap(16.px());

            for (byte index = 1; index <= PixelAvatarSprites.PaletteSize; index++)
            {
                var i      = index;
                var picker = ColorPicker(Color.FromString(palette.ColorAt(i))).Width(52.px());

                pickers[i - 1] = picker;

                picker.OnInput((_, __) =>
                {
                    if (syncing) return;

                    // Recoloring one index while a shift is active would be ambiguous, so the shift
                    // is baked into the palette first. Nothing changes on screen; the sliders just
                    // go back to 0 and the shifted colors become the new starting point.
                    palette = Shifted().WithColor(i, picker.Text);
                    ResetShift();

                    foreach (var preview in previews)
                    {
                        preview.SetColor(i, picker.Text);
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
            designs.Add(Button("Mint").Compact().OnClick(() => Load(PixelAvatarPalette.FromShades("#D6F5E3", "#8FD9B6", "#3F8F6E", "Mint"))));
            designs.Add(Button("Lavender").Compact().OnClick(() => Load(PixelAvatarPalette.FromShades("#EBE1FA", "#B9A0E3", "#6B4E9B", "Lavender"))));

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
                    TextBlock("Current palette").SemiBold().PT(20).PB(4),
                    HStack().WS().AlignItemsCenter().Children(
                        colors.Grow(),
                        Button("Copy colors").SetIcon(UIcons.Copy).Compact().NoShrink().ML(8).OnClick(() => Copy(Shifted().ToString(), "Colors")),
                        Button("Copy C#").SetIcon(UIcons.BracketsCurly).Compact().NoShrink().ML(8).OnClick(() => Copy(Shifted().ToCode(), "C# snippet"))),
                    TextBlock("Import").SemiBold().PT(20).PB(4),
                    HStack().WS().AlignItemsCenter().Children(
                        import.Grow(),
                        Button("Load").SetIcon(UIcons.Download).Compact().NoShrink().ML(8).OnClick(() => Load(PixelAvatarPalette.Parse(import.Text))))));
        }

        private static void Copy(string text, string what)
        {
            navigator.clipboard.writeText(text);
            Toast().Information($"{what} copied to the clipboard.");
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
                    swatches.Add(Raw(Div(Att("", styles: s =>
                    {
                        s.width           = "18px";
                        s.height          = "18px";
                        s.marginRight     = "4px";
                        s.borderRadius    = "3px";
                        s.backgroundColor = color;
                        s.border          = "1px solid var(--tss-default-border-color)";
                    }))).Tooltip(color));
                }

                rows.Add(HStack().WS().AlignItemsCenter().PB(8).Children(
                    TextBlock($"{design}").Width(140.px()),
                    swatches));
            }

            return rows;
        }

        public HTMLElement Render() => _content.Render();
    }
}
