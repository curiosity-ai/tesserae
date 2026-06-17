using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 210, Icon = UIcons.CursorFingerClick)]
    public class GesturesSample : IComponent, ISample
    {
        private readonly IComponent _content;

        // Live transform state, accumulated from per-event gesture deltas.
        private double _panX, _panY, _scale = 1, _rotation;

        public GesturesSample()
        {
            var status = TextBlock("Drag, pinch or rotate the box (use touch / trackpad for pinch & rotate).").Secondary();
            var values = TextBlock("pan: 0, 0 · scale: 1.00 · rotation: 0°");

            var box = VStack()
               .WidthStretch().HeightStretch()
               .JustifyContent(ItemJustify.Center).AlignItemsCenter()
               .Children(TextBlock("Tap · Double-tap · Long-press · Drag · Pinch · Rotate").Bold())
               .Background(Theme.Primary.Background)
               .Style(s =>
                {
                    s.color        = "white";
                    s.borderRadius = "12px";
                    s.cursor       = "grab";
                    s.userSelect   = "none";
                    s.maxWidth     = "260px";
                    s.maxHeight    = "160px";
                    s.textAlign    = "center";
                    s.padding      = "16px";
                });

            void ApplyTransform()
            {
                box.Render().style.transform = $"translate({_panX.ToString("0.#")}px, {_panY.ToString("0.#")}px) scale({_scale.ToString("0.###")}) rotate({_rotation.ToString("0.#")}deg)";
                values.Text = $"pan: {_panX.ToString("0")}, {_panY.ToString("0")} · scale: {_scale.ToString("0.00")} · rotation: {_rotation.ToString("0")}°";
            }

            box
               .OnTapped(_ => status.Text = "Tapped")
               .OnDoubleTapped(_ =>
                {
                    _panX = 0; _panY = 0; _scale = 1; _rotation = 0;
                    ApplyTransform();
                    status.Text = "Double-tapped — reset";
                })
               .OnLongPress(_ => status.Text = "Long-pressed")
               .OnPan(g =>
                {
                    _panX += g.DeltaX;
                    _panY += g.DeltaY;
                    if (g.Phase == GesturePhase.Start) status.Text = "Panning…";
                    if (g.Phase == GesturePhase.End)   status.Text = $"Pan ended (offset {g.OffsetX.ToString("0")}, {g.OffsetY.ToString("0")})";
                    ApplyTransform();
                })
               .OnPinch(g =>
                {
                    _scale = Math.Max(0.25, Math.Min(4, _scale * g.ScaleDelta));
                    status.Text = "Pinching…";
                    ApplyTransform();
                })
               .OnRotate(g =>
                {
                    _rotation += g.RotationDelta;
                    ApplyTransform();
                });

            var stage = Stack().Relative().WS().H(280)
               .Children(box)
               .Style(s =>
                {
                    s.border          = "1px dashed var(--tss-colors-neutral-500-alpha)";
                    s.borderRadius     = "8px";
                    s.overflow         = "hidden";
                    s.alignItems       = "center";
                    s.justifyContent   = "center";
                    s.touchAction      = "none";
                });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(GesturesSample), UIcons.CursorFingerClick, "High-level pointer / touch gesture recognizers")
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("The fluent gesture extensions (.OnTapped, .OnDoubleTapped, .OnLongPress, .OnPan, .OnPinch, .OnRotate) layer tap, drag, pinch and rotate recognition on top of the unified Pointer Events API, so a single code path works for mouse, touch and pen. Each handler receives a GestureState snapshot with per-event deltas and cumulative offset/scale/rotation. These same recognizers now drive SplitView resizing on touch screens."))).SetTitle("Overview")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        stage,
                        VStack().WS().PT(12).Children(status, values))).SetTitle("Interactive box")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
