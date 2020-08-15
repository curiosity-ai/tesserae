using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public sealed class EditableArea : ComponentBase<EditableArea, HTMLTextAreaElement>, ITextFormating, IBindableComponent<string>
    {
        private event SaveEditHandler Saved;

        public delegate bool SaveEditHandler(EditableArea sender, string newValue);

        private readonly HTMLDivElement  _container;
        private readonly HTMLSpanElement _labelText;
        private readonly HTMLDivElement  _editView;
        private readonly HTMLDivElement  _labelView;


        private SettableObservable<string>           _observable;
        private ObservableEvent.ValueChanged<string> valueGetter;
        private bool                                 _observableReferenceUsed = false;

        public SettableObservable<string> Observable
        {
            get
            {
                _observableReferenceUsed = true;
                return _observable;
            }
            set
            {
                if (_observableReferenceUsed)
                {
                    throw new ArgumentException("Can't set the observable after a reference of it has been used! (.AsObservable() might have been called before .Bind())");
                }

                if (_observable is object)
                    _observable.StopObserving(valueGetter);
                _observable = value;
                _observable.Observe(valueGetter);
            }
        }

        private readonly HTMLElement _editIcon;
        private readonly HTMLElement _cancelEditIcon;

        private bool _isCanceling = false;

        public EditableArea(string text = string.Empty)
        {
            _labelText = Span(_("tss-editablelabel-textspan", text: text, title: "Click to edit"));
            _editIcon  = I(_("tss-editablelabel-edit-icon las la-edit"));
            _labelView = Div(_("tss-editablelabel-displaybox"), _labelText, _editIcon);

            InnerElement    = TextArea(_("tss-editablelabel-textbox", type: "text"));
            _cancelEditIcon = Div(_("tss-editablelabel-cancel-icon",  title: "Cancel edit"), I(_("las la-times")));
            _editView       = Div(_("tss-editablelabel-editbox"),                            InnerElement, _cancelEditIcon);

            _container = Div(_("tss-editablelabel"), _labelView, _editView);

            valueGetter = value => SetText(value);
            Observable  = new SettableObservable<string>(text);
            
            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();

            _labelView.addEventListener("click", BeginEditing);
            _cancelEditIcon.addEventListener("click", CancelEditing);

            OnKeyUp((_, e) =>
            {
                if (e.key == "Escape")
                {
                    CancelEditing();
                }
            });

            OnBlur((_, __) => window.setTimeout(SaveEditing, 150)); // We need to do this on a timeout, because clicking on the Cancel would trigger this method first, with no opportunity to cancel
        }

        public TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
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
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToClassName());
                _labelText.classList.remove(Weight.ToClassName());
                InnerElement.classList.add(value.ToClassName());
                _labelText.classList.add(value.ToClassName());
            }
        }

        public TextAlign TextAlign
        {
            get
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-textalign-"));
                if (curFontSize is object && Enum.TryParse<TextAlign>(curFontSize.Substring("tss-textalign-".Length), true, out var result))
                {
                    return result;
                }
                else
                {
                    return TextAlign.Left;
                }
            }
            set
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-textalign-"));
                if (curFontSize is object)
                {
                    InnerElement.classList.remove(curFontSize);
                }

                InnerElement.classList.add($"tss-textalign-{value.ToString().ToLower()}");
            }
        }

        public bool IsEditingMode
        {
            get => _container.classList.contains("tss-editing");
            set
            {
                if (value)
                {
                    var labelRect = (DOMRect) _labelText.getBoundingClientRect();
                    InnerElement.style.minWidth  = (labelRect.width  * 1.2) + "px";
                    InnerElement.style.minHeight = (labelRect.height * 1.2) + "px";
                    _container.classList.add("tss-editing");
                }
                else
                {
                    _container.classList.remove("tss-editing");
                }
            }
        }

        public EditableArea OnSave(SaveEditHandler onSave)
        {
            Saved += onSave;
            return this;
        }

        private void BeginEditing()
        {
            InnerElement.value = _labelText.textContent;
            IsEditingMode      = true;
            _isCanceling       = false;
            InnerElement.focus();
        }

        private void CancelEditing()
        {
            _isCanceling  = true;
            IsEditingMode = false;
            InnerElement.blur();
        }

        private void SaveEditing(object e)
        {
            if (_isCanceling) return;

            var newValue = InnerElement.value;

            if (newValue != _labelText.textContent)
            {
                if (Saved is null || Saved(this, newValue))
                {
                    _labelText.textContent = newValue;
                    _observable.Value      = newValue;
                    IsEditingMode          = false;
                }
                else
                {
                    InnerElement.focus();
                }
            }
        }

        public EditableArea SetText(string text)
        {
            if (IsEditingMode)
            {
                InnerElement.value = text;
            }
            else
            {
                _labelText.textContent = text;
            }

            _observable.Value = text;

            return this;
        }

        public override HTMLElement Render() => _container;
    }
}