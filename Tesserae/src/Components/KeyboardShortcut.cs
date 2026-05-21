using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Renders a keyboard shortcut as styled &lt;kbd&gt; chips (e.g. Ctrl+K).
    /// </summary>
    [H5.Name("tss.KBS")]
    public sealed class KeyboardShortcut : ComponentBase<KeyboardShortcut, HTMLElement>
    {
        public KeyboardShortcut(params string[] keys)
        {
            InnerElement = Span(_("tss-kbd-shortcut"));

            for (int i = 0; i < keys.Length; i++)
            {
                if (i > 0)
                {
                    InnerElement.appendChild(Span(_("tss-kbd-separator", text: "+")));
                }

                var key = Span(_("tss-kbd-key", text: NormalizeKey(keys[i])));
                key.setAttribute("role", "term");
                InnerElement.appendChild(key);
            }

            InnerElement.setAttribute("aria-label", string.Join("+", keys.Select(NormalizeKey)));
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        private static string NormalizeKey(string key)
        {
            if (key == null) return "";
            string trimmedKey = key.Trim();

            switch (trimmedKey)
            {
                case "Ctrl":
                case "ctrl":
                case "Control":
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
