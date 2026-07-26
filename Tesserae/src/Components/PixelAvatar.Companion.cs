using System;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Gives a <see cref="PixelAvatar"/> perched on top of an <see cref="OmniBox"/> a life of its
    /// own: while you leave the box alone the cat wanders along the top edge and plays the odd
    /// animation, when you type it settles back down, and after a long silence it falls asleep
    /// until you come back.
    ///
    /// Created automatically by <see cref="PixelAvatar.AttachTo"/> when the target is an
    /// <see cref="OmniBox"/> and the anchor is one of the <c>Top*</c> ones, and reachable through
    /// <see cref="PixelAvatarAttachment.Companion"/> to tune the timings.
    /// </summary>
    [Transpose.Name("tss.PixelAvatarCompanion")]
    public sealed class PixelAvatarCompanion
    {
        /// <summary>Shortest gap between spontaneous animations, in milliseconds.</summary>
        public const int DefaultMinIdleMs = 5000;
        /// <summary>Longest gap between spontaneous animations, in milliseconds.</summary>
        public const int DefaultMaxIdleMs = 14000;
        /// <summary>How long the box has to stay untouched before the cat sleeps, in milliseconds.</summary>
        public const int DefaultSleepAfterMs = 60000;
        /// <summary>Walking speed, in CSS pixels per second.</summary>
        public const int DefaultWalkSpeedPxPerSecond = 55;

        // Distance from the host's edges the cat will not walk past.
        private const int EdgeInset = 6;
        // Below this, a walk is not worth playing.
        private const int MinWalkDistance = 12;
        private const int MinWalkMs       = 300;
        private const int MaxWalkMs       = 4000;

        // What the cat may do on its own. Sitting and crouching are not in here: resting is
        // AutoIdle's job, and the companion only supplies the activity between rests. JumpDown is
        // left out because JumpUp already chains into it.
        private static readonly PixelAvatarAnimation[] Repertoire =
        {
            PixelAvatarAnimation.Move,
            PixelAvatarAnimation.Interact,
            PixelAvatarAnimation.JumpUp,
            PixelAvatarAnimation.Startle,
            PixelAvatarAnimation.Stretch
        };

        // Waking up is a little performance rather than a snap back to Idle: the cat stretches,
        // then startles at whoever woke it. Startle's own chain drops it back into Idle.
        private static readonly PixelAvatarAnimation[] WakeSequence =
        {
            PixelAvatarAnimation.Stretch,
            PixelAvatarAnimation.Startle
        };

        private readonly PixelAvatar       _avatar;
        private readonly HTMLElement       _element;
        private readonly HTMLElement       _host;
        private readonly PixelAvatarAnchor _anchor;

        private double _actionTimer;
        private double _sleepTimer;
        private double _walkTimer;
        private int    _minIdleMs    = DefaultMinIdleMs;
        private int    _maxIdleMs    = DefaultMaxIdleMs;
        private int    _sleepAfterMs = DefaultSleepAfterMs;
        private int    _walkSpeed    = DefaultWalkSpeedPxPerSecond;
        private double _x;
        private bool   _asleep;
        private bool   _walking;
        private bool   _returnToIdle;
        private bool   _running;
        private PixelAvatarAnimation[] _sequence;
        private int                    _sequenceIndex;

        internal PixelAvatarCompanion(OmniBox omniBox, PixelAvatar avatar, HTMLElement host, PixelAvatarAnchor anchor)
        {
            _avatar  = avatar;
            _element = avatar.Render();
            _host    = host;
            _anchor  = anchor;

            _host.classList.add("tss-pixelavatar-roaming");

            // A spontaneous animation is over once the avatar settles into a looping pose, which
            // is also where the follow-up chains land (Stretch ends sitting, Crouch ends crouched).
            _avatar.TrackAnimation(OnAnimationStarted, OnAnimationFinished);

            omniBox.OnInput((_, __) => Poke(true));
            omniBox.OnFocus((_, __) => Poke(false));

            TrackMounting();
        }

        /// <summary>Gets the avatar this companion drives.</summary>
        public PixelAvatar Avatar => _avatar;

        /// <summary>Gets whether the cat is currently asleep.</summary>
        public bool IsAsleep => _asleep;

        /// <summary>
        /// Sets the range a spontaneous animation is scheduled within. Both bounds are clamped to
        /// at least <see cref="DefaultMinIdleMs"/> so the cat never becomes a distraction.
        /// </summary>
        public PixelAvatarCompanion IdleDelay(int minMs, int maxMs)
        {
            _minIdleMs = minMs < DefaultMinIdleMs ? DefaultMinIdleMs : minMs;
            _maxIdleMs = maxMs < _minIdleMs ? _minIdleMs : maxMs;
            return this;
        }

        /// <summary>
        /// Sets how long the box has to stay untouched before the cat falls asleep.
        /// </summary>
        public PixelAvatarCompanion SleepAfter(int milliseconds)
        {
            _sleepAfterMs = milliseconds < 1000 ? 1000 : milliseconds;
            ScheduleSleep();
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

        // Anything the user does at the box: wake up, push the sleep countdown back, and (when the
        // user actually typed) bring the cat back to idle.
        private void Poke(bool settleToIdle)
        {
            var wasAsleep = _asleep;
            _asleep = false;

            ScheduleSleep();

            if (wasAsleep)
            {
                StopWalking();
                _returnToIdle = false;
                PlaySequence(WakeSequence);
                return;
            }

            if (settleToIdle) ReturnToIdle();
            else ScheduleAction(true);
        }

        // Typing settles the cat, but a one-shot animation mid-flight is allowed to play out - only
        // the looping poses are cut short, since those would otherwise never end on their own.
        private void ReturnToIdle()
        {
            var current = _avatar.CurrentAnimation;

            if (_walking)
            {
                _sequence = null;
                StopWalking();
                _avatar.Play(PixelAvatarAnimation.AutoIdle);
                return;
            }

            if (IsSettled(current) && _avatar.IsAutoIdling)
            {
                ScheduleAction(true);
                return;
            }

            if (IsSettled(current))
            {
                _avatar.Play(PixelAvatarAnimation.AutoIdle);
                return;
            }

            _returnToIdle = true;
        }

        // Chains the next step of a scripted sequence in place of the animation's own follow-up.
        // PixelAvatar suppresses that follow-up when a handler calls Play, so Stretch goes to
        // Startle here instead of settling into Sit.
        private void OnAnimationFinished(PixelAvatarAnimation animation)
        {
            if (_sequence == null) return;

            if (_sequenceIndex >= _sequence.Length)
            {
                _sequence = null;
                return;
            }

            _avatar.Play(_sequence[_sequenceIndex]);
            _sequenceIndex++;
        }

        private void PlaySequence(PixelAvatarAnimation[] sequence)
        {
            _sequence      = sequence;
            _sequenceIndex = 1;
            _avatar.Play(sequence[0]);
        }

        private void OnAnimationStarted(PixelAvatarAnimation animation)
        {
            if (!IsSettled(animation)) return;

            if (_returnToIdle && animation != PixelAvatarAnimation.Idle)
            {
                _returnToIdle = false;
                _avatar.Play(PixelAvatarAnimation.AutoIdle);
                return;
            }

            _returnToIdle = false;

            if (_asleep) return;

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
            if (!_running || _asleep) return;

            _sequence = null;

            var animation = Repertoire[PixelAvatarRandom.Next(Repertoire.Length)];

            if (animation == PixelAvatarAnimation.Move) StartWalking();
            else _avatar.Play(animation);
        }

        private void StartWalking()
        {
            var span = AvailableSpan();

            if (span <= 0)
            {
                _avatar.Play(PixelAvatarAnimation.AutoIdle);
                return;
            }

            var target   = PixelAvatarRandom.Between(EdgeInset, EdgeInset + span);
            var distance = System.Math.Abs(target - _x);

            if (distance < MinWalkDistance)
            {
                _avatar.Play(PixelAvatarAnimation.AutoIdle);
                return;
            }

            var duration = (int)(distance / _walkSpeed * 1000);
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
                _walking = false;
                _avatar.Play(PixelAvatarAnimation.AutoIdle);
            }, duration);
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

            window.clearTimeout(_actionTimer);
            _actionTimer = 0;

            if (!_running || _asleep) return;

            var delay = PixelAvatarRandom.Between(_minIdleMs, _maxIdleMs);
            _actionTimer = window.setTimeout(_ => PerformRandomAction(), delay);
        }

        private void ScheduleSleep()
        {
            window.clearTimeout(_sleepTimer);
            _sleepTimer = 0;

            if (!_running) return;

            _sleepTimer = window.setTimeout(_ => FallAsleep(), _sleepAfterMs);
        }

        private void FallAsleep()
        {
            if (_asleep) return;

            _asleep = true;

            window.clearTimeout(_actionTimer);
            _actionTimer = 0;

            StopWalking();
            _sequence     = null;
            _returnToIdle = false;
            _avatar.Play(PixelAvatarAnimation.Sleep);
        }

        private void Start()
        {
            _running = true;
            MoveToAnchor();
            _avatar.Play(PixelAvatarAnimation.AutoIdle);
            ScheduleAction(true);
            ScheduleSleep();
        }

        private void Stop()
        {
            _running = false;

            window.clearTimeout(_actionTimer);
            window.clearTimeout(_sleepTimer);
            window.clearTimeout(_walkTimer);

            _actionTimer = 0;
            _sleepTimer  = 0;
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
