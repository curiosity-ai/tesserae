using System;
using System.Collections.Generic;
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
            _fileInput = FileInput(Att("tss-file-input"));

            _container = CreateDefaultDropArea();

            _fileInput.onchange = (e) => OnFileInputChanged();
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public FileDropArea(IComponent component)
        {
            _fileInput = FileInput(Att("tss-file-input"));

            _container = CreateWrappedDropArea(component);

            _fileInput.onchange = (e) => OnFileInputChanged();
        }

        private void OnFileInputChanged()
        {
            if (_fileInput.files.length == 0) return;

            FilesDropped?.Invoke(this,
                IsMultiple
                    ? _fileInput.files.ToArray()
                    : new[] { _fileInput.files.First() });
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
            var wrapper = Div(Att("tss-filedroparea-wrapper"));
            wrapper.appendChild(_fileInput);

            _raw = Raw(component);
            wrapper.appendChild(_raw.Render());

            var overlay = Div(Att("tss-filedroparea-overlay"), Div(Att("tss-filedroparea-message"), I(Att($"{UIcons.Upload.ToCssClass()} tss-filedroparea-icon")), TextBlock("Drop files here").SemiBold().Render()));
            wrapper.appendChild(overlay);

            wrapper.onclick = (e) =>
            {
                if (IsOwnClickOfWrappedContent(e)) return;
                _fileInput.click();
            };

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

                OnDropped(e);
            };

            return wrapper;
        }

        // The wrapped content brings its own buttons and links, and the hidden file input re-dispatches
        // the click we open it with - those clicks belong to the element that was hit, not to browsing.
        private static bool IsOwnClickOfWrappedContent(MouseEvent e)
        {
            var target = e?.target.As<HTMLElement>();

            if (target is null) return false;

            return target.closest("a, button, input, select, textarea, label, [role=button], [role=link], [contenteditable=true]") is object;
        }

        private HTMLElement CreateDefaultDropArea()
        {
            var dropArea = Div(Att("tss-filedroparea"));
            dropArea.appendChild(_fileInput);

            _raw = Raw(Div(Att("tss-filedroparea-message"), I(Att($"{UIcons.Upload.ToCssClass()} tss-filedroparea-icon")), TextBlock("Drop files here or click to upload").SemiBold().Render()));

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

                OnDropped(e);
            };

            return dropArea;
        }

        // What a drop reports is every file in it, in one call - the same shape as picking them in the file dialog.
        // The entries have to be taken while the drop event is being handled (the browser empties the list afterwards), but
        // reading them is asynchronous, and a folder holds more files that are only found by reading it. So the entries
        // are walked until nothing is left to read, and only then is the handler told, once.
        private void OnDropped(DragEvent e)
        {
            var entries = new List<object>();

            foreach (var item in e.dataTransfer.items)
            {
                if (item.kind != "file") continue;

                var entry = item.webkitGetAsEntry();

                if (entry is object) entries.Add(entry);

                if (!IsMultiple) break;
            }

            if (entries.Count == 0) return;

            var found   = new List<DroppedFile>();
            var pending = 0;
            var order   = 0;

            // Every entry visited counts as pending until it was read; a folder's children are counted before the folder
            // is, so the count only reaches zero when the last file of the last folder has arrived.
            Action finished = () =>
            {
                pending--;

                if (pending > 0) return;

                //The files arrive in whatever order the browser reads them: hand them over in the order they were dropped.
                var files = found.OrderBy(f => f.Order).Select(f => f.File).ToArray();

                if (files.Length == 0) return;

                FilesDropped?.Invoke(this, IsMultiple ? files : new[] { files[0] });
            };

            Action<object> visit = null;

            visit = (entry) =>
            {
                pending++;

                var position = order++;

                if (Script.Write<bool>("{0}.isDirectory", entry) == true)
                {
                    ReadAllEntries(Script.Write<object>("{0}.createReader()", entry), new List<object>(), children =>
                    {
                        foreach (var child in children) visit(child);

                        finished();
                    });
                }
                else if (Script.Write<bool>("{0}.isFile", entry) == true)
                {
                    Action<File> read  = (f) => { found.Add(new DroppedFile(position, f)); finished(); };
                    Action<object> failed = (_) => finished();

                    Script.Write("{0}.file({1}, {2})", entry, read, failed);
                }
                else
                {
                    finished();
                }
            };

            foreach (var entry in entries) visit(entry);
        }

        // A directory reader hands over its entries in batches (Chromium: 100 at a time) and says it is done with an empty
        // one, so a folder with more files than a batch is only read completely by asking again until nothing comes back.
        private static void ReadAllEntries(object reader, List<object> read, Action<List<object>> done)
        {
            Action<object[]> onBatch = null;

            onBatch = (batch) =>
            {
                if (batch.Length == 0)
                {
                    done(read);
                    return;
                }

                for (var i = 0; i < batch.Length; i++) read.Add(batch[i]);

                Script.Write("{0}.readEntries({1}, {2})", reader, onBatch, new Action<object>(_ => done(read)));
            };

            Script.Write("{0}.readEntries({1}, {2})", reader, onBatch, new Action<object>(_ => done(read)));
        }

        private sealed class DroppedFile
        {
            public int  Order { get; }
            public File File  { get; }

            public DroppedFile(int order, File file)
            {
                Order = order;
                File  = file;
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