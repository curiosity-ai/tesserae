using System;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The formatted initial value of a <see cref="TextBox"/> or <see cref="TextArea"/> (<c>InitialValue(html)</c>), shared by both.
    /// An input cannot draw bold, italic or coloured text, so the value is drawn by an overlay laid over the field's own box. It is
    /// not the field's <c>Text</c>: the field stays empty, the overlay takes no clicks, and the first edit the user makes removes it
    /// for good (typing, pasting, cutting, dictation, or <c>Text = …</c> from code, which is the application taking the field over).
    /// Only <c>b</c>/<c>strong</c>, <c>i</c>/<c>em</c>, <c>br</c>, and colour (<c>span style="color:…"</c>, <c>font color="…"</c>) survive; everything else is
    /// reduced to its text, and a colour that is not a plain colour value (hex, rgb/hsl, a name, a <c>var(--…)</c>) is dropped, so
    /// the markup can come from anywhere without being a way in for script.
    /// </summary>
    internal sealed class InitialRichText
    {
        private readonly HTMLElement _host;
        private readonly HTMLElement _input;
        private readonly bool        _multiline;
        private readonly HTMLElement _overlay;

        private bool _gone;

        /// <param name="host">The field's root, which is positioned and holds the overlay.</param>
        /// <param name="input">The field's input or textarea, whose box and font the overlay takes.</param>
        /// <param name="multiline">Whether the text wraps (a text area) or stays on one line (a text box).</param>
        public InitialRichText(HTMLElement host, HTMLElement input, bool multiline)
        {
            _host      = host;
            _input     = input;
            _multiline = multiline;
            _overlay   = Div(Att("tss-initial-value" + (multiline ? " tss-initial-value-multiline" : "")));

            _overlay.setAttribute("aria-hidden", "true");
        }

        /// <summary>Shows <paramref name="html"/> (reduced to the allowed markup) in the field, unless the user already edited it.</summary>
        public void Set(string html)
        {
            if (_gone) return;

            _overlay.innerHTML = "";

            if (string.IsNullOrEmpty(html))
            {
                Hide();
                return;
            }

            AppendSanitized(_overlay, html);

            if (_overlay.parentElement != _host)
            {
                _host.appendChild(_overlay);

                // The overlay copies the field's box and font, which are only known once it is laid out and change with it.
                Action sync = Sync;

                Script.Write("if (typeof ResizeObserver === 'function') { new ResizeObserver(function () { {1}(); }).observe({0}); }", _input, sync);
                _input.addEventListener("focus", _ => Sync());
                _input.addEventListener("mouseenter", _ => Sync());
                _input.addEventListener("input", _ => Remove());
                _input.addEventListener("paste", _ => Remove());
                _input.addEventListener("cut", _ => Remove());
            }

            _host.classList.add("tss-has-initial-value");
            Sync();
        }

        /// <summary>The application took the field over (<c>Text = …</c>): the initial value has nothing left to say.</summary>
        public void Remove()
        {
            _gone = true;
            Hide();
        }

        private void Hide()
        {
            _overlay.remove();
            _host.classList.remove("tss-has-initial-value");
        }

        private void Sync()
        {
            if (_overlay.parentElement != _host) return;

            Script.Write(@"
                var input = {0}, o = {1}, cs = getComputedStyle(input);
                o.style.left          = (input.offsetLeft + input.clientLeft) + 'px';
                o.style.top           = (input.offsetTop  + input.clientTop)  + 'px';
                o.style.width         = input.clientWidth  + 'px';
                o.style.height        = input.clientHeight + 'px';
                o.style.paddingTop    = cs.paddingTop;
                o.style.paddingRight  = cs.paddingRight;
                o.style.paddingBottom = cs.paddingBottom;
                o.style.paddingLeft   = cs.paddingLeft;
                o.style.font          = cs.font;
                o.style.letterSpacing = cs.letterSpacing;
                o.style.textAlign     = cs.textAlign;
                o.style.borderRadius  = cs.borderRadius;
                if (!{2}) { o.style.lineHeight = input.clientHeight + 'px'; }", _input, _overlay, _multiline);
        }

        // ---- sanitising -------------------------------------------------------------------------------------------------------

        /// <summary>Parses <paramref name="html"/> in an inert document (nothing in it loads or runs) and appends the allowed part of it to <paramref name="target"/>.</summary>
        internal static void AppendSanitized(HTMLElement target, string html)
        {
            var body = Script.Write<HTMLElement>("new DOMParser().parseFromString('<!doctype html><body>' + {0}, 'text/html').body", html);

            Copy(body, target);
        }

        private static void Copy(Node from, Node into)
        {
            for (var child = from.firstChild; child != null; child = child.nextSibling)
            {
                if (child.nodeType == 3)
                {
                    into.appendChild(document.createTextNode(child.textContent));
                    continue;
                }

                if (child.nodeType != 1) continue;

                var element = (HTMLElement)child;
                var tag     = element.tagName.ToLowerInvariant();

                switch (tag)
                {
                    case "script":
                    case "style":
                    case "template":
                    case "iframe":
                    case "object":
                    case "embed":
                        break; // not even their text
                    case "br":
                        into.appendChild(document.createElement("br"));
                        break;
                    case "b":
                    case "strong":
                        Wrap(element, into, "b", null);
                        break;
                    case "i":
                    case "em":
                        Wrap(element, into, "i", null);
                        break;
                    case "span":
                        Wrap(element, into, "span", SafeColor(element.style.color));
                        break;
                    case "font":
                        Wrap(element, into, "span", SafeColor(element.getAttribute("color")));
                        break;
                    default:
                        Copy(element, into); // unknown markup is dropped, its content kept
                        break;
                }
            }
        }

        private static void Wrap(HTMLElement source, Node into, string tag, string color)
        {
            var copy = document.createElement(tag);

            if (color is object) copy.style.color = color;

            Copy(source, copy);
            into.appendChild(copy);
        }

        /// <summary>Returns <paramref name="value"/> when it is a plain colour (<c>#rgb</c>, <c>rgb()</c>, <c>hsl()</c>, a name, a <c>var(--name)</c>), else null.</summary>
        internal static string SafeColor(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            value = value.Trim();

            var ok = Script.Write<bool>(@"/^(#[0-9a-f]+|[a-z]+|(rgb|hsl|oklch|hwb)a?\([0-9a-z\s.,%\/+-]*\)|var\(--[a-z0-9_-]+\))$/i.test({0})", value);

            return ok ? value : null;
        }
    }
}
