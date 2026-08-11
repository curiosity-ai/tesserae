using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.FormField")]
    public sealed class FormField : ComponentBase<FormField, HTMLElement>
    {
        private readonly string _id;
        private readonly string _label;
        private readonly IComponent _control;
        private string _description;
        private string _error;

        private readonly TextBlock _labelElement;
        private readonly TextBlock _descElement;
        private readonly TextBlock _errElement;

        public FormField(string label, IComponent control)
        {
            _id = $"tss-ff-{Guid.NewGuid():N}";
            _label = label;
            _control = control;

            var labelId = _id + "-label";
            var descId  = _id + "-desc";
            var errId   = _id + "-err";

            _labelElement = TextBlock(_label).Id(labelId);
            _descElement = TextBlock("").Id(descId);
            _errElement = TextBlock("").Id(errId).Foreground(Theme.Danger.Foreground);

            _descElement.Collapse();
            _errElement.Collapse();

            // Wire ARIA label
            var renderedControl = _control.Render();

            if (_control is IAccessibility accessibilityControl)
            {
                accessibilityControl.AriaLabelledBy = labelId;
            }
            else
            {
                renderedControl.setAttribute("aria-labelledby", labelId);
            }

            InnerElement = VStack().Children(_labelElement, _control, _descElement, _errElement).Render();
            AttachClick();
            AttachContextMenu();
        }

        public FormField Description(string text)
        {
            _description = text;
            _descElement.Text = text;
            if (string.IsNullOrEmpty(text))
            {
                _descElement.Collapse();
            }
            else
            {
                _descElement.Show();
            }
            UpdateAriaDescribedBy();
            return this;
        }

        public FormField Error(string text)
        {
            _error = text;
            _errElement.Text = text;
            if (string.IsNullOrEmpty(text))
            {
                _errElement.Collapse();

                var renderedControl = _control.Render();
                if (renderedControl != null)
                {
                    renderedControl.removeAttribute("aria-invalid");
                }
            }
            else
            {
                _errElement.Show();

                var renderedControl = _control.Render();
                if (renderedControl != null)
                {
                    renderedControl.setAttribute("aria-invalid", "true");
                }
            }
            UpdateAriaDescribedBy();
            return this;
        }

        private void UpdateAriaDescribedBy()
        {
            var descId  = _id + "-desc";
            var errId   = _id + "-err";

            string describedBy = "";
            if (!string.IsNullOrEmpty(_description) && !string.IsNullOrEmpty(_error))
            {
                describedBy = $"{descId} {errId}";
            }
            else if (!string.IsNullOrEmpty(_description))
            {
                describedBy = descId;
            }
            else if (!string.IsNullOrEmpty(_error))
            {
                describedBy = errId;
            }

            if (_control is IAccessibility accessibilityControl)
            {
                accessibilityControl.AriaDescribedBy = describedBy;
            }
            else
            {
                var renderedControl = _control.Render();
                if (renderedControl != null)
                {
                    if (string.IsNullOrEmpty(describedBy))
                    {
                        renderedControl.removeAttribute("aria-describedby");
                    }
                    else
                    {
                        renderedControl.setAttribute("aria-describedby", describedBy);
                    }
                }
            }
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
