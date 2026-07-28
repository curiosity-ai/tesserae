using Transpose;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The body of a tool-call inspection: the arguments a tool was called with, listed as
    /// name/value rows, above the response it returned, shown in a read-only code block.
    /// <para>
    /// Meant as the content of a <see cref="ToolCall"/> - inline, or in the detail pane of a
    /// <see cref="ToolsUsed"/> modal. The sections scroll independently: the arguments take at
    /// most half of the height available and the response claims the rest, so inspecting a long
    /// response never scrolls the arguments off screen.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ToolCallInspect")]
    public sealed class ToolCallInspect : ComponentBase<ToolCallInspect, HTMLElement>
    {
        private readonly HTMLElement _argumentsLabel;
        private readonly HTMLElement _argumentsBody;
        private readonly HTMLElement _errorLabel;
        private readonly HTMLElement _errorBody;
        private readonly HTMLElement _responseLabel;
        private readonly HTMLElement _responseBody;
        private readonly HTMLElement _empty;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="arguments">The arguments the tool was called with, as a JSON object.</param>
        /// <param name="response">The response the tool returned.</param>
        public ToolCallInspect(string arguments = null, string response = null)
        {
            _argumentsLabel = Div(Att("tss-toolcallinspect-label", text: "Arguments"));
            _argumentsBody  = Div(Att("tss-toolcallinspect-arguments"));

            _errorLabel = Div(Att("tss-toolcallinspect-label tss-toolcallinspect-label-error", text: "Error"));
            _errorBody  = Div(Att("tss-toolcallinspect-error"));

            _responseLabel = Div(Att("tss-toolcallinspect-label", text: "Response"));
            _responseBody  = Pre(Att("tss-toolcallinspect-response"));

            _empty = Div(Att("tss-toolcallinspect-empty", text: "No details available."));

            InnerElement = Div(Att("tss-toolcallinspect"),
                               _argumentsLabel, _argumentsBody,
                               _errorLabel, _errorBody,
                               _responseLabel, _responseBody,
                               _empty);

            SetArguments(arguments);
            SetError(null);
            SetResponse(response);
        }

        /// <summary>
        /// Sets the arguments shown by the component. A JSON object is listed as one name/value row
        /// per property; anything else is shown verbatim. An empty value hides the section.
        /// </summary>
        public ToolCallInspect SetArguments(string arguments)
        {
            ClearChildren(_argumentsBody);

            if (!string.IsNullOrWhiteSpace(arguments))
            {
                AppendArguments(arguments);
            }

            return UpdateSectionsVisibility();
        }

        /// <summary>
        /// Sets the response shown by the component. JSON is re-indented for reading; anything else
        /// is shown verbatim. An empty value hides the section.
        /// </summary>
        public ToolCallInspect SetResponse(string response)
        {
            _responseBody.innerText = TryFormatJson(response) ?? response ?? string.Empty;

            return UpdateSectionsVisibility();
        }

        /// <summary>
        /// Sets the error message shown by the component, for a call that failed. An empty value
        /// hides the section.
        /// </summary>
        public ToolCallInspect SetError(string error)
        {
            _errorBody.innerText = error ?? string.Empty;

            return UpdateSectionsVisibility();
        }

        /// <summary>
        /// Sets the caption shown above the arguments. Defaults to "Arguments".
        /// </summary>
        public ToolCallInspect SetArgumentsLabel(string label)
        {
            _argumentsLabel.innerText = label ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the caption shown above the response. Defaults to "Response".
        /// </summary>
        public ToolCallInspect SetResponseLabel(string label)
        {
            _responseLabel.innerText = label ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the caption shown above the error. Defaults to "Error".
        /// </summary>
        public ToolCallInspect SetErrorLabel(string label)
        {
            _errorLabel.innerText = label ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the text shown when the component has no arguments, response or error to show.
        /// </summary>
        public ToolCallInspect SetEmptyText(string text)
        {
            _empty.innerText = text ?? string.Empty;
            return this;
        }

        // Parsed natively rather than into a dictionary so numbers and booleans stay plain JS values:
        // a boxed value type would stringify to its runtime wrapper instead of 10 / true.
        private void AppendArguments(string arguments)
        {
            if (arguments.TrimStart().StartsWith("{"))
            {
                try
                {
                    var parsed = es5.JSON.parse(arguments);
                    var keys   = Transpose.Script.Write<string[]>("Object.keys({0})", parsed);

                    if (keys is object && keys.Length > 0)
                    {
                        foreach (var key in keys)
                        {
                            var value = FormatValue(Transpose.Script.Write<object>("{0}[{1}]", parsed, key));

                            _argumentsBody.appendChild(Div(Att("tss-toolcallinspect-row"),
                                                           Span(Att("tss-toolcallinspect-name",  text: key + ":")),
                                                           Span(Att("tss-toolcallinspect-value", text: value))));
                        }
                        return;
                    }
                }
                catch { /* not a JSON object after all - fall through to the raw text */ }
            }

            _argumentsBody.appendChild(Div(Att("tss-toolcallinspect-value", text: arguments)));
        }

        private ToolCallInspect UpdateSectionsVisibility()
        {
            var hasArguments = _argumentsBody.childElementCount > 0;
            var hasError     = _errorBody.innerText.Length > 0;
            var hasResponse  = _responseBody.innerText.Length > 0;

            Show(_argumentsLabel, hasArguments);
            Show(_argumentsBody,  hasArguments);
            Show(_errorLabel,     hasError);
            Show(_errorBody,      hasError);
            Show(_responseLabel,  hasResponse);
            Show(_responseBody,   hasResponse);
            Show(_empty,          !hasArguments && !hasError && !hasResponse);

            return this;
        }

        private static void Show(HTMLElement element, bool visible)
        {
            element.style.display = visible ? "" : "none";
        }

        private static string FormatValue(object value)
        {
            if (value is null) return "null";
            if (Transpose.Script.Write<bool>("typeof {0} === 'string'", value)) return Transpose.Script.Write<string>("{0}", value);

            try   { return es5.JSON.stringify(value); }
            catch { return value.ToString(); }
        }

        // Returns the content re-indented when it parses as a JSON object or array, null otherwise.
        private static string TryFormatJson(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            var trimmed = content.TrimStart();

            if (!(trimmed.StartsWith("{") || trimmed.StartsWith("["))) return null;

            try   { return es5.JSON.stringify(es5.JSON.parse(content), (double[])null, 2); }
            catch { return null; }
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
