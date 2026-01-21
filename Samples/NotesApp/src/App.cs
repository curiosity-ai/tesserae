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
        private const string StorageKey = "tss-notes-app-v3";

        private static void Main()
        {
            document.body.style.overflow = "hidden";
            Theme.Light();

            var notes = LoadNotes();
            var observableNotes = new ObservableList<Note>(notes.ToArray());
            var selectedNote = new SettableObservable<Note>(notes.FirstOrDefault());

            observableNotes.Observe(_ => SaveNotes(observableNotes.ToList()));

            var searchBox = SearchBox("Search notes...").SearchAsYouType().W(1).Grow();
            var filterObservable = new SettableObservable<string>("");
            searchBox.OnSearch((_, val) => filterObservable.Value = val);

            var noteList = Defer(observableNotes, filterObservable, (n, f) => Task.FromResult(RenderNoteList(observableNotes, selectedNote, f)));
            var noteEditor = Defer(selectedNote, n => Task.FromResult(RenderEditor(n, () => observableNotes.NotifyObservers(), () => {
                var toDelete = selectedNote.Value;
                selectedNote.Value = observableNotes.FirstOrDefault(x => x != toDelete);
                observableNotes.Remove(toDelete);
            })));

            var sidebar = VStack().S().Class("notes-sidebar").Children(
                HStack().P(16).AlignItemsCenter().Children(
                    TextBlock("My Notes").Large().Bold().W(1).Grow(),
                    Button().SetIcon(UIcons.Plus).Primary().OnClick((_, __) => {
                        var newNote = new Note { Id = Guid.NewGuid().ToString(), Title = "New Note", Content = "", Folder = "General", CreatedAt = DateTime.Now };
                        observableNotes.Insert(0, newNote);
                        selectedNote.Value = newNote;
                    })
                ),
                VStack().P(16).Children(searchBox),
                noteList.W(1).Grow()
            );

            var splitView = SplitView().LeftIsSmaller(300.px()).Left(sidebar).Right(noteEditor).S();

            document.body.appendChild(splitView.Render());
        }

        private static IComponent RenderNoteList(ObservableList<Note> notes, SettableObservable<Note> selectedNote, string filter)
        {
            var filtered = notes.Where(n => string.IsNullOrEmpty(filter) || n.Title.ToLower().Contains(filter.ToLower()) || n.Content.ToLower().Contains(filter.ToLower()))
                                .OrderByDescending(n => n.IsPinned)
                                .ThenByDescending(n => n.CreatedAt);

            var list = VStack().ScrollY().Grow().Class("note-items-list");

            foreach (var note in filtered)
            {
                var isSelected = selectedNote.Value?.Id == note.Id;
                var itemContent = VStack().P(12).Children(
                        HStack().AlignItemsCenter().Children(
                            TextBlock(note.Title).SemiBold().W(1).Grow().Class("text-ellipsis"),
                            If(note.IsPinned, Icon(UIcons.Thumbtack).Small().ML(8))
                        ),
                        TextBlock(note.Content.Length > 50 ? note.Content.Substring(0, 50) + "..." : note.Content)
                            .Small().Secondary().MT(4).Class("text-ellipsis")
                    );

                var item = Button().NoBackground().Class("note-item-btn").Class(isSelected ? "selected" : "").W(1).Grow().ReplaceContent(itemContent);
                item.OnClick((_, __) => selectedNote.Value = note);
                list.Add(item);
            }

            return list;
        }

        private static IComponent RenderEditor(Note note, Action onChanged, Action onDelete)
        {
            if (note == null) return CenteredTextBlock("Select or create a note");

            var titleEditor = TextBox(note.Title).W(1).Grow().Class("note-title-editor");
            var contentEditor = TextArea(note.Content).W(1).Grow().HS().Class("note-content-editor");

            titleEditor.OnInput((_, __) => { note.Title = titleEditor.Text; onChanged(); });
            contentEditor.OnInput((_, __) => { note.Content = contentEditor.Text; onChanged(); });

            return VStack().S().Children(
                HStack().P(16).AlignItemsCenter().Class("editor-toolbar").Children(
                    titleEditor,
                    Button().SetIcon(UIcons.Thumbtack).Class(note.IsPinned ? "pinned-btn" : "").ML(16).OnClick((_, __) => { note.IsPinned = !note.IsPinned; onChanged(); }),
                    Button().SetIcon(UIcons.Trash).Danger().ML(8).OnClick((_, __) => onDelete())
                ),
                contentEditor.P(32)
            );
        }

        private static List<Note> LoadNotes()
        {
            var json = localStorage.getItem(StorageKey);
            if (string.IsNullOrEmpty(json)) return new List<Note> { new Note { Id = "1", Title = "Welcome to Notes", Content = "Organize your thoughts here!", Folder = "General", CreatedAt = DateTime.Now } };
            try { return JsonConvert.DeserializeObject<List<Note>>(json); }
            catch { return new List<Note>(); }
        }

        private static void SaveNotes(List<Note> notes)
        {
            localStorage.setItem(StorageKey, JsonConvert.SerializeObject(notes));
        }

        private static IComponent CenteredTextBlock(string text) =>
            VStack().S().AlignCenter().JustifyCenter().Children(TextBlock(text).Medium().Secondary());
    }

    public class Note
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Folder { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public static class Extensions
    {
        public static void NotifyObservers<T>(this ObservableList<T> list)
        {
            var temp = list.ToList();
            list.ReplaceAll(temp);
        }
    }
}
