using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H5.Core;
using Tesserae;
using Newtonsoft.Json;
using static H5.Core.dom;
using static Tesserae.UI;

namespace NotesApp
{
    internal static class App
    {
        private const string StorageKey = "tss-notes-app-data";

        private static void Main()
        {
            document.body.style.overflow = "hidden";
            Theme.Light();

            var notes = LoadNotes();
            var observableNotes = new ObservableList<Note>(notes.ToArray());
            var selectedNote = new SettableObservable<Note>(notes.FirstOrDefault());

            observableNotes.Observe(_ => SaveNotes(observableNotes.ToList()));

            var noteList = Defer(observableNotes, n => Task.FromResult(RenderNoteList(observableNotes, selectedNote)));
            var noteEditor = Defer(selectedNote, n => Task.FromResult(RenderEditor(n, () => {
                SaveNotes(observableNotes.ToList());
            })));

            var splitView = SplitView().LeftIsSmaller(300.px()).Left(noteList).Right(noteEditor).S();

            document.body.appendChild(splitView.Render());
        }

        private static IComponent RenderNoteList(ObservableList<Note> notes, SettableObservable<Note> selectedNote)
        {
            var searchBox = SearchBox("Search notes...").SearchAsYouType();
            var list = VStack().ScrollY().Grow();

            void UpdateList(string filter = "")
            {
                list.Clear();
                foreach (var note in notes.Where(n => string.IsNullOrEmpty(filter) || n.Title.Contains(filter) || n.Content.Contains(filter)))
                {
                    var item = Button(note.Title).NoBackground().W(1).Grow().TextLeft();
                    item.OnClick((_, __) => selectedNote.Value = note);
                    list.Add(item);
                }
            }

            searchBox.OnSearch((_, val) => UpdateList(val));
            UpdateList();

            return VStack().S().Children(
                HStack().Padding(10.px()).Children(
                    TextBlock("Notes").Large().SemiBold().W(1).Grow(),
                    Button().SetIcon(UIcons.Plus).OnClick((_, __) => {
                        var newNote = new Note { Title = "New Note", Content = "", Id = Guid.NewGuid().ToString() };
                        notes.Add(newNote);
                        selectedNote.Value = newNote;
                        UpdateList();
                    })
                ),
                searchBox.Padding(10.px()),
                list
            );
        }

        private static IComponent RenderEditor(Note note, Action onChanged)
        {
            if (note == null) return CenteredTextBlock("Select a note to edit");

            var titleEditor = TextBox(note.Title).W(1).Grow();
            var contentEditor = TextArea(note.Content).W(1).Grow().HS();

            titleEditor.OnInput((_, __) => { note.Title = titleEditor.Text; onChanged(); });
            contentEditor.OnInput((_, __) => { note.Content = contentEditor.Text; onChanged(); });

            return VStack().S().Padding(20.px()).Children(
                HStack().AlignItemsCenter().Children(
                    titleEditor.Large().SemiBold(),
                    Button().SetIcon(UIcons.Trash).Danger().MarginLeft(8.px()).OnClick((_, __) => {
                        note.Title = "Deleted";
                        note.Content = "";
                        onChanged();
                    })
                ),
                contentEditor.MarginTop(20.px())
            );
        }

        private static List<Note> LoadNotes()
        {
            var json = localStorage.getItem(StorageKey);
            if (string.IsNullOrEmpty(json)) return new List<Note> { new Note { Title = "Welcome", Content = "Create your first note!", Id = Guid.NewGuid().ToString() } };
            try { return JsonConvert.DeserializeObject<List<Note>>(json); }
            catch { return new List<Note>(); }
        }

        private static void SaveNotes(List<Note> notes)
        {
            localStorage.setItem(StorageKey, JsonConvert.SerializeObject(notes));
        }

        private static IComponent CenteredTextBlock(string text) =>
            HStack().JustifyContent(ItemJustify.Center).AlignItemsCenter().S().Children(TextBlock(text).Medium().Secondary());
    }

    public class Note
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
