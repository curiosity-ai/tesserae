using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A form control that lets the user pick one or more files from disk, with optional drag-and-drop support.
    /// </summary>
    [H5.Name("tss.FileSelector")]
    public sealed class FileSelector : IComponent, ICanValidate<FileSelector>, IObservableComponent<File>
    {
        public delegate void              FileSelectedHandler(FileSelector sender, File file);
        private event FileSelectedHandler FileSelected;

        private readonly HTMLInputElement         _fileInput;
        private readonly IComponent               _stack;
        private readonly TextBox                  _textBox;
        private readonly HTMLElement              _container;
        private readonly SettableObservable<File> _observable = new SettableObservable<File>();
        private          File                     _selectedFile;

        /// <summary>
        /// Gets or sets the selected file.
        /// </summary>
        public File SelectedFile
        {
            get => _selectedFile;
            private set
            {
                _selectedFile     = value;
                _observable.Value = value;
                FileSelected?.Invoke(this, value);
            }
        }

        /// <summary>
        /// Returns an observable that tracks the currently selected file (read-only — browser security
        /// prevents pushing a File value back into the input).
        /// </summary>
        public IObservable<File> AsObservable() => _observable;

        /// <summary>
        /// Gets or sets the placeholder text shown when the component is empty.
        /// </summary>
        public string Placeholder
        {
            get => _textBox.Placeholder;
            set => _textBox.Placeholder = value;
        }

        /// <summary>
        /// Gets or sets the validation error message displayed beneath the component.
        /// </summary>
        public string Error
        {
            get => _textBox.Error;
            set => _textBox.Error = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component is currently in an invalid state.
        /// </summary>
        public bool IsInvalid
        {
            get => _textBox.IsInvalid;
            set => _textBox.IsInvalid = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component is required for form submission.
        /// </summary>
        public bool IsRequired
        {
            get => _textBox.IsRequired;
            set => _textBox.IsRequired = value;
        }


        /// <summary>
        /// Gets or sets the type of files accepted by this selector. See https://www.w3schools.com/tags/att_input_accept.asp for more information.
        /// Valid values are a list of extensions, like ".txt|.doc|.docx", of media type, such as  "audio/*|video/*|image/*", or a combination of both
        /// </summary>
        public string Accepts
        {
            get => _fileInput.accept;
            set => _fileInput.accept = value;
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public FileSelector()
        {
            _fileInput = FileInput(_("tss-file-input"));
            _textBox   = TextBox().ReadOnly().Grow(1).AlignCenter().Class("tss-file-input-text");

            _stack = HStack().WS()
               .Children(_textBox,
                    Button().SetTitle("Click to select file...").NoWrap().SetIcon(UIcons.Folder).OnClick((s, e) => _fileInput.click()).NoBorder().NoBackground(),
                    Raw(_fileInput));

            _fileInput.onchange = _ => updateFile();

            _container = Div(_("tss-fileselector"), _stack.Render());

            void updateFile()
            {
                if (_fileInput.files.length > 0)
                {
                    SelectedFile  = _fileInput.files[0];
                    _textBox.Text = GetFileName(_fileInput.value);
                }
            }
            ;
        }

        /// <summary>
        /// Removes / disables the text box on the component.
        /// </summary>
        public FileSelector NoTextBox()
        {
            _textBox.Collapse();
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the file selected event fires.
        /// </summary>
        public FileSelector OnFileSelected(FileSelectedHandler handler)
        {
            FileSelected += handler;
            return this;
        }

        /// <summary>
        /// Sets the placeholder of the component.
        /// </summary>
        public FileSelector SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;
            return this;
        }

        /// <summary>
        /// Sets the type of files accepted by this selector. See https://www.w3schools.com/tags/att_input_accept.asp for more information.
        /// Valid values are a list of extensions, like ".txt|.doc|.docx", of media type, such as  "audio/*|video/*|image/*", or a combination of both
        /// </summary>
        /// <param name="accepts"></param>
        /// <returns></returns>
        public FileSelector SetAccepts(string accepts)
        {
            Accepts = accepts;
            return this;
        }

        /// <summary>
        /// Marks the component as required.
        /// </summary>
        public FileSelector Required()
        {
            IsRequired = true;
            return this;
        }

        /// <summary>
        /// Resets the component to its initial state.
        /// </summary>
        public void Reset()
        {
            _fileInput.value = null;
        }

        /// <summary>
        /// Attaches a handler to the component's value-changed event.
        /// </summary>
        public void Attach(ComponentEventHandler<FileSelector> handler)
        {
            FileSelected += (s, _) => handler(s);
        }

        private string GetFileName(string value)
        {
            var lastSep = value.LastIndexOfAny(new[] { '/', '\\' });
            return value.Substring(lastSep + 1);
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            return _container;
        }
    }
}