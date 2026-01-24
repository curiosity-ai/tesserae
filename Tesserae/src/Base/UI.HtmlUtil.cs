using System;
using System.Collections.Generic;
using H5;
using static H5.Core.dom;

namespace Tesserae
{
    public static partial class UI
    {
        /// <summary>
        /// Tries to find an ancestor of the specified element with the given tag name.
        /// </summary>
        public static HTMLElement TryToFindAncestor(this HTMLElement source, string tagNameToFind)
        {
            while (source.parentElement != null)
            {
                if (source.parentElement.tagName.ToUpper() == tagNameToFind)
                    return source.parentElement;
                source = source.parentElement;
            }
            return null;
        }

        /// <summary>
        /// Checks if the element is currently mounted in the DOM.
        /// </summary>
        public static bool IsMounted(this HTMLElement source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.isConnected;
            //return IsEqualToOrIsChildOf(source, document.querySelector("html") as Node);
        }

        /// <summary>
        /// Checks if the component is currently mounted in the DOM.
        /// </summary>
        public static bool IsMounted(this IComponent source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Render().isConnected;
            //return IsEqualToOrIsChildOf(source.Render(), document.querySelector("html") as Node);
        }

        /// <summary>
        /// Checks if the element is equal to or a child of the specified node.
        /// </summary>
        public static bool IsEqualToOrIsChildOf(this HTMLElement element, Node possibleParentElement)
        {
            return Script.Write<bool>("{0} == {1}", element, possibleParentElement) || possibleParentElement.contains(element);

            while (Script.Write<bool>("{0} != null", element)) //Short-circuit the == opeartor in C# to make this method faster
            {
                if (Script.Write<bool>("{0} == {1}", element, possibleParentElement)) //Short-circuit the == opeartor in C# to make this method faster
                {
                    return true;
                }
                element = element.parentElement;
            }
            return false;
        }

        /// <summary>
        /// Appends multiple children to the specified element.
        /// </summary>
        public static void AppendChildren(this HTMLElement source, params HTMLElement[] children)
        {
            foreach (var child in children)
                source.appendChild(child);
        }

        /// <summary>
        /// Renders a component and appends it to the document body.
        /// </summary>
        public static HTMLElement MountToBody(IComponent component, bool clearExisting = true)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            if (clearExisting)
            {
                document.body.RemoveChildElements();
            }

            var rendered = component.Render();
            document.body.appendChild(rendered);
            return rendered;
        }

        /// <summary>
        /// Prevents the default action and stops propagation for the specified event.
        /// </summary>
        public static void StopEvent(Event e)
        {
            e?.preventDefault();
            e?.stopImmediatePropagation();
        }

        public static List<Element> ToList(HTMLCollection c)
        {
            var l = new List<Element>();

            for (uint i = 0; i < c.length; i++)
            {
                l.Add(c.item(i));
            }
            return l;
        }

        /// <summary>
        /// Removes all child elements from the specified element.
        /// </summary>
        public static void RemoveChildElements(this HTMLElement source)
        {
            while (source.firstElementChild != null)
                source.firstElementChild.remove();
        }

        /// <summary>
        /// Adds or removes a CSS class based on a condition.
        /// </summary>
        public static HTMLElement UpdateClassIf(this HTMLElement htmlElement, bool value, string cssClass)
        {
            if (value)
            {
                htmlElement.classList.add(cssClass);
            }
            else
            {
                htmlElement.classList.remove(cssClass);
            }

            return htmlElement;
        }

        /// <summary>
        /// Adds or removes a CSS class based on the inverse of a condition.
        /// </summary>
        public static HTMLElement UpdateClassIfNot(this HTMLElement htmlElement, bool value, string cssClass)
        {
            if (!value)
            {
                htmlElement.classList.add(cssClass);
            }
            else
            {
                htmlElement.classList.remove(cssClass);
            }

            if (cssClass == "tss-disabled" && !Script.Write<bool>("(typeof {0}.disabled === 'undefined')", htmlElement))
            {
                Script.Write("{0}.disabled = {1}", htmlElement, !value);
            }

            return htmlElement;
        }

        /// <summary>
        /// Replaces an element with another element in the DOM.
        /// </summary>
        public static void ReplaceElement(HTMLElement source, HTMLElement replaceWith)
        {
            if (source.parentElement == null)
            {
                // If the element is isn't (or is no longer) mounted then do nothing (this may happen if an element is rendered and then async work is required to populate
                // some data in it and then the element is destroyed before the async data comes back)
                return;
            }
            source.parentElement.replaceChild(replaceWith, source);
        }

        public static void AppendElements(HTMLElement parent, IEnumerable<HTMLElement> children)
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child != null)
                    {
                        parent.appendChild(child);
                    }
                }
            }
        }

        public static void AppendElements(HTMLElement parent, params HTMLElement[] children)
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child != null)
                    {
                        parent.appendChild(child);
                    }
                }
            }
        }

        /// <summary>
        /// Clears all children (including text nodes) from the specified element.
        /// </summary>
        public static void ClearChildren(HTMLElement element)
        {
            while (element.lastChild != null)
            {
                element.removeChild(element.lastChild);
            }
        }

        public static HTMLElement AppendClass(HTMLElement element, params string[] classes)
        {
            element.classList.add(classes);
            return element;
        }

        public static HTMLElement SetStyle(HTMLElement element, Action<CSSStyleDeclaration> style)
        {
            style(element.style);
            return element;
        }

        public static T SetStyle<T>(this T element, Action<CSSStyleDeclaration> style) where T : HTMLElement
        {
            style(element.style);
            return element;
        }

        [IgnoreGeneric] // 2019-07-10 DWR: This is needed so that we can call it with types such as HTMLTableHeaderCellElement that seem to be [External] (and so can't be passed as a generic type param at runtime)
        static T InitElement<T>(T element, Attributes init, params HTMLElement[] children) where T : HTMLElement
        {
            init?.InitElement(element);

            if (children != null)
            {
                if (Script.Write<bool>("Array.isArray(children)"))
                {
                    AppendElements(element, children);
                }
                else
                {
                    element.appendChild(children);
                }
            }
            return element;
        }

        /// <summary>
        /// Creates an element from a raw string, either as text or HTML.
        /// </summary>
        public static HTMLElement Raw(string html, bool forceParseAsHTML = false)
        {
            if (string.IsNullOrWhiteSpace(html))
                return SPAN();

            return (IsProbablyHtml(html) || forceParseAsHTML)
                ? new HTMLSpanElement { innerHTML   = html }
                : new HTMLSpanElement { textContent = html };
        }

        private static bool IsProbablyHtml(string html)
        {
            //Super simple heuristic to detect if text contains any <> html tags
            var open = html.IndexOf('<');

            if (open >= 0)
            {
                var close = html.IndexOf('>', open);
                if (close >= 1) return true;
            }

            if (html.IndexOf("&nbsp;") >= 0)
            {
                return true;
            }

            return false;
        }

        public static Text Text(string text)
        {
            return document.createTextNode(text);
        }

        public static HTMLDivElement Div(Attributes init, HTMLElement first)
        {
            var result = InitElement(new HTMLDivElement(), init, null);
            result.appendChild(first);
            return result;
        }

        public static HTMLDivElement Div(Attributes init, HTMLElement first, HTMLElement second)
        {
            var result = InitElement(new HTMLDivElement(), init, null);
            result.appendChild(first);
            result.appendChild(second);
            return result;
        }

        /// <summary>
        /// Creates a &lt;div&gt; element.
        /// </summary>
        public static HTMLDivElement Div(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLDivElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;span&gt; element.
        /// </summary>
        public static HTMLSpanElement Span(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLSpanElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;p&gt; element.
        /// </summary>
        public static HTMLParagraphElement P(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLParagraphElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;pre&gt; element.
        /// </summary>
        public static HTMLPreElement Pre(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLPreElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;strong&gt; element.
        /// </summary>
        public static HTMLElement Strong(Attributes init, params HTMLElement[] children)
        {
            return InitElement(document.createElement("strong"), init, children);
        }

        /// <summary>
        /// Creates an &lt;i&gt; element.
        /// </summary>
        public static HTMLElement I(Attributes init, params HTMLElement[] children)
        {
            return InitElement(document.createElement("i"), init, children);
        }

        public static HTMLElement I(UIcons icon, UIconsWeight weight = UIconsWeight.Regular, string cssClass = null)
        {
            return I(_($"{Tesserae.Icon.Transform(icon, weight)} {cssClass}"));
        }

        /// <summary>
        /// Creates a &lt;sup&gt; element.
        /// </summary>
        public static HTMLElement Sup(Attributes init, params HTMLElement[] children)
        {
            return InitElement(document.createElement("sup"), init, children);
        }

        /// <summary>
        /// Creates a &lt;br&gt; element.
        /// </summary>
        public static HTMLBRElement Br(Attributes init)
        {
            return InitElement(new HTMLBRElement(), init);
        }

        /// <summary>
        /// Creates an &lt;ol&gt; element.
        /// </summary>
        public static HTMLOListElement Ol(Attributes init, params HTMLLIElement[] children)
        {
            var ol = new HTMLOListElement();
            init?.InitElement(ol);
            AppendElements(ol, children);
            return ol;
        }

        /// <summary>
        /// Creates an &lt;ul&gt; element.
        /// </summary>
        public static HTMLUListElement Ul(Attributes init, params HTMLLIElement[] children)
        {
            return InitElement(new HTMLUListElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;li&gt; element.
        /// </summary>
        public static HTMLLIElement Li(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLLIElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;main&gt; element.
        /// </summary>
        public static HTMLElement Main(Attributes init, params HTMLElement[] children)
        {
            return InitElement(document.createElement("main"), init, children);
        }

        /// <summary>
        /// Creates a &lt;header&gt; element.
        /// </summary>
        public static HTMLElement Header(Attributes init, params HTMLElement[] children)
        {
            return InitElement(document.createElement("header"), init, children);
        }

        /// <summary>
        /// Creates a &lt;footer&gt; element.
        /// </summary>
        public static HTMLElement Footer(Attributes init, params HTMLElement[] children)
        {
            return InitElement(document.createElement("footer"), init, children);
        }

        /// <summary>
        /// Creates a &lt;section&gt; element.
        /// </summary>
        public static HTMLElement Section(Attributes init, params HTMLElement[] children)
        {
            return InitElement(document.createElement("section"), init, children);
        }

        /// <summary>
        /// Creates a &lt;nav&gt; element.
        /// </summary>
        public static HTMLElement Nav(Attributes init, params HTMLElement[] children)
        {
            return InitElement(document.createElement("nav"), init, children);
        }

        /// <summary>
        /// Creates a &lt;hr&gt; element.
        /// </summary>
        public static HTMLHRElement HR(Attributes init)
        {
            return InitElement(new HTMLHRElement(), init);
        }

        /// <summary>
        /// Creates an &lt;a&gt; element.
        /// </summary>
        public static HTMLAnchorElement A(Attributes init, params HTMLElement[] children)
        {
            var a = new HTMLAnchorElement();
            init?.InitAnchorElement(a);
            AppendElements(a, children);
            return a;
        }

        /// <summary>
        /// Creates an &lt;img&gt; element.
        /// </summary>
        public static HTMLImageElement Image(Attributes init)
        {
            var a = new HTMLImageElement();
            init?.InitImageElement(a);
            return a;
        }

        /// <summary>
        /// Creates a &lt;canvas&gt; element.
        /// </summary>
        public static HTMLCanvasElement Canvas(Attributes init)
        {
            return InitElement(new HTMLCanvasElement(), init);
        }

        static HTMLElement H(Attributes init, string type, params HTMLElement[] children)
        {
            var h = document.createElement(type);
            init?.InitElement(h);
            AppendElements(h, children);
            return h;
        }

        /// <summary>
        /// Creates an &lt;h1&gt; element.
        /// </summary>
        public static HTMLElement H1(Attributes init, params HTMLElement[] children)
        {
            return H(init, "h1", children);
        }

        /// <summary>
        /// Creates an &lt;h2&gt; element.
        /// </summary>
        public static HTMLElement H2(Attributes init, params HTMLElement[] children)
        {
            return H(init, "h2", children);
        }

        /// <summary>
        /// Creates an &lt;h3&gt; element.
        /// </summary>
        public static HTMLElement H3(Attributes init, params HTMLElement[] children)
        {
            return H(init, "h3", children);
        }

        /// <summary>
        /// Creates an &lt;h4&gt; element.
        /// </summary>
        public static HTMLElement H4(Attributes init, params HTMLElement[] children)
        {
            return H(init, "h4", children);
        }

        /// <summary>
        /// Creates an &lt;h5&gt; element.
        /// </summary>
        public static HTMLElement H5(Attributes init, params HTMLElement[] children)
        {
            return H(init, "h5", children);
        }

        /// <summary>
        /// Creates an &lt;h6&gt; element.
        /// </summary>
        public static HTMLElement H6(Attributes init, params HTMLElement[] children)
        {
            return H(init, "h6", children);
        }

        /// <summary>
        /// Creates a &lt;form&gt; element.
        /// </summary>
        public static HTMLFormElement FormE(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLFormElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;fieldset&gt; element.
        /// </summary>
        public static HTMLFieldSetElement FieldSet(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLFieldSetElement(), init, children);
        }

        /// <summary>
        /// Creates an &lt;optgroup&gt; element.
        /// </summary>
        public static HTMLOptGroupElement OptGroup(Attributes init, params HTMLOptionElement[] children)
        {
            return InitElement(new HTMLOptGroupElement(), init, children);
        }

        /// <summary>
        /// Creates an &lt;option&gt; element.
        /// </summary>
        public static HTMLOptionElement Option(Attributes init)
        {
            var f = new HTMLOptionElement();
            init?.InitOptionElement(f);
            return f;
        }

        /// <summary>
        /// Creates a &lt;legend&gt; element.
        /// </summary>
        public static HTMLLegendElement Legend(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLLegendElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;button&gt; element.
        /// </summary>
        public static HTMLButtonElement Button(Attributes init, params HTMLElement[] children)
        {
            var f = new HTMLButtonElement();
            init?.InitButtonElement(f);
            AppendElements(f, children);
            return f;
        }

        /// <summary>
        /// Creates a &lt;select&gt; element.
        /// </summary>
        public static HTMLSelectElement Select(Attributes init, params HTMLOptionElement[] children)
        {
            return InitElement(new HTMLSelectElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;datalist&gt; element.
        /// </summary>
        public static HTMLDataListElement DataList(Attributes init, params HTMLOptionElement[] children)
        {
            var dl = new HTMLDataListElement();
            init?.InitElement(dl);
            AppendElements(dl, children);
            return dl;
        }

        /// <summary>
        /// Creates an &lt;input type="text"&gt; element.
        /// </summary>
        public static HTMLInputElement TextBox(Attributes init)
        {
            var f = new HTMLInputElement();
            init?.InitInputElement(f);
            return f;
        }

        /// <summary>
        /// Creates an &lt;input type="checkbox"&gt; element.
        /// </summary>
        public static HTMLInputElement CheckBox(Attributes init)
        {
            var input = new HTMLInputElement { type = "checkbox" };
            init?.InitInputElement(input);
            return input;
        }

        /// <summary>
        /// Creates an &lt;input type="radio"&gt; element.
        /// </summary>
        public static HTMLInputElement RadioButton(Attributes init)
        {
            var input = new HTMLInputElement { type = "radio" };
            init?.InitInputElement(input);
            return input;
        }

        /// <summary>
        /// Creates an &lt;input type="file"&gt; element.
        /// </summary>
        public static HTMLInputElement FileInput(Attributes init)
        {
            var input = new HTMLInputElement { type = "file" };
            init?.InitInputElement(input);
            return input;
        }

        /// <summary>
        /// Creates a &lt;textarea&gt; element.
        /// </summary>
        public static HTMLTextAreaElement TextArea(Attributes init)
        {
            var a = new HTMLTextAreaElement();
            init?.InitTextAreaElement(a);
            return a;
        }

        /// <summary>
        /// Creates a &lt;label&gt; element.
        /// </summary>
        public static HTMLLabelElement Label(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLLabelElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;table&gt; element.
        /// </summary>
        public static HTMLTableElement Table(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLTableElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;caption&gt; element.
        /// </summary>
        public static HTMLTableCaptionElement Caption(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLTableCaptionElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;thead&gt; element.
        /// </summary>
        public static HTMLElement THeader(Attributes init, params HTMLTableRowElement[] children)
        {
            return InitElement(document.createElement("thead"), init, children);
        }

        /// <summary>
        /// Creates a &lt;colgroup&gt; element.
        /// </summary>
        public static HTMLTableColElement TColgroup(Attributes init, params HTMLTableColElement[] children)
        {
            return InitElement(document.createElement("colgroup").As<HTMLTableColElement>(), init, children);
        }

        /// <summary>
        /// Creates a &lt;col&gt; element.
        /// </summary>
        public static HTMLTableColElement TCol(Attributes init)
        {
            return InitElement(new HTMLTableColElement(), init);
        }

        /// <summary>
        /// Creates a &lt;tbody&gt; element.
        /// </summary>
        public static HTMLTableSectionElement TBody(Attributes init, params HTMLTableRowElement[] children)
        {
            return InitElement(new HTMLTableSectionElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;tfoot&gt; element.
        /// </summary>
        public static HTMLTableSectionElement TFooter(Attributes init, params HTMLTableRowElement[] children)
        {
            return InitElement(new HTMLTableSectionElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;tr&gt; element.
        /// </summary>
        public static HTMLTableRowElement TRow(Attributes init, params HTMLTableDataCellElement[] children)
        {
            return InitElement(new HTMLTableRowElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;tr&gt; element.
        /// </summary>
        public static HTMLTableRowElement TRow(Attributes init, params HTMLTableHeaderCellElement[] children)
        {
            return InitElement(new HTMLTableRowElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;td&gt; element.
        /// </summary>
        public static HTMLTableDataCellElement Td(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLTableDataCellElement(), init, children);
        }

        /// <summary>
        /// Creates a &lt;th&gt; element.
        /// </summary>
        public static HTMLTableHeaderCellElement Th(Attributes init, params HTMLElement[] children)
        {
            return InitElement(new HTMLTableHeaderCellElement(), init, children);
        }

        /// <summary>
        /// Creates an &lt;iframe&gt; element.
        /// </summary>
        public static HTMLIFrameElement IFrame(Attributes init, params HTMLElement[] children)
        {
            var a = new HTMLIFrameElement();
            init?.InitIFrameElement(a);
            AppendElements(a, children);
            return a;
        }

        /// <summary>
        /// Creates a &lt;div&gt; element.
        /// </summary>
        public static HTMLDivElement DIV(params HTMLElement[] child)
        {
            return InitElement(new HTMLDivElement(), null, child);
        }

        /// <summary>
        /// Creates a &lt;span&gt; element.
        /// </summary>
        public static HTMLSpanElement SPAN(params HTMLElement[] child)
        {
            return InitElement(new HTMLSpanElement(), null, child);
        }

        /// <summary>
        /// Makes an element scroll its content on hover.
        /// </summary>
        public static void MakeScrollableOnHover(HTMLElement element, int initialDelay = 1000, double pixelsPerSecond = 100)
        {
            double               progress       = 0;
            bool                 stop           = false;
            double               initial        = 0;
            double               maxDelta       = 0;
            double               t0             = -1;
            double               ratio          = 1;
            FrameRequestCallback animateElement = null;

            var target = ((HTMLElement)element.firstElementChild);

            animateElement = (t) =>
            {
                if (t0 < 0)
                {
                    t0    = t;
                    ratio = (pixelsPerSecond) / 1000;
                }
                progress = (t - t0) * ratio;

                if (progress > maxDelta || stop)
                {
                    if (stop)
                    {
                        target.style.marginLeft = "";
                    }
                    else
                    {
                        target.style.marginLeft = "-" + maxDelta + "px";
                    }
                }
                else
                {
                    target.style.marginLeft = "-" + (progress) + "px";
                    window.requestAnimationFrame(animateElement);
                }
            };

            element.onmouseenter = (e) =>
            {
                stop     = false;
                progress = 0;
                t0       = -1;
                maxDelta = -((DOMRect)element.getBoundingClientRect()).width;

                foreach (var c in element.children)
                {
                    maxDelta += ((DOMRect)c.getBoundingClientRect()).width;
                }

                if (maxDelta > 0)
                {
                    initial = window.setTimeout((t) =>
                    {
                        window.requestAnimationFrame(animateElement);
                    }, initialDelay);
                }
            };

            element.onmouseleave = (e) =>
            {
                stop = true;
                window.clearTimeout(initial);
                target.style.marginLeft = "";
            };
        }
    }
}
