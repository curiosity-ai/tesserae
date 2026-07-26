using System;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Gives a <see cref="PixelAvatar"/> perched on top of an <see cref="OmniBox"/> a life of its
    /// own: while you leave the box alone the cat wanders along the top edge and plays the odd
    /// animation, when you type it settles back down and, a little later, pads over to the text
    /// caret to watch you type.
    ///
    /// Resting - drifting between the idle, sitting and crouching poses, and eventually falling
    /// asleep - belongs to the avatar itself, through
    /// <see cref="PixelAvatarAnimation.AutoIdle"/>; the companion only supplies the activity in
    /// between and wakes the cat up when you come back to the box.
    ///
    /// Created automatically by <see cref="PixelAvatar.AttachTo"/> when the target is an
    /// <see cref="OmniBox"/> and the anchor is one of the <c>Top*</c> ones, and reachable through
    /// <see cref="PixelAvatarAttachment.Companion"/> to tune the timings. Every delay below is
    /// jittered on use, so nothing the cat does lands on a stopwatch.
    /// </summary>
    [Transpose.Name("tss.PixelAvatarCompanion")]
    public sealed class PixelAvatarCompanion
    {
        /// <summary>Shortest gap between spontaneous animations, in milliseconds.</summary>
        public const int DefaultMinIdleMs = 5000;
        /// <summary>Longest gap between spontaneous animations, in milliseconds.</summary>
        public const int DefaultMaxIdleMs = 14000;
        /// <summary>How long the cat waits after you stop typing before walking to the caret, in milliseconds.</summary>
        public const int DefaultCursorDelayMs = 10000;
        /// <summary>Walking speed, in CSS pixels per second.</summary>
        public const int DefaultWalkSpeedPxPerSecond = 55;

        /// <summary>
        /// How long the box has to stay untouched before the cat sleeps, in milliseconds. Sleeping
        /// is the avatar's own doing, so this is just <see cref="PixelAvatar.DefaultSleepAfterMs"/>.
        /// </summary>
        public const int DefaultSleepAfterMs = PixelAvatar.DefaultSleepAfterMs;

        // Distance from the host's edges the cat will not walk past.
        private const int EdgeInset = 6;
        // Below this, a walk is not worth playing.
        private const int MinWalkDistance = 12;
        private const int MinWalkMs       = 300;
        private const int MaxWalkMs       = 4000;

        // What the cat may do on its own. Sitting, crouching and sleeping are not in here: resting
        // is AutoIdle's job, and the companion only supplies the activity between rests. JumpDown
        // is left out because JumpUp already chains into it.
        private static readonly PixelAvatarAnimation[] Repertoire =
        {
            PixelAvatarAnimation.Move,
            PixelAvatarAnimation.Interact,
            PixelAvatarAnimation.JumpUp,
            PixelAvatarAnimation.Startle,
            PixelAvatarAnimation.Stretch
        };

        private readonly OmniBox           _omniBox;
        private readonly PixelAvatar       _avatar;
        private readonly HTMLElement       _element;
        private readonly HTMLElement       _host;
        private readonly PixelAvatarAnchor _anchor;

        private double _actionTimer;
        private double _cursorTimer;
        private double _walkTimer;
        private int    _minIdleMs  = DefaultMinIdleMs;
        private int    _maxIdleMs  = DefaultMaxIdleMs;
        private int    _cursorMs   = DefaultCursorDelayMs;
        private int    _walkSpeed  = DefaultWalkSpeedPxPerSecond;
        private double _x;
        private bool   _walking;
        private bool   _returnToIdle;
        private bool   _running;

        internal PixelAvatarCompanion(OmniBox omniBox, PixelAvatar avatar, HTMLElement host, PixelAvatarAnchor anchor)
        {
            _omniBox = omniBox;
            _avatar  = avatar;
            _element = avatar.Render();
            _host    = host;
            _anchor  = anchor;

            _host.classList.add("tss-pixelavatar-roaming");

            // A spontaneous animation is over once the avatar settles into a looping pose, which
            // is also where the follow-up chains land (Stretch ends sitting, Crouch ends crouched).
            _avatar.TrackAnimation(OnAnimationStarted);

            omniBox.OnInput((_, __) => Poke(true));
            omniBox.OnFocus((_, __) => Poke(false));

            TrackMounting();
        }

        /// <summary>Gets the avatar this companion drives.</summary>
        public PixelAvatar Avatar => _avatar;

        /// <summary>Gets whether the cat is currently asleep.</summary>
        public bool IsAsleep => _avatar.IsAsleep;

        /// <summary>
        /// Sets the range a spontaneous animation is scheduled within. Both bounds are clamped to
        /// at least <see cref="DefaultMinIdleMs"/> so the cat never becomes a distraction. The
        /// actual delay is drawn uniformly from the range every time.
        /// </summary>
        public PixelAvatarCompanion IdleDelay(int minMs, int maxMs)
        {
            _minIdleMs = minMs < DefaultMinIdleMs ? DefaultMinIdleMs : minMs;
            _maxIdleMs = maxMs < _minIdleMs ? _minIdleMs : maxMs;
            return this;
        }

        /// <summary>
        /// Sets how long the resting poses hold before the cat shifts to another one. See
        /// <see cref="PixelAvatar.RestDelay"/>; passing zero for both restores the built-in timing.
        /// </summary>
        public PixelAvatarCompanion RestDelay(int minMs, int maxMs)
        {
            _avatar.RestDelay(minMs, maxMs);
            return this;
        }

        /// <summary>
        /// Sets how long the cat waits after your last keystroke before padding over to the text
        /// caret. Pass zero to leave the caret alone entirely.
        /// </summary>
        public PixelAvatarCompanion CursorDelay(int milliseconds)
        {
            _cursorMs = milliseconds < 0 ? 0 : milliseconds;

            if (_cursorMs == 0) ClearCursorWalk();
            return this;
        }

        /// <summary>
        /// Sets how long the box has to stay untouched before the cat falls asleep. Pass zero to
        /// keep it awake indefinitely.
        /// </summary>
        public PixelAvatarCompanion SleepAfter(int milliseconds)
        {
            _avatar.SleepAfter(milliseconds);
            return this;
        }

        /// <summary>
        /// Sets how fast the cat walks, in CSS pixels per second.
        /// </summary>
        public PixelAvatarCompanion WalkSpeed(int pixelsPerSecond)
        {
            _walkSpeed = pixelsPerSecond < 1 ? 1 : pixelsPerSecond;
            return this;
        }

        /// <summary>
        /// Wakes the cat if it is asleep and restarts the sleep countdown.
        /// </summary>
        public PixelAvatarCompanion WakeUp()
        {
            Poke(false);
            return this;
        }

        /// <summary>
        /// Plays a spontaneous animation right now, as if the timer had fired.
        /// </summary>
        public PixelAvatarCompanion Fidget()
        {
            PerformRandomAction();
            return this;
        }

        /// <summary>
        /// Walks the cat over to the text caret right now, as if the countdown had elapsed.
        /// </summary>
        public PixelAvatarCompanion FollowCursor()
        {
            WalkToCursor();
            return this;
        }

        // Anything the user does at the box: wake up, push the sleep countdown back, and (when the
        // user actually typed) bring the cat back to idle and line up the walk to the caret.
        private void Poke(bool typed)
        {
            var wasAsleep = _avatar.IsAsleep;

            if (wasAsleep)
            {
                StopWalking();
                _returnToIdle = false;
                ClearActionTimer();
            }

            // Restarts the sleep countdown either way, and plays the wake-up performance when the
            // cat was actually out. The performance ends back in AutoIdle on its own.
            _avatar.Wake();

            if (typed)
            {
                if (!wasAsleep) ReturnToIdle();
                ScheduleCursorWalk();
                return;
            }

            if (!wasAsleep) ScheduleAction(true);
        }

        // Typing settles the cat, but a one-shot animation mid-flight is allowed to play out - only
        // the looping poses are cut short, since those would otherwise never end on their own.
        private void ReturnToIdle()
        {
            var current = _avatar.CurrentAnimation;

            if (_walking)
            {
                StopWalking();
                _avatar.Play(PixelAvatarAnimation.AutoIdle);
                return;
            }

            if (IsSettled(current) && _avatar.IsAutoIdling) return;

            if (IsSettled(current))
            {
                _avatar.Play(PixelAvatarAnimation.AutoIdle);
                return;
            }

            _returnToIdle = true;
        }

        private void OnAnimationStarted(PixelAvatarAnimation animation)
        {
            // Falling asleep is the avatar's own decision, taken when it has been resting long
            // enough. All the companion has to do is stop poking it until the user comes back.
            if (_avatar.IsAsleep)
            {
                _returnToIdle = false;
                ClearActionTimer();
                ClearCursorWalk();
                return;
            }

            if (!IsSettled(animation)) return;

            if (_returnToIdle && animation != PixelAvatarAnimation.Idle)
            {
                _returnToIdle = false;
                _avatar.Play(PixelAvatarAnimation.AutoIdle);
                return;
            }

            _returnToIdle = false;

            // An action's hand-over chain lands on a plain pose, which stops the cat drifting
            // between resting poses - hand it back to AutoIdle before timing the next action.
            if (!_avatar.IsAutoIdling)
            {
                _avatar.Play(PixelAvatarAnimation.AutoIdle);
                return;
            }

            ScheduleAction();
        }

        private static bool IsSettled(PixelAvatarAnimation animation)
        {
            return animation == PixelAvatarAnimation.Idle
                || animation == PixelAvatarAnimation.SitIdle
                || animation == PixelAvatarAnimation.CrouchIdle
                || animation == PixelAvatarAnimation.SleepIdle;
        }

        private void PerformRandomAction()
        {
            if (!_running || _avatar.IsAsleep) return;

            var animation = Repertoire[PixelAvatarRandom.Next(Repertoire.Length)];

            if (animation != PixelAvatarAnimation.Move)
            {
                _avatar.Play(animation);
                return;
            }

            var span = AvailableSpan();

            if (span <= 0 || !WalkTo(PixelAvatarRandom.Between(EdgeInset, EdgeInset + span)))
            {
                _avatar.Play(PixelAvatarAnimation.AutoIdle);
            }
        }

        // Walks over to wherever the user is typing, then hands back to the ordinary random flow.
        private void WalkToCursor()
        {
            _cursorTimer = 0;

            if (!_running || _avatar.IsAsleep) return;

            var caret = _omniBox.CaretClientX();

            // Nothing focused, or the box is not laid out yet - just carry on as usual.
            if (double.IsNaN(caret))
            {
                ScheduleAction(true);
                return;
            }

            var hostLeft = _host.getBoundingClientRect().As<DOMRect>().left;

            if (!WalkTo(caret - hostLeft - _avatar.RenderedWidth / 2.0)) ScheduleAction(true);
        }

        // Starts a walk to a host-relative x, clamped to the span the cat may roam. Returns false
        // when the trip is too short to be worth animating, leaving the cat where it is.
        private bool WalkTo(double target)
        {
            var span = AvailableSpan();
            if (span <= 0) return false;

            if (target < EdgeInset)        target = EdgeInset;
            if (target > EdgeInset + span) target = EdgeInset + span;

            var distance = System.Math.Abs(target - _x);
            if (distance < MinWalkDistance) return false;

            var duration = PixelAvatarRandom.Jittered((int)(distance / _walkSpeed * 1000));
            if (duration < MinWalkMs) duration = MinWalkMs;
            if (duration > MaxWalkMs) duration = MaxWalkMs;

            _avatar.Turn(target > _x ? PixelAvatarFacing.Right : PixelAvatarFacing.Left);

            _walking = true;
            _x       = target;

            _host.style.setProperty("--tss-pxav-walk", $"{duration}ms");
            _element.style.left = $"{target}px";
            _avatar.Play(PixelAvatarAnimation.Move);

            window.clearTimeout(_walkTimer);
            _walkTimer = window.setTimeout(_ =>
            {
                _walkTimer = 0;
                _walking   = false;
                _avatar.Play(PixelAvatarAnimation.AutoIdle);
            }, duration);

            return true;
        }

        // Freezes the cat wherever it happens to be rather than snapping it to either end.
        private void StopWalking()
        {
            window.clearTimeout(_walkTimer);
            _walkTimer = 0;

            if (!_walking) return;

            _walking = false;
            _x       = _element.getBoundingClientRect().As<DOMRect>().left - _host.getBoundingClientRect().As<DOMRect>().left;

            _host.style.setProperty("--tss-pxav-walk", "0ms");
            _element.style.left = $"{_x}px";
        }

        private int AvailableSpan()
        {
            var span = (int)(_host.clientWidth - _avatar.RenderedWidth - EdgeInset * 2);
            return span < 0 ? 0 : span;
        }

        private void MoveToAnchor()
        {
            var span = AvailableSpan();

            if (_anchor == PixelAvatarAnchor.TopRight)       _x = EdgeInset + span;
            else if (_anchor == PixelAvatarAnchor.TopCenter) _x = EdgeInset + span / 2.0;
            else                                             _x = EdgeInset;

            _host.style.setProperty("--tss-pxav-walk", "0ms");
            _element.style.left = $"{_x}px";
        }

        private void ScheduleAction(bool restart = false)
        {
            // Auto-idling settles every few seconds as the cat drifts between resting poses. Only
            // a deliberate reset restarts the countdown, or the next action would never come due.
            if (!restart && _actionTimer != 0) return;

            ClearActionTimer();

            if (!_running || _avatar.IsAsleep) return;

            // The caret walk is queued ahead of anything spontaneous, so the cat is not halfway
            // through a jump when it is due to go and look at what you are typing.
            if (_cursorTimer != 0) return;

            var delay = PixelAvatarRandom.Between(_minIdleMs, _maxIdleMs);
            _actionTimer = window.setTimeout(_ => PerformRandomAction(), delay);
        }

        // Typing arms this instead of the usual random action, so the cat spends the pause after
        // your last keystroke settling down and then comes over to the caret.
        private void ScheduleCursorWalk()
        {
            ClearCursorWalk();
            ClearActionTimer();

            if (!_running || _cursorMs == 0) return;

            _cursorTimer = window.setTimeout(_ => WalkToCursor(), PixelAvatarRandom.Jittered(_cursorMs));
        }

        private void ClearActionTimer()
        {
            window.clearTimeout(_actionTimer);
            _actionTimer = 0;
        }

        private void ClearCursorWalk()
        {
            window.clearTimeout(_cursorTimer);
            _cursorTimer = 0;
        }

        private void Start()
        {
            _running = true;
            MoveToAnchor();
            _avatar.Play(PixelAvatarAnimation.AutoIdle);
            ScheduleAction(true);
        }

        private void Stop()
        {
            _running = false;

            window.clearTimeout(_actionTimer);
            window.clearTimeout(_cursorTimer);
            window.clearTimeout(_walkTimer);

            _actionTimer = 0;
            _cursorTimer = 0;
            _walkTimer   = 0;
        }

        // Same one-shot dance as PixelAvatar's own mount tracking: re-arm on removal so a
        // companion inside a tab that gets swapped out keeps working when it comes back.
        private void TrackMounting()
        {
            DomObserver.WhenMounted(_host, () =>
            {
                Start();

                DomObserver.WhenRemoved(_host, () =>
                {
                    Stop();
                    TrackMounting();
                });
            });
        }
    }
}
