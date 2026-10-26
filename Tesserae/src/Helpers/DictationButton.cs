using System;
using TNT;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The microphone button of a <see cref="TextBox"/> or <see cref="TextArea"/>, behaving as the one of an <see cref="OmniBox"/>
    /// does: it dictates after the text already in the field, shows a recording state while listening, is shown only when
    /// something can supply words (<see cref="Dictation.Resolve"/>), and stops when pressed again or when the field leaves the page.
    /// </summary>
    internal sealed class DictationButton
    {
        private readonly Button               _button;
        private readonly HTMLElement          _host;
        private readonly Func<string>         _getText;
        private readonly Action<string>       _setText;
        private readonly Func<int>            _maxLength;
        private readonly DictationProvider    _own;

        private DictationSession _dictation;
        private Action           _stop;
        private DictationSession _lingering;
        private double           _lingerTimer;
        private string           _committed;

        /// <param name="host">The field's root element; the button is added to it and it carries the dictating class.</param>
        /// <param name="mountedOn">The element whose presence on the page is watched.</param>
        public DictationButton(HTMLElement host, HTMLElement mountedOn, Func<string> getText, Action<string> setText, Func<int> maxLength, DictationProvider own)
        {
            _host      = host;
            _getText   = getText;
            _setText   = setText;
            _maxLength = maxLength;
            _own       = own;

            _button = Button().SetIcon(UIcons.Microphone).Class("tss-textbox-dictate-btn").AriaLabel("Dictate".t()).Tooltip("Dictate".t());
            _button.Render().setAttribute("aria-pressed", "false");
            _button.OnClick((_, e) =>
            {
                // Clicks inside a field's container must not move focus to the input, which on a phone opens the keyboard over the listening state
                e.stopPropagation();
                Toggle();
            });

            host.classList.add("tss-textbox-has-dictation");
            host.appendChild(_button.Render());

            // Shown only when something can supply words; an application may install its provider after the field was built, so it is
            // checked when the field reaches the page. Leaving the page ends the dictation (the microphone must not stay open behind a field nobody can see).
            DomObserver.WhenMounted(mountedOn, () =>
            {
                if (Dictation.Resolve(_own) is null)
                {
                    _button.Collapse();
                    host.classList.remove("tss-textbox-has-dictation");
                }
                else
                {
                    _button.Show();
                    host.classList.add("tss-textbox-has-dictation");
                }

                DomObserver.WhenRemoved(mountedOn, () =>
                {
                    Stop();
                    DropLingering();
                });
            });
        }

        public bool IsDictating => _dictation is object;

        public void Start()
        {
            if (_dictation is object) return;

            var provider = Dictation.Resolve(_own);
            if (provider is null) return;

            _committed = _getText() ?? string.Empty;

            DictationSession session = null;
            session = new DictationSession(_committed, (text, final) => OnTranscript(session, text, final), error => OnEnded(session, error));
            _dictation = session;
            SetState(true);

            Action stop;
            try
            {
                stop = provider(session);
            }
            catch
            {
                OnEnded(session, null);
                throw;
            }

            // A provider that ended the session while starting (no microphone) has nothing left to stop.
            if (_dictation == session) _stop = stop;
        }

        public void Stop()
        {
            var session = _dictation;
            if (session is null) return;

            var stop = _stop;
            _dictation = null;
            _stop      = null;
            SetState(false);

            // A source that transcribes what it was sent (a speech-to-text service) answers a moment after being told to stop, with the last
            // words. The session keeps taking them until the provider ends it - or for a few seconds.
            DropLingering();
            _lingering   = session;
            _lingerTimer = window.setTimeout(_ => DropLingering(), 5000);

            stop?.Invoke();
        }

        private void Toggle()
        {
            if (_dictation is object) Stop();
            else Start();
        }

        private void DropLingering()
        {
            if (_lingering is null) return;

            window.clearTimeout(_lingerTimer);
            _lingering.Stop();
            _lingering = null;
        }

        private void OnTranscript(DictationSession session, string text, bool isFinal)
        {
            if (session != _dictation && session != _lingering) return;

            var composed = Join(_committed, text);
            var max      = _maxLength();

            if (max > 0 && composed.Length > max) composed = composed.Substring(0, max);

            _setText(composed);

            if (isFinal) _committed = composed;
        }

        private void OnEnded(DictationSession session, string error)
        {
            if (session == _lingering)
            {
                window.clearTimeout(_lingerTimer);
                _lingering = null;

                if (error is object) Toast().Error(error);
                return;
            }

            if (session != _dictation) return;

            _dictation = null;
            _stop      = null;
            SetState(false);

            if (error is object) Toast().Error(error);
        }

        private static string Join(string before, string heard)
        {
            if (string.IsNullOrEmpty(before)) return heard;
            if (string.IsNullOrEmpty(heard)) return before;

            var glued = char.IsWhiteSpace(before[before.Length - 1]) || char.IsWhiteSpace(heard[0]);
            return glued ? before + heard : before + " " + heard;
        }

        private void SetState(bool on)
        {
            _host.classList.toggle("tss-textbox-dictating", on);

            var el = _button.Render();
            el.classList.toggle("tss-textbox-dictating-btn", on);
            el.setAttribute("aria-pressed", on ? "true" : "false");
            _button.Tooltip(on ? "Stop dictation".t() : "Dictate".t());
        }
    }
}
