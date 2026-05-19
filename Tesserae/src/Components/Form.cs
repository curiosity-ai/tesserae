using H5;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A &lt;form&gt; container. Wrapping login inputs in a real form (with <see cref="Action"/> and
    /// proper <c>autocomplete</c> hints on the inputs) is what lets browser and third-party password
    /// managers detect the submission, scope saved credentials to the current origin (subdomain
    /// included) instead of falling back to registrable-domain heuristics, and prompt the user to
    /// save or update the password.
    /// </summary>
    [H5.Name("tss.Form")]
    public class Form : IContainer<Form, IComponent>, IHasMarginPadding
    {
        private readonly HTMLFormElement _form;

        private event ComponentEventHandler<Form, Event> Submitted;

        public Form()
        {
            _form = FormE(_("tss-form"));

            // Default to "POST" so password managers treat this as a credential submission target
            // (GET forms are usually treated as search). Consumers can override via SetMethod.
            _form.method = "post";

            // Prevent the browser's default navigation - the login is performed via fetch/XHR in
            // OnSubmit handlers. We still raise the submit event so the browser's built-in
            // password manager observes a real form submission and can offer to save credentials.
            _form.addEventListener("submit", e =>
            {
                e.preventDefault();
                Submitted?.Invoke(this, e);
            });
        }

        /// <summary>
        /// The form's submission URL. Should be a same-origin URL (absolute or relative) on the
        /// current subdomain — this is the strongest hint a password manager has for scoping the
        /// saved credential to <c>location.host</c> rather than to the registrable domain.
        /// </summary>
        public string Action
        {
            get => _form.action;
            set => _form.action = value ?? string.Empty;
        }

        /// <summary>The form's HTTP method (defaults to <c>post</c>).</summary>
        public string Method
        {
            get => _form.method;
            set => _form.method = string.IsNullOrEmpty(value) ? "post" : value;
        }

        /// <summary>The form's <c>name</c> attribute.</summary>
        public string FormName
        {
            get => _form.name;
            set => _form.name = value ?? string.Empty;
        }

        /// <summary>
        /// The form-level <c>autocomplete</c> attribute (<c>on</c> / <c>off</c>). Leave unset to
        /// inherit the browser default. Setting this to <c>off</c> disables saving for the whole
        /// form, which is the opposite of what a sign-in form usually wants.
        /// </summary>
        public string AutoComplete
        {
            get => _form.getAttribute("autocomplete");
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _form.removeAttribute("autocomplete");
                }
                else
                {
                    _form.setAttribute("autocomplete", value);
                }
            }
        }

        public string Margin  { get => _form.style.margin;  set => _form.style.margin  = value; }
        public string Padding { get => _form.style.padding; set => _form.style.padding = value; }

        public Form SetAction(string action)         { Action       = action; return this; }
        public Form SetMethod(string method)         { Method       = method; return this; }
        public Form SetName(string name)             { FormName     = name;   return this; }
        public Form SetAutoComplete(string value)    { AutoComplete = value;  return this; }

        /// <summary>
        /// Attaches a handler invoked when the form is submitted (via Enter on a child input or
        /// a <c>&lt;button type="submit"&gt;</c> inside the form). The native browser navigation
        /// is always cancelled.
        /// </summary>
        public Form OnSubmit(ComponentEventHandler<Form, Event> onSubmit)
        {
            Submitted += onSubmit;
            return this;
        }

        /// <summary>
        /// Programmatically request submission (mirrors pressing Enter inside the form).
        /// Uses <c>HTMLFormElement.requestSubmit()</c> when available so that validation runs and
        /// the <c>submit</c> event fires; otherwise falls back to dispatching a synthetic event.
        /// </summary>
        public Form Submit()
        {
            Script.Write("if (typeof {0}.requestSubmit === 'function') { {0}.requestSubmit(); } else { {0}.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true })); }", _form);
            return this;
        }

        public void Add(IComponent component) => _form.appendChild(component.Render());

        public void Clear() => ClearChildren(_form);

        public void Replace(IComponent newComponent, IComponent oldComponent)
            => _form.replaceChild(newComponent.Render(), oldComponent.Render());

        public HTMLElement Render() => _form;
    }
}
