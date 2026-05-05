using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Renders a keyboard shortcut as styled &lt;kbd&gt; chips (e.g. Ctrl+K).
    /// </summary>
    [H5.Name("tss.KbdShortcut")]
    public sealed class KbdShortcut : ComponentBase<KbdShortcut, HTMLElement>
    {
        public KbdShortcut(params string[] keys)
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

        public override HTMLElement Render() => InnerElement;

        private static string NormalizeKey(string key)
        {
            if (key == null) return string.Empty;
            return key.Trim() switch
            {
                "Ctrl"  or "ctrl"  or "Control" => IsApple() ? "⌃" : "Ctrl",
                "Alt"   or "alt"                => IsApple() ? "⌥" : "Alt",
                "Shift" or "shift"              => IsApple() ? "⇧" : "Shift",
                "Meta"  or "meta"  or "Cmd"     => IsApple() ? "⌘" : "Win",
                "Enter" or "enter"              => "↵",
                "Escape"or "escape"or "Esc"     => "Esc",
                "ArrowUp"                       => "↑",
                "ArrowDown"                     => "↓",
                "ArrowLeft"                     => "←",
                "ArrowRight"                    => "→",
                "Backspace"                     => "⌫",
                "Delete"                        => "⌦",
                "Tab"                           => "⇥",
                var k                           => k
            };
        }

        private static bool IsApple()
        {
            // Simple detection — errs on the side of non-Apple for consistency
            return navigator.userAgent.IndexOf("Mac") >= 0 || navigator.userAgent.IndexOf("iPhone") >= 0 || navigator.userAgent.IndexOf("iPad") >= 0;
        }
    }
}
