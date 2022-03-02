using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Clipboard")]
    public static class Clipboard
    {
        public static void Copy(string valueToCopy, bool showMessage = true, string customMessage = null)
        {
            if (navigator.clipboard is object)
            {
                navigator.clipboard.writeText(valueToCopy);
            }
            else
            {
                var ta = TextBox(_());
                ta.style.opacity = "0";
                ta.style.position = "absolute";
                document.body.appendChild(ta);

                try
                {
                    var curEl = (HTMLElement)document.activeElement;
                    ta.value = valueToCopy;
                    ta.@select();
                    document.execCommand("copy");

                    if (curEl != null)
                    {
                        curEl.focus();
                    }
                }
                finally
                {
                    document.body.removeChild(ta);
                }
            }

            if (showMessage)
            {
                Toast().Success("", customMessage ?? $"📋 Copied\n{valueToCopy}");
            }
        }
    }
}