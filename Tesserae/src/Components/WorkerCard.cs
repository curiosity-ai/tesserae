using Transpose;
using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using TNT;
using static TNT.T;

namespace Tesserae
{
    /// <summary>One worker listed in a <see cref="WorkerCard"/>: a piece of work handed to an agent.</summary>
    public sealed class WorkerCardItem
    {
        /// <summary>The stable key of the worker. Rows are matched by it, so an update only touches the row that changed.</summary>
        public string                Id         { get; set; }
        /// <summary>What the worker is doing: the task's name.</summary>
        public string                Title      { get; set; }
        /// <summary>A quiet line under the name - the name of the agent doing it, say.</summary>
        public string                Subtitle   { get; set; }
        /// <summary>An optional column naming who does it, drawn between the title and the status. None draws no column.</summary>
        public string                Agent      { get; set; }
        /// <summary>How the status at the end of the row is drawn: a spinner while running, a check once done.</summary>
        public ConversationStateTone Tone       { get; set; }
        /// <summary>The status text ("Working", "In review").</summary>
        public string                StateLabel { get; set; }
        /// <summary>An icon drawn at the start of the row.</summary>
        public UIcons?               Icon       { get; set; }
        /// <summary>The icon's colour, as any CSS colour. None uses the text colour.</summary>
        public string                IconColor  { get; set; }
        /// <summary>A small component drawn in place of the icon - an agent's logo. Wins over <see cref="Icon"/>.</summary>
        public IComponent            Leading    { get; set; }
        /// <summary>A component drawn after the status - a "Review" button, say. Rendered once per update of the item.</summary>
        public IComponent            Trailing   { get; set; }
        /// <summary>Whatever the host wants to find again when the row is clicked.</summary>
        public object                Data       { get; set; }

        internal string Signature() => Title + "\u0001" + Subtitle + "\u0001" + Agent + "\u0001" + Tone + "\u0001" + StateLabel + "\u0001" + (Icon.HasValue ? Icon.Value.ToString() : "") + "\u0001" + IconColor;
    }

    /// <summary>
    /// A card that says an assistant handed work to others: a header ("Started 3 workers") with a quiet note at its
    /// end, and a row per worker - the logo of whoever does it, the task and the branch it works in, the agent's name
    /// and a live status. It sits inline in a chat, under the message that announces the work.
    /// <para>
    /// Rows are matched by <see cref="WorkerCardItem.Id"/>, so <see cref="UpdateItem"/> changes one status without
    /// redrawing the others, and the spinner of a worker that is still going keeps turning.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.WorkerCard")]
    public sealed class WorkerCard : IComponent
    {
        private sealed class Row
        {
            public WorkerCardItem Item;
            public string         Signature;
            public HTMLElement    Element;
            public HTMLElement    IconHost;
            public HTMLElement    Title;
            public HTMLElement    Subtitle;
            public HTMLElement    Agent;
            public HTMLElement    Status;
            public HTMLElement    StatusGlyph;
            public HTMLElement    StatusText;
            public HTMLElement    TrailingHost;
        }

        private readonly HTMLElement _root;
        private readonly HTMLElement _iconHost;
        private readonly HTMLElement _title;
        private readonly HTMLElement _hint;
        private readonly HTMLElement _rows;

        private readonly List<WorkerCardItem>    _items   = new List<WorkerCardItem>();
        private readonly Dictionary<string, Row> _rowById = new Dictionary<string, Row>();

        private Action<WorkerCardItem> _onSelect;

        /// <summary>Initializes a worker card.</summary>
        /// <param name="title">The header, e.g. "Started 3 workers".</param>
        public WorkerCard(string title = null)
        {
            _iconHost = Div(Att("tss-workercard-icon"));
            _iconHost.appendChild(I(UIcons.CodeFork));

            _title = Div(Att("tss-workercard-title"));
            _title.textContent = title ?? "";

            _hint = Div(Att("tss-workercard-hint"));
            _hint.style.display = "none";

            _rows = Div(Att("tss-workercard-rows"));
            _rows.setAttribute("role", "list");

            _root = Div(Att("tss-workercard"), Div(Att("tss-workercard-header"), _iconHost, _title, _hint), _rows);
            _root.setAttribute("role", "group");
        }

        /// <inheritdoc />
        public HTMLElement Render() => _root;

        /// <summary>The workers the card lists, in order.</summary>
        public IReadOnlyList<WorkerCardItem> Items => _items;

        /// <summary>Sets the header's text.</summary>
        public WorkerCard Title(string title)
        {
            _title.textContent = title ?? "";
            return this;
        }

        /// <summary>Sets the quiet note at the end of the header ("Each in its own worktree"). None hides it.</summary>
        public WorkerCard Hint(string hint)
        {
            _hint.textContent   = hint ?? "";
            _hint.style.display = string.IsNullOrEmpty(hint) ? "none" : "";
            return this;
        }

        /// <summary>Sets the icon in the header.</summary>
        public WorkerCard HeaderIcon(UIcons icon)
        {
            while (_iconHost.firstChild is object) _iconHost.removeChild(_iconHost.firstChild);
            _iconHost.appendChild(I(icon));
            return this;
        }

        /// <summary>Runs when a worker's row is clicked or picked with Enter.</summary>
        public WorkerCard OnSelect(Action<WorkerCardItem> onSelect)
        {
            _onSelect = onSelect;
            Refresh();
            return this;
        }

        /// <summary>Replaces every worker. Rows whose worker did not change are kept as they are.</summary>
        public WorkerCard SetItems(IEnumerable<WorkerCardItem> items)
        {
            _items.Clear();

            var seen = new HashSet<string>();

            foreach (var item in items ?? Enumerable.Empty<WorkerCardItem>())
            {
                if (item?.Id is null || !seen.Add(item.Id)) continue;
                _items.Add(item);
            }

            Refresh();
            return this;
        }

        /// <summary>Replaces the worker with the same id, or adds it at the end when the card does not list it.</summary>
        public WorkerCard UpdateItem(WorkerCardItem item)
        {
            if (item?.Id is null) return this;

            var index = _items.FindIndex(i => i.Id == item.Id);

            if (index >= 0) _items[index] = item;
            else            _items.Add(item);

            Refresh();
            return this;
        }

        private void Refresh()
        {
            var keep     = new HashSet<string>();
            var expected = _rows.firstElementChild.As<HTMLElement>();

            foreach (var item in _items)
            {
                keep.Add(item.Id);

                if (!_rowById.TryGetValue(item.Id, out var row))
                {
                    row = CreateRow(item);
                    _rowById[item.Id] = row;
                }
                else if (row.Signature != item.Signature() || !ReferenceEquals(row.Item.Trailing, item.Trailing) || !ReferenceEquals(row.Item.Leading, item.Leading))
                {
                    UpdateRow(row, item);
                }
                else
                {
                    row.Item = item;
                }

                if (expected != row.Element) _rows.insertBefore(row.Element, expected);
                else                         expected = expected.nextElementSibling.As<HTMLElement>();
            }

            foreach (var id in _rowById.Keys.ToArray())
            {
                if (keep.Contains(id)) continue;

                _rowById[id].Element.remove();
                _rowById.Remove(id);
            }

            //A card for one worker reads as a sentence, not a list: no header rule when there is nothing under it.
            _root.classList.toggle("tss-workercard-empty", _items.Count == 0);

            foreach (var row in _rowById.Values) row.Element.classList.toggle("tss-workercard-row-clickable", _onSelect is object);
        }

        private Row CreateRow(WorkerCardItem item)
        {
            var row = new Row
            {
                IconHost     = Div(Att("tss-workercard-row-icon")),
                Title        = Div(Att("tss-workercard-row-title")),
                Subtitle     = Div(Att("tss-workercard-row-subtitle")),
                Agent        = Div(Att("tss-workercard-row-agent")),
                StatusGlyph  = Div(Att("tss-workercard-row-glyph")),
                StatusText   = Div(Att("tss-workercard-row-state-text")),
                TrailingHost = Div(Att("tss-workercard-row-trailing")),
            };

            row.Status = Div(Att("tss-workercard-row-state"), row.StatusGlyph, row.StatusText);

            var main = Div(Att("tss-workercard-row-main"), row.Title, row.Subtitle);

            row.Element = Div(Att("tss-workercard-row"), row.IconHost, main, row.Agent, row.Status, row.TrailingHost);
            row.Element.setAttribute("role", "listitem");

            row.Element.addEventListener("click", (Event e) =>
            {
                //A button in the row (Review) is its own action.
                if (row.TrailingHost.contains(e.target.As<Node>())) return;
                _onSelect?.Invoke(row.Item);
            });

            row.Element.addEventListener("keydown", (Event e) =>
            {
                var ke = e.As<KeyboardEvent>();

                if (_onSelect is null || row.TrailingHost.contains(ke.target.As<Node>())) return;

                if (ke.key == "Enter" || ke.key == " ")
                {
                    e.preventDefault();
                    _onSelect(row.Item);
                }
            });

            UpdateRow(row, item);

            return row;
        }

        private void UpdateRow(Row row, WorkerCardItem item)
        {
            row.Item      = item;
            row.Signature = item.Signature();

            row.Element.dataset["workerId"] = item.Id;
            row.Element.setAttribute("tabindex", "0");

            row.Title.textContent      = item.Title ?? "";
            row.Subtitle.textContent   = item.Subtitle ?? "";
            row.Subtitle.style.display = string.IsNullOrEmpty(item.Subtitle) ? "none" : "";
            row.Agent.textContent      = item.Agent ?? "";

            while (row.IconHost.firstChild is object) row.IconHost.removeChild(row.IconHost.firstChild);

            if (item.Leading is object)
            {
                row.IconHost.appendChild(item.Leading.Render());
            }
            else if (item.Icon.HasValue)
            {
                var icon = I(item.Icon.Value);
                if (!string.IsNullOrEmpty(item.IconColor)) icon.style.color = item.IconColor;
                row.IconHost.appendChild(icon);
            }

            row.Status.className = "tss-workercard-row-state tss-workercard-row-state-" + item.Tone.ToString().ToLowerInvariant();
            row.StatusText.textContent = item.StateLabel ?? "";

            while (row.StatusGlyph.firstChild is object) row.StatusGlyph.removeChild(row.StatusGlyph.firstChild);

            switch (item.Tone)
            {
                case ConversationStateTone.Success:   row.StatusGlyph.appendChild(I(UIcons.CheckCircle));     break;
                case ConversationStateTone.Attention: row.StatusGlyph.appendChild(I(UIcons.TriangleWarning)); break;
                case ConversationStateTone.Danger:    row.StatusGlyph.appendChild(I(UIcons.CrossCircle));     break;
            }

            while (row.TrailingHost.firstChild is object) row.TrailingHost.removeChild(row.TrailingHost.firstChild);

            if (item.Trailing is object)
            {
                row.TrailingHost.appendChild(item.Trailing.Render());
                row.TrailingHost.style.display = "";
            }
            else
            {
                row.TrailingHost.style.display = "none";
            }
        }
    }
}
