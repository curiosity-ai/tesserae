using System;
using Transpose;
using Transpose.Core;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Starts dictating into a text box: whatever listens (the browser's speech recognition, or a speech-to-text
    /// service of the application's own) calls <see cref="DictationSession.SetTranscript"/> as the words come in, and
    /// returns the action that stops listening. Called when the user presses the microphone button of an
    /// <see cref="OmniBox"/> (see <see cref="OmniBox.Config.DictationButton"/>).
    /// </summary>
    /// <returns>Stops listening. Called when the user presses the button again, sends the message, or the box leaves the page.
    /// May be null when there is nothing to stop. It is not called after the provider ended the session itself. After it, the session
    /// still takes <see cref="DictationSession.SetTranscript"/> for a few seconds (until <see cref="DictationSession.End"/>), so a
    /// provider that transcribes what it was sent can still deliver the last words.</returns>
    public delegate Action DictationProvider(DictationSession session);

    /// <summary>
    /// One press of the microphone button, as a <see cref="DictationProvider"/> sees it: a place to put the words
    /// heard so far, and a way to say it is over. The box shows the transcript after whatever was already typed.
    /// </summary>
    public sealed class DictationSession
    {
        private readonly Action<string, bool> _onTranscript;
        private readonly Action<string>       _onEnded;

        internal DictationSession(string startText, Action<string, bool> onTranscript, Action<string> onEnded)
        {
            StartText     = startText ?? string.Empty;
            IsActive      = true;
            _onTranscript = onTranscript;
            _onEnded      = onEnded;
        }

        /// <summary>The text in the box when the microphone was pressed; the transcript is added after it.</summary>
        public string StartText { get; }

        /// <summary>False once the provider ended the session, the user stopped it, or it failed.</summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Shows <paramref name="text"/> as what was heard so far, replacing the previous call's text - pass the
        /// whole segment each time, not only what is new. When <paramref name="isFinal"/> is true the segment is
        /// kept, and the next call starts a new one after it.
        /// </summary>
        public void SetTranscript(string text, bool isFinal = false)
        {
            if (!IsActive) return;
            _onTranscript(text ?? string.Empty, isFinal);
        }

        /// <summary>The provider has nothing more to hear (it stopped by itself, or after the user stopped it).</summary>
        public void End() => Finish(null);

        /// <summary>The provider could not listen (no microphone, no permission). The box shows <paramref name="message"/> and stops.</summary>
        public void Fail(string message) => Finish(string.IsNullOrEmpty(message) ? "Dictation failed" : message);

        internal void Stop() => IsActive = false;

        private void Finish(string error)
        {
            if (!IsActive) return;
            IsActive = false;
            _onEnded(error);
        }
    }

    /// <summary>
    /// Where an <see cref="OmniBox"/>'s microphone button gets its words from. Out of the box that is the browser's own
    /// speech recognition (<see cref="Browser"/>), and the button is not shown when the browser has none. An application
    /// that has a better source - its own speech-to-text service, an on-device model - replaces it once with
    /// <see cref="Provider"/>, for every box, or per box with <see cref="OmniBox.Config.DictationProvider"/>.
    /// </summary>
    [Transpose.Name("tss.Dictation")]
    public static class Dictation
    {
        /// <summary>
        /// Replaces the browser's speech recognition for every OmniBox that has no provider of its own. Null goes
        /// back to the browser's. With a provider set the button is always shown: it decides what is available.
        /// </summary>
        public static DictationProvider Provider { get; set; }

        /// <summary>True when the browser exposes the Web Speech API's <c>SpeechRecognition</c>.</summary>
        public static bool IsBrowserSupported =>
            Script.Write<bool>("(typeof window !== 'undefined' && (typeof window.SpeechRecognition === 'function' || typeof window.webkitSpeechRecognition === 'function'))");

        /// <summary>The browser's speech recognition as a provider: interim results stream in as the user speaks.</summary>
        public static DictationProvider Browser => StartBrowser;

        /// <summary>What a box with <paramref name="own"/> as its provider would use: that, else <see cref="Provider"/>, else the browser's when it has one.</summary>
        internal static DictationProvider Resolve(DictationProvider own)
        {
            if (own is object) return own;
            if (Provider is object) return Provider;
            return IsBrowserSupported ? Browser : null;
        }

        private static Action StartBrowser(DictationSession session)
        {
            Action<string, bool> onText = (text, final) => session.SetTranscript(text, false);
            Action               onEnd  = () => session.End();
            Action<string>       onFail = error =>
            {
                if (error == "no-speech" || error == "aborted") session.End();
                else session.Fail(error == "not-allowed" || error == "service-not-allowed" ? "The microphone is blocked" : "Speech recognition failed: " + error);
            };

            var recognition = Script.Write<object>("new (window.SpeechRecognition || window.webkitSpeechRecognition)()");
            Script.Write("{0}.continuous = true", recognition);
            Script.Write("{0}.interimResults = true", recognition);
            Script.Write("{0}.onresult = function (ev) { var s = ''; for (var i = 0; i < ev.results.length; i++) { s += ev.results[i][0].transcript; } {1}(s, false); }", recognition, onText);
            Script.Write("{0}.onerror = function (ev) { {1}(ev.error); }", recognition, onFail);
            Script.Write("{0}.onend = function () { {1}(); }", recognition, onEnd);
            Script.Write("{0}.start()", recognition);

            return () => Script.Write("try { {0}.stop(); } catch (e) {}", recognition);
        }
    }
}
