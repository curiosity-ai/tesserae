﻿using System;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class EditableLabel : ComponentBase<EditableLabel, HTMLInputElement>
    {
        protected readonly HTMLDivElement _container;

        protected readonly HTMLSpanElement _textSpan;

        protected          HTMLElement    _iconSpan;
        protected          HTMLElement    _closeEditIconSpan;
        protected readonly HTMLDivElement _editDiv;
        protected readonly HTMLDivElement _displayDiv;


        public EditableLabel(string text = string.Empty)
        {
            _textSpan   = Span(_("tss-editablelabel-textspan", text: text));
            _iconSpan   = I(_("tss-editablelabel-edit-indicator-icon" + " fas fa-edit"));
            _displayDiv = Div(_("tss-editablelabel-displaybox"), _textSpan, _iconSpan);


            InnerElement       = TextBox(_("tss-editablelabel-textbox", type: "text", value: text));
            _closeEditIconSpan = I(_("tss-editablelabel-edit-close-icon" + " fas fa-times-circle"));
            _editDiv           = Div(_("tss-editablelabel-editbox"), InnerElement, _closeEditIconSpan);

            _container = Div(_("tss-editablelabel"), _displayDiv, _editDiv);

            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();

            _displayDiv.addEventListener("click", ClickHandler);
            _closeEditIconSpan.addEventListener("click", ClickHandler);


            this.OnChange((s, e) => _textSpan.textContent = InnerElement.value);
        }


        public bool IsEditingMode
        {
            get { return _container.classList.contains("editing"); }
            set
            {
                if (value)
                {
                    _container.classList.add("editing");
                }
                else
                {
                    _container.classList.remove("editing");
                }
            }
        }

        private HTMLDivElement ShownElement()
        {
            return _editDiv.hidden ? _displayDiv : _editDiv;
        }


        protected void ClickHandler(object sender)
        {
            IsEditingMode = !IsEditingMode;
        }

        public override HTMLElement Render()
        {
            return _container;
        }
    }
}