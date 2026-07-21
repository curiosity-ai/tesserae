using System;
using System.Linq;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A drop target that accepts files dragged from the operating system, with hover and validation feedback.
    /// </summary>
    [Transpose.Name("tss.FileDropArea")]
    public sealed class FileDropArea : IComponent
    {
        private event FilesDroppedHandler FilesDropped;

        /// <summary>
        /// To accept multiple files, mark the FileDropArea as multiple.
        /// </summary>
        public delegate void FilesDroppedHandler(FileDropArea sender, File[] files);

        private readonly HTMLInputElement _fileInput;
        private          Raw              _raw;
        private readonly HTMLElement      _container;
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public FileDropArea()
        {
            _fileInput = FileInput(_("tss-file-input"));

            _container = CreateDefaultDropArea();

            _fileInput.onchange = (e) => triggerDroppedOnFile();

            void triggerDroppedOnFile()
            {
                if (_fileInput.files.length > 0)
                {
                    FilesDropped(this,
                        IsMultiple
                            ? _fileInput.files.ToArray()
                            : new[] { _fileInput.files.First() });
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public FileDropArea(IComponent component)
        {
            _fileInput = FileInput(_("tss-file-input"));

            _container = CreateWrappedDropArea(component);

            _fileInput.onchange = (e) => triggerDroppedOnFile();

            void triggerDroppedOnFile()
            {
                if (_fileInput.files.length > 0)
                {
                    FilesDropped(this,
                        IsMultiple
                            ? _fileInput.files.ToArray()
                            : new[] { _fileInput.files.First() });
                }
            }
        }

        /// <summary>
        /// Sets the content rendered inside the surface.
        /// </summary>
        public IComponent Content
        {
            set => _raw?.Content(value);
        }

        /// <summary>
        /// Opens the file selection.
        /// </summary>
        public void OpenFileSelection()
        {
            _fileInput.click();
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
        /// Returns a value indicating whether the component is multiple.
        /// </summary>
        public bool IsMultiple
        {
            get => _fileInput.multiple;
            set => _fileInput.multiple = value;
        }

        private HTMLElement CreateWrappedDropArea(IComponent component)
        {
            var wrapper = Div(_("tss-filedroparea-wrapper"));
            wrapper.appendChild(_fileInput);
            wrapper.appendChild(component.Render());

            var overlay = Div(_("tss-filedroparea-overlay"), Div(_("tss-filedroparea-message"), I(_($"{UIcons.Upload} tss-filedroparea-icon")), TextBlock("Drop files here").SemiBold().Render()));
            wrapper.appendChild(overlay);

            int dragCounter = 0;

            wrapper.ondragenter = (e) =>
            {
                StopEvent(e);
                dragCounter++;
                wrapper.classList.add("tss-dropping");
            };

            wrapper.ondragleave = (e) =>
            {
                StopEvent(e);
                dragCounter--;
                if (dragCounter <= 0)
                {
                    dragCounter = 0;
                    wrapper.classList.remove("tss-dropping");
                }
            };

            wrapper.ondragover = (e) =>
            {
                StopEvent(e); // necessary to allow drop
            };

            wrapper.ondrop = (e) =>
            {
                StopEvent(e);
                dragCounter = 0;
                wrapper.classList.remove("tss-dropping");

                foreach (var item in e.dataTransfer.items)
                {
                    if (item.kind != "file") continue;
                    OnReadEntry(item.webkitGetAsEntry());
                    if (!IsMultiple) break;
                }
            };

            return wrapper;
        }

        private HTMLElement CreateDefaultDropArea()
        {
            var dropArea = Div(_("tss-filedroparea"));
            dropArea.appendChild(_fileInput);

            _raw = Raw(Div(_("tss-filedroparea-message"), I(_($"{UIcons.Upload} tss-filedroparea-icon")), TextBlock("Drop files here or click to upload").SemiBold().Render()));

            dropArea.appendChild(_raw.Render());
            dropArea.onclick = (e) => { _fileInput.click(); };

            dropArea.ondragover = (e) =>
            {
                StopEvent(e);
                dropArea.classList.add("tss-dropping");
            };

            dropArea.ondragleave = (e) =>
            {
                StopEvent(e);
                dropArea.classList.remove("tss-dropping");
            };

            dropArea.ondrop = (e) =>
            {
                StopEvent(e);
                dropArea.classList.remove("tss-dropping");

                foreach (var item in e.dataTransfer.items)
                {
                    if (item.kind != "file") continue;
                    OnReadEntry(item.webkitGetAsEntry());
                    if (!IsMultiple) break;
                }
            };

            return dropArea;
        }

        private void ReadDirectory(object dir)
        {
            var              dirReader = Script.Write<object>("{0}.createReader()", dir);
            Action<object[]> readEnt   = OnReadEntries;
            Script.Write(@"{0}.readEntries({1});", dirReader, readEnt);
        }

        private void OnReadEntries(object[] entries)
        {
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                OnReadEntry(entry);
                if (!IsMultiple) break;
            }
        }

        private void OnReadEntry(object entry)
        {
            if (Script.Write<bool>("{0}.isDirectory", entry) == true)
            {
                ReadDirectory(entry);
            }
            else if (Script.Write<bool>("{0}.isFile", entry) == true)
            {
                Action<File> upload = (f) => { FilesDropped?.Invoke(this, new[] { f }); };
                Script.Write("{0}.file({1})", entry, upload);
            }
        }

        /// <summary>
        /// Registers a callback invoked when the files dropped event fires.
        /// </summary>
        public FileDropArea OnFilesDropped(FilesDroppedHandler handler)
        {
            FilesDropped += handler;
            return this;
        }

        /// <summary>
        /// Sets the content of the component.
        /// </summary>
        public FileDropArea SetContent(IComponent content)
        {
            Content = content;
            return this;
        }

        /// <summary>
        /// Sets the type of files accepted by this selector. See https://www.w3schools.com/tags/att_input_accept.asp for more information.
        /// Valid values are a list of extensions, like ".txt|.doc|.docx", of media type, such as  "audio/*|video/*|image/*", or a combination of both
        /// </summary>
        /// <param name="accepts"></param>
        /// <returns></returns>
        public FileDropArea SetAccepts(string accepts)
        {
            Accepts = accepts;
            return this;
        }

        /// <summary>
        /// Configures the component to multiple.
        /// </summary>
        public FileDropArea Multiple()
        {
            IsMultiple = true;
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
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            return _container;
        }
    }
}