﻿using System;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class EditableArea : ComponentBase<EditableArea, HTMLTextAreaElement>, IHasTextSize
    {
        protected readonly HTMLDivElement _container;

        protected readonly HTMLSpanElement _labelText;

        protected          HTMLElement    _editIcon;
        protected          HTMLElement    _cancelEditIcon;
        protected readonly HTMLDivElement _editView;
        protected readonly HTMLDivElement _labelView;

        public delegate bool SaveEditHandler(EditableArea sender, string newValue);

        public event SaveEditHandler onSave;

        private bool _isCanceling = false;

        public TextSize Size
        {
            get
            {
                return TextSizeExtensions.FromClassList(InnerElement, TextSize.Small);
            }
            set
            {
                string current = Size.ToClassName();
                InnerElement.classList.remove(current);
                _labelText.classList.remove(current);
                _editIcon.classList.remove(current);
                _cancelEditIcon.classList.remove(current);
                
                string newValue = value.ToClassName();
                InnerElement.classList.add(newValue);
                _labelText.classList.add(newValue);
                _editIcon.classList.add(newValue);
                _cancelEditIcon.classList.add(newValue);
            }
        }

        public TextWeight Weight
        {
            get
            {
                return TextSizeExtensions.FromClassList(InnerElement, TextWeight.Regular);
            }
            set
            {
                InnerElement.classList.remove(Weight.ToClassName());
                _labelText.classList.remove(Weight.ToClassName());
                InnerElement.classList.add(value.ToClassName());
                _labelText.classList.add(value.ToClassName());
            }
        }

        public EditableArea(string text = string.Empty)
        {
            _labelText  = Span(_("tss-editablelabel-textspan", text: text, title: "Click to edit"));
            _editIcon   = I(_("tss-editablelabel-edit-icon far fa-edit"));
            _labelView  = Div(_("tss-editablelabel-displaybox"), _labelText, _editIcon);

            InnerElement     = TextArea(_("tss-editablelabel-textbox", type: "text"));
            _cancelEditIcon  = Div(_("tss-editablelabel-cancel-icon", title:"Cancel edit"), I(_("far fa-times")));
            _editView        = Div(_("tss-editablelabel-editbox"), InnerElement, _cancelEditIcon);

            _container = Div(_("tss-editablelabel"), _labelView, _editView);

            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();

            _labelView.addEventListener("click",      BeginEditing);
            _cancelEditIcon.addEventListener("click", CancelEditing);

            OnKeyUp(KeyUp);
            OnBlur(BeginSaveEditing);
        }

        private void KeyUp(EditableArea sender, KeyboardEvent e)
        {
            if(e.key == "Escape")
            {
                CancelEditing(sender);
            }
        }

        public bool IsEditingMode
        {
            get { return _container.classList.contains("editing"); }
            set
            {
                if (value)
                {
                    var labelRect = (DOMRect)_labelText.getBoundingClientRect();
                    InnerElement.style.minWidth = (labelRect.width * 1.2) + "px";
                    InnerElement.style.minHeight = (labelRect.height * 1.2) + "px";
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
            return _editView.hidden ? _labelView : _editView;
        }

        public EditableArea OnSave(SaveEditHandler onSave)
        {
            this.onSave += onSave;
            return this;
        }

        protected void BeginEditing(object sender)
        {
            InnerElement.value = _labelText.textContent;
            IsEditingMode = true;
            _isCanceling = false;
            InnerElement.focus();
        }

        protected void CancelEditing(object sender)
        {
            _isCanceling = true;
            IsEditingMode = false;
            InnerElement.blur();
        }

        private void BeginSaveEditing(EditableArea sender, Event e)
        {
            //We need to do this on a timeout, because clicking on the Cancel would trigger this method first, 
            //with no opportunity to cancel
            window.setTimeout(SaveEditing, 150);
        }

        private void SaveEditing(object e)
        {
            if (_isCanceling) return;
            
            var newValue = InnerElement.value;

            if(onSave is null || onSave(this, newValue))
            {
                _labelText.textContent = newValue;
                IsEditingMode = false;
            }
            else
            {
                InnerElement.focus();
            }
        }

        public override HTMLElement Render()
        {
            return _container;
        }
    }
}