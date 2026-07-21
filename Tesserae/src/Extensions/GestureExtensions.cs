using System;
using System.Collections.Generic;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Identifies the phase of a continuous gesture (pan / pinch / rotate).
    /// </summary>
    [Transpose.Name("tss.GesturePhase")]
    public enum GesturePhase
    {
        /// <summary>The gesture has just started (movement crossed the threshold, or a second finger landed).</summary>
        Start,
        /// <summary>The gesture is in progress.</summary>
        Move,
        /// <summary>The gesture has ended (the last relevant pointer was lifted or cancelled).</summary>
        End
    }

    /// <summary>
    /// Immutable-ish snapshot of a recognised gesture, handed to gesture callbacks. A single instance is reused
    /// per recogniser and its fields are refreshed before every callback, so do not hold a reference across events.
    /// </summary>
    [Transpose.Name("tss.GestureState")]
    public sealed class GestureState
    {
        /// <summary>The component the gesture was recognised on.</summary>
        public IComponent Component { get; internal set; }

        /// <summary>The current phase of a continuous gesture (always <see cref="GesturePhase.End"/> for taps / long-press).</summary>
        public GesturePhase Phase { get; internal set; }

        /// <summary>Current pointer (or two-pointer centroid) X position in client coordinates.</summary>
        public double X { get; internal set; }

        /// <summary>Current pointer (or two-pointer centroid) Y position in client coordinates.</summary>
        public double Y { get; internal set; }

        /// <summary>Horizontal movement since the previous event of this gesture.</summary>
        public double DeltaX { get; internal set; }

        /// <summary>Vertical movement since the previous event of this gesture.</summary>
        public double DeltaY { get; internal set; }

        /// <summary>Cumulative horizontal movement since the gesture started.</summary>
        public double OffsetX { get; internal set; }

        /// <summary>Cumulative vertical movement since the gesture started.</summary>
        public double OffsetY { get; internal set; }

        /// <summary>Cumulative scale factor since the pinch started (1 = no change).</summary>
        public double Scale { get; internal set; } = 1;

        /// <summary>Scale factor change since the previous pinch event (1 = no change).</summary>
        public double ScaleDelta { get; internal set; } = 1;

        /// <summary>Cumulative rotation in degrees since the rotate gesture started.</summary>
        public double Rotation { get; internal set; }

        /// <summary>Rotation change in degrees since the previous rotate event.</summary>
        public double RotationDelta { get; internal set; }

        /// <summary>Number of pointers currently down on the element.</summary>
        public int PointerCount { get; internal set; }

        /// <summary>The underlying DOM pointer event that produced this state, if any.</summary>
        public Event Event { get; internal set; }
    }

    /// <summary>
    /// Higher-level pointer/touch gesture recognition layered on top of the unified Pointer Events API
    /// (<c>pointerdown</c>/<c>pointermove</c>/<c>pointerup</c> + <c>setPointerCapture</c>). A single recogniser is
    /// attached per element and shared by every gesture handler registered through <see cref="GestureExtensions"/>.
    /// </summary>
    [Transpose.Name("tss.GestureRecognizer")]
    public sealed class GestureRecognizer
    {
        // Movement (px) a single pointer may drift before a press is reclassified as a pan (and disqualified as a tap).
        private const double TapMoveTolerance = 10;
        // Max time (ms) between two taps for them to count as a double-tap.
        private const int DoubleTapMs = 300;
        // Time (ms) a stationary pointer must be held to fire a long-press.
        private const int LongPressMs = 500;

        private sealed class PointerInfo
        {
            public double StartX, StartY, LastX, LastY, StartTime;
        }

        private readonly IComponent  _component;
        private readonly HTMLElement _element;
        private readonly GestureState _state = new GestureState();
        private readonly Dictionary<int, PointerInfo> _pointers = new Dictionary<int, PointerInfo>();
        private readonly List<int> _order = new List<int>();

        private event Action<GestureState> Tapped;
        private event Action<GestureState> DoubleTapped;
        private event Action<GestureState> LongPressed;
        private event Action<GestureState> Panned;
        private event Action<GestureState> Pinched;
        private event Action<GestureState> Rotated;

        private bool   _panning;
        private bool   _multiTouched;
        private bool   _longPressed;
        private double _panStartX, _panStartY, _panLastX, _panLastY;
        private double _startDistance, _startAngle, _lastScale, _lastRotation;
        private int    _longPressTimer = -1;
        private int    _pendingTapTimer = -1;
        private double _lastTapTime;

        private GestureRecognizer(IComponent component)
        {
            _component       = component;
            _element         = component.Render();
            _state.Component = component;

            _element.addEventListener("pointerdown",   e => OnPointerDown(e));
            _element.addEventListener("pointermove",   e => OnPointerMove(e));
            _element.addEventListener("pointerup",     e => OnPointerUp(e));
            _element.addEventListener("pointercancel", e => OnPointerUp(e));
        }

        /// <summary>
        /// Returns the recogniser attached to the component's element, creating and caching one on the element on
        /// first use so multiple gesture handlers share a single set of DOM listeners.
        /// </summary>
        public static GestureRecognizer For(IComponent component)
        {
            var el = component.Render();

            if (el.HasOwnProperty("__tssGesture"))
            {
                return el["__tssGesture"].As<GestureRecognizer>();
            }

            var recognizer        = new GestureRecognizer(component);
            el["__tssGesture"] = recognizer;
            return recognizer;
        }

        /// <summary>Registers a tap (quick press-and-release without movement) handler.</summary>
        public GestureRecognizer OnTapped(Action<GestureState> handler) { Tapped += handler; return this; }

        /// <summary>Registers a double-tap handler. When present, single taps are delayed to disambiguate.</summary>
        public GestureRecognizer OnDoubleTapped(Action<GestureState> handler) { DoubleTapped += handler; return this; }

        /// <summary>Registers a long-press handler (stationary press held past the long-press threshold).</summary>
        public GestureRecognizer OnLongPress(Action<GestureState> handler) { LongPressed += handler; return this; }

        /// <summary>Registers a pan handler. Enables <c>touch-action: none</c> so touch panning isn't hijacked by scrolling.</summary>
        public GestureRecognizer OnPan(Action<GestureState> handler) { Panned += handler; SuppressTouchAction(); return this; }

        /// <summary>Registers a pinch (two-finger scale) handler. Enables <c>touch-action: none</c>.</summary>
        public GestureRecognizer OnPinch(Action<GestureState> handler) { Pinched += handler; SuppressTouchAction(); return this; }

        /// <summary>Registers a rotate (two-finger angle) handler. Enables <c>touch-action: none</c>.</summary>
        public GestureRecognizer OnRotate(Action<GestureState> handler) { Rotated += handler; SuppressTouchAction(); return this; }

        private void SuppressTouchAction() => _element.style.touchAction = "none";

        private static double Now() => Script.Write<double>("Date.now()");

        private static int PointerId(Event e) => Script.Write<int>("({0}.pointerId || 0)", e);

        private void CapturePointer(int id)   => Script.Write("try { {0}.setPointerCapture({1}); } catch (e) { }", _element, id);
        private void ReleasePointer(int id)    => Script.Write("try { {0}.releasePointerCapture({1}); } catch (e) { }", _element, id);

        private void OnPointerDown(Event e)
        {
            var me = e.As<MouseEvent>();
            var id = PointerId(e);

            CapturePointer(id);

            var info = new PointerInfo { StartX = me.clientX, StartY = me.clientY, LastX = me.clientX, LastY = me.clientY, StartTime = Now() };

            if (!_pointers.ContainsKey(id))
            {
                _pointers[id] = info;
                _order.Add(id);
            }
            else
            {
                _pointers[id] = info;
            }

            if (_pointers.Count == 1)
            {
                _panning      = false;
                _multiTouched = false;
                _longPressed  = false;
                _panStartX    = me.clientX;
                _panStartY    = me.clientY;
                _panLastX     = me.clientX;
                _panLastY     = me.clientY;

                if (LongPressed != null)
                {
                    CancelLongPress();
                    _longPressTimer = (int)window.setTimeout(_ => FireLongPress(), LongPressMs);
                }
            }
            else if (_pointers.Count == 2)
            {
                _multiTouched = true;
                _panning      = false;
                CancelLongPress();
                InitTwoPointerBaseline();
            }
        }

        private void OnPointerMove(Event e)
        {
            var id = PointerId(e);

            if (!_pointers.TryGetValue(id, out var info))
                return;

            var me = e.As<MouseEvent>();
            info.LastX = me.clientX;
            info.LastY = me.clientY;

            if (_pointers.Count >= 2)
            {
                HandleTwoPointerMove(e);
                return;
            }

            // Single pointer -> potential pan.
            var offsetX = me.clientX - _panStartX;
            var offsetY = me.clientY - _panStartY;

            if (!_panning)
            {
                if (Math.Sqrt(offsetX * offsetX + offsetY * offsetY) <= TapMoveTolerance)
                    return;

                _panning = true;
                CancelLongPress();
                _panLastX = _panStartX;
                _panLastY = _panStartY;
                EmitPan(GesturePhase.Start, me, offsetX, offsetY);
            }
            else
            {
                EmitPan(GesturePhase.Move, me, offsetX, offsetY);
            }

            StopEvent(e);
        }

        private void EmitPan(GesturePhase phase, MouseEvent me, double offsetX, double offsetY)
        {
            _state.Phase        = phase;
            _state.X            = me.clientX;
            _state.Y            = me.clientY;
            _state.DeltaX       = me.clientX - _panLastX;
            _state.DeltaY       = me.clientY - _panLastY;
            _state.OffsetX      = offsetX;
            _state.OffsetY      = offsetY;
            _state.Scale        = 1;
            _state.ScaleDelta   = 1;
            _state.Rotation     = 0;
            _state.RotationDelta = 0;
            _state.PointerCount = _pointers.Count;
            _state.Event        = me;
            _panLastX           = me.clientX;
            _panLastY           = me.clientY;
            Panned?.Invoke(_state);
        }

        private void InitTwoPointerBaseline()
        {
            var (ax, ay, bx, by) = FirstTwoPositions();
            var dx = bx - ax;
            var dy = by - ay;
            _startDistance = Math.Max(1, Math.Sqrt(dx * dx + dy * dy));
            _startAngle    = Math.Atan2(dy, dx) * 180 / Math.PI;
            _lastScale     = 1;
            _lastRotation  = 0;
        }

        private void HandleTwoPointerMove(Event e)
        {
            var (ax, ay, bx, by) = FirstTwoPositions();
            var dx       = bx - ax;
            var dy       = by - ay;
            var distance = Math.Max(1, Math.Sqrt(dx * dx + dy * dy));
            var angle    = Math.Atan2(dy, dx) * 180 / Math.PI;

            var scale         = distance / _startDistance;
            var scaleDelta    = scale / (_lastScale == 0 ? 1 : _lastScale);
            var rotation      = NormalizeAngle(angle - _startAngle);
            var rotationDelta = NormalizeAngle(rotation - _lastRotation);

            _state.Phase         = GesturePhase.Move;
            _state.X             = (ax + bx) / 2;
            _state.Y             = (ay + by) / 2;
            _state.DeltaX        = 0;
            _state.DeltaY        = 0;
            _state.OffsetX       = 0;
            _state.OffsetY       = 0;
            _state.Scale         = scale;
            _state.ScaleDelta    = scaleDelta;
            _state.Rotation      = rotation;
            _state.RotationDelta = rotationDelta;
            _state.PointerCount  = _pointers.Count;
            _state.Event         = e;

            Pinched?.Invoke(_state);
            Rotated?.Invoke(_state);

            _lastScale    = scale;
            _lastRotation = rotation;

            StopEvent(e);
        }

        private (double, double, double, double) FirstTwoPositions()
        {
            var a = _pointers[_order[0]];
            var b = _pointers[_order[1]];
            return (a.LastX, a.LastY, b.LastX, b.LastY);
        }

        private static double NormalizeAngle(double angle)
        {
            while (angle > 180) angle  -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }

        private void OnPointerUp(Event e)
        {
            var id = PointerId(e);

            if (!_pointers.TryGetValue(id, out var info))
                return;

            ReleasePointer(id);
            CancelLongPress();

            var me               = e.As<MouseEvent>();
            var wasSinglePointer = _pointers.Count == 1;
            var movedX           = me.clientX - info.StartX;
            var movedY           = me.clientY - info.StartY;
            var moved            = Math.Sqrt(movedX * movedX + movedY * movedY);
            var duration         = Now() - info.StartTime;

            _pointers.Remove(id);
            _order.Remove(id);

            if (_panning && _pointers.Count == 0)
            {
                _state.Phase        = GesturePhase.End;
                _state.X            = me.clientX;
                _state.Y            = me.clientY;
                _state.DeltaX       = me.clientX - _panLastX;
                _state.DeltaY       = me.clientY - _panLastY;
                _state.OffsetX      = me.clientX - _panStartX;
                _state.OffsetY      = me.clientY - _panStartY;
                _state.PointerCount = 0;
                _state.Event        = me;
                Panned?.Invoke(_state);
            }

            // A tap only counts if a single pointer was down, it stayed put, was quick, and no pan / pinch / long-press fired.
            if (wasSinglePointer && !_panning && !_multiTouched && !_longPressed && moved <= TapMoveTolerance && duration < LongPressMs)
            {
                HandleTap(me);
            }

            if (_pointers.Count < 2)
            {
                _multiTouched = _multiTouched && _pointers.Count > 0;
            }

            if (_pointers.Count == 0)
            {
                _panning = false;
            }
        }

        private void HandleTap(MouseEvent me)
        {
            var now = Now();

            FillTapState(me);

            if (_lastTapTime > 0 && (now - _lastTapTime) < DoubleTapMs && DoubleTapped != null)
            {
                CancelPendingTap();
                _lastTapTime = 0;
                DoubleTapped?.Invoke(_state);
                return;
            }

            _lastTapTime = now;

            if (DoubleTapped != null && Tapped != null)
            {
                // Delay the single tap so a follow-up tap can upgrade it to a double-tap.
                var captured = me;
                _pendingTapTimer = (int)window.setTimeout(_ =>
                {
                    _pendingTapTimer = -1;
                    FillTapState(captured);
                    Tapped?.Invoke(_state);
                }, DoubleTapMs);
            }
            else
            {
                Tapped?.Invoke(_state);
            }
        }

        private void FillTapState(MouseEvent me)
        {
            _state.Phase         = GesturePhase.End;
            _state.X             = me.clientX;
            _state.Y             = me.clientY;
            _state.DeltaX        = 0;
            _state.DeltaY        = 0;
            _state.OffsetX       = 0;
            _state.OffsetY       = 0;
            _state.Scale         = 1;
            _state.ScaleDelta    = 1;
            _state.Rotation      = 0;
            _state.RotationDelta = 0;
            _state.PointerCount  = 0;
            _state.Event         = me;
        }

        private void FireLongPress()
        {
            _longPressTimer = -1;

            if (_pointers.Count != 1 || _panning)
                return;

            _longPressed = true;

            var info = _pointers[_order[0]];
            _state.Phase         = GesturePhase.End;
            _state.X             = info.LastX;
            _state.Y             = info.LastY;
            _state.DeltaX        = 0;
            _state.DeltaY        = 0;
            _state.OffsetX       = info.LastX - info.StartX;
            _state.OffsetY       = info.LastY - info.StartY;
            _state.Scale         = 1;
            _state.ScaleDelta    = 1;
            _state.Rotation      = 0;
            _state.RotationDelta = 0;
            _state.PointerCount  = 1;
            _state.Event         = null;
            LongPressed?.Invoke(_state);
        }

        private void CancelLongPress()
        {
            if (_longPressTimer != -1)
            {
                window.clearTimeout(_longPressTimer);
                _longPressTimer = -1;
            }
        }

        private void CancelPendingTap()
        {
            if (_pendingTapTimer != -1)
            {
                window.clearTimeout(_pendingTapTimer);
                _pendingTapTimer = -1;
            }
        }
    }

    /// <summary>
    /// Fluent gesture extensions on <see cref="IComponent"/>. These layer tap, double-tap, long-press, pan, pinch
    /// and rotate recognition on top of the native pointer events already exposed by <see cref="ComponentBase{T, THTML}"/>.
    /// All handlers receive a <see cref="GestureState"/> snapshot; thresholds (tap tolerance, double-tap timing,
    /// long-press duration) are sensible defaults shared by the per-element <see cref="GestureRecognizer"/>.
    /// </summary>
    [Transpose.Name("tss.GestureX")]
    public static class GestureExtensions
    {
        /// <summary>Recognises a tap (quick press-and-release without movement).</summary>
        public static T OnTapped<T>(this T component, Action<GestureState> handler) where T : IComponent
        {
            GestureRecognizer.For(component).OnTapped(handler);
            return component;
        }

        /// <summary>Recognises a tap, invoking a parameterless handler.</summary>
        public static T OnTapped<T>(this T component, Action handler) where T : IComponent => component.OnTapped(_ => handler());

        /// <summary>Recognises a double-tap. Registering this delays single taps to disambiguate them.</summary>
        public static T OnDoubleTapped<T>(this T component, Action<GestureState> handler) where T : IComponent
        {
            GestureRecognizer.For(component).OnDoubleTapped(handler);
            return component;
        }

        /// <summary>Recognises a double-tap, invoking a parameterless handler.</summary>
        public static T OnDoubleTapped<T>(this T component, Action handler) where T : IComponent => component.OnDoubleTapped(_ => handler());

        /// <summary>Recognises a long-press (stationary press held past the long-press threshold).</summary>
        public static T OnLongPress<T>(this T component, Action<GestureState> handler) where T : IComponent
        {
            GestureRecognizer.For(component).OnLongPress(handler);
            return component;
        }

        /// <summary>Recognises a long-press, invoking a parameterless handler.</summary>
        public static T OnLongPress<T>(this T component, Action handler) where T : IComponent => component.OnLongPress(_ => handler());

        /// <summary>
        /// Recognises a pan / drag. The handler receives per-event deltas (<see cref="GestureState.DeltaX"/>/
        /// <see cref="GestureState.DeltaY"/>) and the cumulative offset since the gesture started
        /// (<see cref="GestureState.OffsetX"/>/<see cref="GestureState.OffsetY"/>).
        /// </summary>
        public static T OnPan<T>(this T component, Action<GestureState> handler) where T : IComponent
        {
            GestureRecognizer.For(component).OnPan(handler);
            return component;
        }

        /// <summary>
        /// Recognises a two-finger pinch. The handler receives the cumulative <see cref="GestureState.Scale"/> and the
        /// per-event <see cref="GestureState.ScaleDelta"/>.
        /// </summary>
        public static T OnPinch<T>(this T component, Action<GestureState> handler) where T : IComponent
        {
            GestureRecognizer.For(component).OnPinch(handler);
            return component;
        }

        /// <summary>
        /// Recognises a two-finger rotation. The handler receives the cumulative <see cref="GestureState.Rotation"/>
        /// and the per-event <see cref="GestureState.RotationDelta"/> in degrees.
        /// </summary>
        public static T OnRotate<T>(this T component, Action<GestureState> handler) where T : IComponent
        {
            GestureRecognizer.For(component).OnRotate(handler);
            return component;
        }
    }
}
