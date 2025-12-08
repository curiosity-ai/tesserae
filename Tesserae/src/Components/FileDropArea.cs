using System;
using System.Linq;
using H5;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.FileDropArea")]
    public sealed class FileDropArea : IComponent
    {
        private event FilesDroppedHandler FilesDropped;
        public delegate void              FilesDroppedHandler(FileDropArea sender, File[] file);
        public delegate void              FileDroppedHandler(FileDropArea  sender, File   file);

        private readonly HTMLInputElement _fileInput;
        private          Raw              _raw;
        private readonly HTMLElement      _container;
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
                            ? new[] { _fileInput.files.First() }
                            : _fileInput.files.ToArray());
                }
            }
        }

        public IComponent Content
        {
            set => _raw.Content(value);
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

        public bool IsMultiple
        {
            get => _fileInput.multiple;
            set => _fileInput.multiple = value;
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


            void ReadDirectory(object dir)
            {
                var              dirReader = Script.Write<object>("{0}.createReader()", dir);
                Action<object[]> readEnt   = OnReadEntries;
                Script.Write(@"{0}.readEntries({1});", dirReader, readEnt);
            }

            void OnReadEntries(object[] entries)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    var entry = entries[i];
                    OnReadEntry(entry);
                    if (!IsMultiple) break;
                }
            }

            void OnReadEntry(object entry)
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

            return dropArea;
        }

        public FileDropArea OnFileDropped(FileDroppedHandler handler)
        {
            FilesDropped += (sender, files) =>
            {
                foreach (var file in files)
                {
                    handler(sender, file);
                }
            };
            return this;
        }

        public FileDropArea OnFilesDropped(FilesDroppedHandler handler)
        {
            FilesDropped += handler;
            return this;
        }

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

        public FileDropArea Multiple()
        {
            IsMultiple = true;
            return this;
        }

        public void Reset()
        {
            _fileInput.value = null;
        }

        public HTMLElement Render()
        {
            return _container;
        }
    }
}