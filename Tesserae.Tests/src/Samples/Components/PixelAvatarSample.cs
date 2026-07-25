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
                        TextBlock("Avatars can be attached to any other component, which perches them on one of its edges without affecting its layout.")))
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
                        SampleSubTitle("Palettes"),
                        TextBlock("The colors each design maps onto palette indices 1 to 11, extracted from the source sprite sheets."),
                        PaletteTable()))
                       .SetTitle("Usage")));
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
