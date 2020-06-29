namespace Tesserae.Components
{
    public class TextBox : Input<TextBox>
    {
        public TextBox(string text = string.Empty)
            : base("text", text)
        {
        }

        public string Placeholder
        {
            get => InnerElement.placeholder;
            set => InnerElement.placeholder = value;
        }

        public bool IsReadOnly
        {
            get => InnerElement.hasAttribute("readonly");
            set
            {
                if (value)
                {
                    InnerElement.setAttribute("readonly", string.Empty);
                }
                else
                {
                    InnerElement.removeAttribute("readonly");
                }
            }
        }

        // Could have a separate type for Password?
        public bool IsPassword
        {
            get => InnerElement.type == "password";
            set
            {
                if (value)
                {
                    InnerElement.type = "password";
                }
                else
                {
                    InnerElement.type = string.Empty;
                }
            }
        }

        public TextBox SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;
            return this;
        }

        public TextBox ReadOnly()
        {
            IsReadOnly = true;
            return this;
        }

        public TextBox Password()
        {
            IsPassword = true;
            return this;
        }
    }
}
