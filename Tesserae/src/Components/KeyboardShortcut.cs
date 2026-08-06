using System;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Renders a keyboard shortcut as styled &lt;kbd&gt; chips (e.g. Ctrl+K).
    /// </summary>
    [Transpose.Name("tss.KBS")]
    public sealed class KeyboardShortcut : ComponentBase<KeyboardShortcut, HTMLElement>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public KeyboardShortcut(params string[] keys)
        {
            InnerElement = Span(Att("tss-kbd-shortcut"));

            for (int i = 0; i < keys.Length; i++)
            {
                if (i > 0)
                {
                    InnerElement.appendChild(Span(Att("tss-kbd-separator", text: "+")));
                }

                var key = Span(Att("tss-kbd-key", text: NormalizeKey(keys[i])));
                key.setAttribute("role", "term");
                InnerElement.appendChild(key);
            }

            InnerElement.setAttribute("aria-label", string.Join("+", keys.Select(NormalizeKey)));
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        /// <summary>
        /// Whether <paramref name="e"/> is the shortcut described by <paramref name="keys"/> - the same key
        /// names this component displays, so a shortcut can be declared once and what is shown cannot drift
        /// from what is bound.
        /// <para>
        /// <c>Ctrl</c> (and its aliases <c>Mod</c> / <c>CmdOrCtrl</c>) is the platform's command modifier:
        /// Ctrl elsewhere, and on Apple either Cmd or Ctrl - which is also why it shows there as ⌘. Use the
        /// explicit <c>Control</c> for a shortcut that means the Control key on a Mac too.
        /// </para>
        /// </summary>
        public static bool Matches(KeyboardEvent e, params string[] keys)
        {
            if (e is null || keys is null) return false;

            bool   needMod   = false;
            bool   needCtrl  = false;
            bool   needAlt   = false;
            bool   needShift = false;
            bool   needMeta  = false;
            string mainKey   = null;

            foreach (var raw in keys)
            {
                if (raw is null) continue;

                switch (raw.Trim())
                {
                    case "Ctrl":
                    case "ctrl":
                    case "Mod":
                    case "mod":
                    case "CmdOrCtrl":
                        needMod = true;
                        break;
                    case "Control":
                    case "control":
                        needCtrl = true;
                        break;
                    case "Alt":
                    case "alt":
                        needAlt = true;
                        break;
                    case "Shift":
                    case "shift":
                        needShift = true;
                        break;
                    case "Meta":
                    case "meta":
                    case "Cmd":
                    case "cmd":
                        needMeta = true;
                        break;
                    default:
                        mainKey = raw.Trim();
                        break;
                }
            }

            if (mainKey is null) return false;

            if (needMod && !needMeta && !needCtrl && IsApple())
            {
                //Cmd is the command modifier here, but the same shortcut pressed with Ctrl is not a different
                //shortcut - a keyboard carried over from Windows should still reach it.
                if (!e.metaKey && !e.ctrlKey) return false;
            }
            else
            {
                if ((needMod || needCtrl) != e.ctrlKey) return false;
                if (needMeta != e.metaKey) return false;
            }

            if (needAlt   != e.altKey)   return false;
            if (needShift != e.shiftKey) return false;

            return string.Equals(e.key, mainKey, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeKey(string key)
        {
            if (key == null) return "";
            string trimmedKey = key.Trim();

            switch (trimmedKey)
            {
                //The command modifier, which on Apple is Cmd - see Matches, which accepts either there.
                case "Ctrl":
                case "ctrl":
                case "Mod":
                case "mod":
                case "CmdOrCtrl":
                    return IsApple() ? "⌘" : "Ctrl";
                case "Control":
                case "control":
                    return IsApple() ? "⌃" : "Ctrl";
                case "Alt":
                case "alt":
                    return IsApple() ? "⌥" : "Alt";
                case "Shift":
                case "shift":
                    return IsApple() ? "⇧" : "Shift";
                case "Meta":
                case "meta":
                case "Cmd":
                    return IsApple() ? "⌘" : "Win";
                case "Enter":
                case "enter":
                    return "↵";
                case "Escape":
                case "escape":
                case "Esc":
                    return "Esc";
                case "ArrowUp":
                    return "↑";
                case "ArrowDown":
                    return "↓";
                case "ArrowLeft":
                    return "←";
                case "ArrowRight":
                    return "→";
                case "Backspace":
                    return "⌫";
                case "Delete":
                    return "⌦";
                case "Tab":
                    return "⇥";
                default:
                    return trimmedKey;
            }
        }

        private static bool IsApple()
        {
            // Simple detection — errs on the side of non-Apple for consistency
            return navigator.userAgent.IndexOf("Mac") >= 0 || navigator.userAgent.IndexOf("iPhone") >= 0 || navigator.userAgent.IndexOf("iPad") >= 0;
        }
    }
}
