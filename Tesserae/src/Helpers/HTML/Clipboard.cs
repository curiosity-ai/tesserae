using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [Transpose.Name("tss.Clipboard")]
    public static class Clipboard
    {
        public static void Copy(string valueToCopy, bool showMessage = true, string customMessage = null)
        {
            if (navigator.clipboard is object)
            {
                navigator.clipboard.writeText(valueToCopy).ToTask().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        if (showMessage)
                        {
                            Toast().Error("", $"Error copying: {t.Exception}");
                        }
                        else
                        {
                            console.log($"Error copying: {t.Exception}");
                        }
                    }
                    else if (showMessage)
                    {
                        Toast().Success("", customMessage ?? $"📋 Copied\n{valueToCopy}");
                    }
                });
            }
            else
            {
                var ta = TextBox(Att());
                ta.style.opacity  = "0";
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

                if (showMessage)
                {
                    Toast().Success("", customMessage ?? $"📋 Copied\n{valueToCopy}");
                }
            }
        }

        /// <summary>
        /// Copies <paramref name="html"/> as rich content, with <paramref name="plainText"/> beside it for
        /// whatever the paste target can't read HTML into (a terminal, a plain text field). A rich target -
        /// a word processor, a spreadsheet, an email - takes the HTML, so a table pastes as a table.
        /// Must run inside a user gesture (a click handler), as browsers only allow a copy from one.
        /// </summary>
        public static void CopyHtml(string html, string plainText, bool showMessage = true, string customMessage = null)
        {
            //A copy event is the one way to put two flavours on the clipboard that every browser supports and
            //that needs no permission: execCommand raises it synchronously and the handler fills clipboardData.
            //navigator.clipboard.write with a ClipboardItem does the same, but not in every browser still in use.
            var copied = false;

            Action<Event> onCopy = e =>
            {
                var data = e.As<ClipboardEvent>().clipboardData;
                if (data is null) return;
                data.setData("text/html",  html);
                data.setData("text/plain", plainText);
                e.preventDefault();
                copied = true;
            };

            document.addEventListener("copy", onCopy);

            try
            {
                document.execCommand("copy");
            }
            finally
            {
                document.removeEventListener("copy", onCopy);
            }

            if (!copied)
            {
                Copy(plainText, showMessage, customMessage);
            }
            else if (showMessage)
            {
                Toast().Success("", customMessage ?? "📋 Copied");
            }
        }
    }
}