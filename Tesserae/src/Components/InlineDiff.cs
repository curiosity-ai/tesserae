using System;
using System.Collections.Generic;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A small, self-contained diff for the middle of a conversation: what one step of an assistant changed in a
    /// file, drawn compactly enough to sit between a tool call and the reply that follows it.
    /// <para>
    /// It reads a unified diff (what <c>git diff</c> prints, one file or several) or works one out from the
    /// before and after text, and draws it in plain C# - no script is loaded. Each file gets a header with its
    /// name and the lines added and removed, then the changed lines in a tinted gutter layout, with the part of a
    /// replaced line that actually changed marked inside it. A diff longer than <see cref="MaxLines"/> shows its
    /// first lines and a "Show N more lines" row; the header can fold the whole thing away.
    /// </para>
    /// <para>
    /// For a full-size review surface - side by side, syntax highlighting - use <see cref="CodeDiff"/> instead.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.InlineDiff")]
    public sealed class InlineDiff : ComponentBase<InlineDiff, HTMLElement>
    {
        /// <summary>The default number of lines shown before a long diff folds.</summary>
        public const int DEFAULT_MAX_LINES = 14;

        private const int CONTEXT_LINES = 3;

        // Past this many lines on a side the diff is not worked out line by line (the table would be
        // MAX x MAX cells) and the middle of the file is reported as replaced as a whole.
        private const int MAX_COMPARED_LINES = 1500;

        private const char ADDED   = '+';
        private const char REMOVED = '-';
        private const char SAME    = ' ';
        private const char HUNK    = '@';

        private string                _diff;
        private int                   _maxLines     = DEFAULT_MAX_LINES;
        private bool                  _lineNumbers  = true;
        private bool                  _wrap         = true;
        private bool                  _collapsible;
        private bool                  _collapsed;
        private string                _fileName;
        private List<InlineDiffFile>  _files        = new List<InlineDiffFile>();

        /// <summary>
        /// Initializes a new <see cref="InlineDiff"/> for a unified diff.
        /// </summary>
        /// <param name="diff">The diff, in unified format. Header lines (<c>diff --git</c>, <c>---</c>, <c>+++</c>) are optional for a single file; without them the diff has no name.</param>
        public InlineDiff(string diff = "")
        {
            InnerElement = Div(Att("tss-inlinediff"));
            _diff        = diff ?? string.Empty;
            Redraw();
        }

        /// <summary>
        /// Initializes a new <see cref="InlineDiff"/> that compares two versions of a text.
        /// </summary>
        /// <param name="before">The text before the change. Empty or null for a file that did not exist.</param>
        /// <param name="after">The text after the change. Empty or null for a file that was removed.</param>
        /// <param name="fileName">The name shown in the header.</param>
        public InlineDiff(string before, string after, string fileName = null) : this(Compare(before, after, fileName)) { }

        /// <summary>Gets or sets the unified diff drawn by the component.</summary>
        public string DiffText
        {
            get => _diff;
            set { _diff = value ?? string.Empty; Redraw(); }
        }

        /// <summary>The number of lines the diff adds, over every file in it.</summary>
        public int Added
        {
            get { var total = 0; foreach (var file in _files) total += file.Added; return total; }
        }

        /// <summary>The number of lines the diff removes, over every file in it.</summary>
        public int Removed
        {
            get { var total = 0; foreach (var file in _files) total += file.Removed; return total; }
        }

        /// <summary>The number of lines the diff changes: those it adds plus those it removes.</summary>
        public int ChangedLines => Added + Removed;

        /// <summary>The number of lines shown before a long diff folds.</summary>
        public int MaxLines => _maxLines;

        /// <summary>
        /// Sets how many diff lines show before the rest fold behind a "Show N more lines" row. A value
        /// of zero or less shows every line. Defaults to <see cref="DEFAULT_MAX_LINES"/>.
        /// </summary>
        public InlineDiff SetMaxLines(int lines)
        {
            _maxLines = lines;
            Redraw();
            return this;
        }

        /// <summary>Shows every line, however long the diff is.</summary>
        public InlineDiff ShowAllLines() => SetMaxLines(0);

        /// <summary>Shows or hides the old and new line numbers in the gutter. On by default.</summary>
        public InlineDiff LineNumbers(bool value = true)
        {
            _lineNumbers = value;
            Redraw();
            return this;
        }

        /// <summary>Lets a long line scroll sideways instead of wrapping inside the diff.</summary>
        public InlineDiff NoWrap(bool value = true)
        {
            _wrap = !value;
            InnerElement.UpdateClassIf(!_wrap, "tss-inlinediff-nowrap");
            return this;
        }

        /// <summary>
        /// Sets the name shown in the header of a diff that carries none of its own (one written without
        /// <c>---</c>/<c>+++</c> lines). A name the diff does carry wins.
        /// </summary>
        public InlineDiff FileName(string fileName)
        {
            _fileName = fileName;
            Redraw();
            return this;
        }

        /// <summary>
        /// Makes the file header fold the lines away when clicked. Off by default, so a small diff is simply
        /// there.
        /// </summary>
        public InlineDiff Collapsible(bool value = true)
        {
            _collapsible = value;
            Redraw();
            return this;
        }

        /// <summary>Starts with the lines folded away. Implies <see cref="Collapsible"/>.</summary>
        public InlineDiff Collapsed(bool value = true)
        {
            _collapsed = value;
            if (value) _collapsible = true;
            Redraw();
            return this;
        }

        /// <summary>
        /// Works out the unified diff between two texts, with three lines of context around every change.
        /// The result is empty when the texts are the same.
        /// </summary>
        /// <param name="before">The text before the change. Empty or null for a file that did not exist.</param>
        /// <param name="after">The text after the change. Empty or null for a file that was removed.</param>
        /// <param name="fileName">The name to put in the diff's header, or null for a diff with no header.</param>
        public static string Compare(string before, string after, string fileName = null)
        {
            var oldLines = SplitLines(before);
            var newLines = SplitLines(after);
            var script   = EditScript(oldLines, newLines);
            var hunks    = new List<string>();
            var changed  = false;

            foreach (var op in script) if (op.Kind != SAME) { changed = true; break; }

            if (!changed) return string.Empty;

            var index = 0;

            while (index < script.Count)
            {
                if (script[index].Kind == SAME) { index++; continue; }

                // A hunk runs from three lines before its first change to three lines after its last,
                // and swallows the next change when the gap between them is no longer than the context.
                var start = Math.Max(0, index - CONTEXT_LINES);
                var end   = index;

                while (true)
                {
                    var next = end;

                    while (next < script.Count && script[next].Kind != SAME) next++;

                    end = next;

                    var gap = 0;
                    var probe = end;

                    while (probe < script.Count && script[probe].Kind == SAME) { gap++; probe++; }

                    if (probe >= script.Count || gap > CONTEXT_LINES * 2) break;

                    end = probe;
                }

                var stop = end;
                var tail = 0;

                while (stop < script.Count && script[stop].Kind == SAME && tail < CONTEXT_LINES) { stop++; tail++; }

                hunks.Add(RenderHunk(script, start, stop));
                index = stop;
            }

            var header = string.IsNullOrEmpty(fileName)
                ? string.Empty
                : "diff --git a/" + fileName + " b/" + fileName + "\n"
                + "--- a/" + fileName + (oldLines.Count == 0 ? "\t(new file)" : "") + "\n"
                + "+++ b/" + fileName + (newLines.Count == 0 ? "\t(deleted)" : "") + "\n";

            return header + string.Join("", hunks);
        }

        /// <inheritdoc />
        public override HTMLElement Render() => InnerElement;

        // ───────────────────────── drawing ─────────────────────────

        private void Redraw()
        {
            ClearChildren(InnerElement);
            _files = Parse(_diff);

            var multiple = _files.Count > 1;

            foreach (var file in _files)
            {
                InnerElement.appendChild(DrawFile(file, multiple));
            }

            InnerElement.UpdateClassIf(_files.Count == 0, "tss-inlinediff-empty");
            InnerElement.UpdateClassIf(!_wrap,            "tss-inlinediff-nowrap");
            InnerElement.UpdateClassIf(!_lineNumbers,     "tss-inlinediff-nonumbers");
        }

        private HTMLElement DrawFile(InlineDiffFile file, bool multiple)
        {
            var name     = !string.IsNullOrEmpty(file.Name) ? file.Name : _fileName;
            var root     = Div(Att("tss-inlinediff-file"));
            var rows     = DrawRows(file);
            var folded   = _collapsible && _collapsed;
            var showHead = !string.IsNullOrEmpty(name) || multiple || _collapsible;

            if (showHead)
            {
                var kind = file.IsNew ? "new" : file.IsDeleted ? "deleted" : null;

                var stats = Span(Att("tss-inlinediff-stats"));

                if (file.Added   > 0) stats.appendChild(Span(Att("tss-inlinediff-stat tss-inlinediff-stat-added",   text: "+" + file.Added)));
                if (file.Removed > 0) stats.appendChild(Span(Att("tss-inlinediff-stat tss-inlinediff-stat-removed", text: "−" + file.Removed)));

                var head = Div(Att("tss-inlinediff-header"),
                               Span(Att("tss-inlinediff-icon"), I(file.IsNew ? UIcons.FileMedical : file.IsDeleted ? UIcons.FileMinus : UIcons.FileEdit)),
                               Span(Att("tss-inlinediff-name", text: name ?? string.Empty, title: name)));

                if (kind is object) head.appendChild(Span(Att("tss-inlinediff-kind", text: kind)));

                head.appendChild(stats);

                if (_collapsible)
                {
                    head.appendChild(Span(Att("tss-inlinediff-chevron"), I(UIcons.AngleSmallDown)));
                    head.classList.add("tss-inlinediff-header-toggle");
                    head.setAttribute("role", "button");
                    head.setAttribute("tabindex", "0");
                    head.setAttribute("aria-expanded", folded ? "false" : "true");

                    void Toggle()
                    {
                        var nowFolded = !root.classList.contains("tss-inlinediff-folded");

                        root.UpdateClassIf(nowFolded, "tss-inlinediff-folded");
                        head.setAttribute("aria-expanded", nowFolded ? "false" : "true");
                    }

                    head.addEventListener("click", _ => Toggle());
                    head.addEventListener("keydown", ev =>
                    {
                        var key = ev.As<KeyboardEvent>().key;

                        if (key == "Enter" || key == " ")
                        {
                            ev.preventDefault();
                            Toggle();
                        }
                    });
                }

                root.appendChild(head);
            }

            root.UpdateClassIf(folded, "tss-inlinediff-folded");
            root.appendChild(rows);

            return root;
        }

        private HTMLElement DrawRows(InlineDiffFile file)
        {
            var body  = Div(Att("tss-inlinediff-body"));
            var shown = _maxLines > 0 && file.Lines.Count > _maxLines + 2 ? _maxLines : file.Lines.Count;
            var more  = file.Lines.Count - shown;

            // Where a replaced run starts, so the part of a line that changed can be found against the
            // line it replaced.
            var lines = file.Lines;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var row  = DrawLine(line, lines, i);

                if (i >= shown) row.classList.add("tss-inlinediff-more");

                body.appendChild(row);
            }

            if (more > 0)
            {
                var label  = more == 1 ? "Show 1 more line" : "Show " + more + " more lines";
                var button = Button(Att("tss-inlinediff-expand", type: "button"), Span(Att(text: label)));

                button.addEventListener("click", _ =>
                {
                    body.classList.add("tss-inlinediff-open");
                    button.remove();
                });

                body.appendChild(button);
            }

            return body;
        }

        private HTMLElement DrawLine(InlineDiffLine line, List<InlineDiffLine> all, int index)
        {
            if (line.Kind == HUNK)
            {
                var hunk = Div(Att("tss-inlinediff-row tss-inlinediff-hunk"),
                               Span(Att("tss-inlinediff-num")),
                               Span(Att("tss-inlinediff-num")),
                               Span(Att("tss-inlinediff-sign")),
                               Span(Att("tss-inlinediff-text", text: line.Text)));

                return hunk;
            }

            var kindClass = line.Kind == ADDED ? "tss-inlinediff-add" : line.Kind == REMOVED ? "tss-inlinediff-del" : "tss-inlinediff-ctx";
            var sign      = line.Kind == ADDED ? "+" : line.Kind == REMOVED ? "−" : "";
            var text      = Span(Att("tss-inlinediff-text"));

            FillText(text, line, all, index);

            return Div(Att("tss-inlinediff-row " + kindClass),
                       Span(Att("tss-inlinediff-num", text: line.OldNumber > 0 ? line.OldNumber.ToString() : "")),
                       Span(Att("tss-inlinediff-num", text: line.NewNumber > 0 ? line.NewNumber.ToString() : "")),
                       Span(Att("tss-inlinediff-sign", text: sign)),
                       text);
        }

        // A removed line followed by an added one (a line edited in place) marks only the span between the
        // text they share at either end, so the eye goes to the words that changed, not the whole line.
        private static void FillText(HTMLElement target, InlineDiffLine line, List<InlineDiffLine> all, int index)
        {
            var partner = line.Kind == REMOVED ? PartnerOf(all, index, 1)
                        : line.Kind == ADDED   ? PartnerOf(all, index, -1)
                        : null;

            if (partner is null || line.Text == partner.Text)
            {
                target.textContent = line.Text.Length == 0 ? "​" : line.Text;
                return;
            }

            var a      = line.Text;
            var b      = partner.Text;
            var prefix = 0;

            while (prefix < a.Length && prefix < b.Length && a[prefix] == b[prefix]) prefix++;

            var suffix = 0;

            while (suffix < a.Length - prefix && suffix < b.Length - prefix && a[a.Length - 1 - suffix] == b[b.Length - 1 - suffix]) suffix++;

            var middle = a.Substring(prefix, a.Length - prefix - suffix);

            // Nothing in common worth pointing at: the whole line is the change, which the row tint already says.
            if (middle.Length == 0 || prefix + suffix == 0)
            {
                target.textContent = a.Length == 0 ? "​" : a;
                return;
            }

            if (prefix > 0) target.appendChild(document.createTextNode(a.Substring(0, prefix)));

            target.appendChild(Span(Att("tss-inlinediff-mark", text: middle)));

            if (suffix > 0) target.appendChild(document.createTextNode(a.Substring(a.Length - suffix)));
        }

        // The line this one was edited into (or from): the first of the opposite kind next to its own run,
        // paired in order, and only when the removed and added runs have the same length.
        private static InlineDiffLine PartnerOf(List<InlineDiffLine> all, int index, int direction)
        {
            var kind  = all[index].Kind;
            var other = kind == REMOVED ? ADDED : REMOVED;

            // Walk to the start of this run.
            var runStart = index;

            while (runStart > 0 && all[runStart - 1].Kind == kind) runStart--;

            var runEnd = index;

            while (runEnd + 1 < all.Count && all[runEnd + 1].Kind == kind) runEnd++;

            var position = index - runStart;
            var length   = runEnd - runStart + 1;

            // The opposite run sits right after a removed run and right before an added one.
            int otherStart;

            if (direction > 0)
            {
                otherStart = runEnd + 1;
            }
            else
            {
                var s = runStart - 1;

                if (s < 0 || all[s].Kind != other) return null;

                while (s > 0 && all[s - 1].Kind == other) s--;

                otherStart = s;
            }

            if (otherStart >= all.Count || all[otherStart].Kind != other) return null;

            var otherLength = 0;

            while (otherStart + otherLength < all.Count && all[otherStart + otherLength].Kind == other) otherLength++;

            if (otherLength != length) return null;

            return all[otherStart + position];
        }

        // ───────────────────────── parsing ─────────────────────────

        private static List<InlineDiffFile> Parse(string diff)
        {
            var files = new List<InlineDiffFile>();

            if (string.IsNullOrWhiteSpace(diff)) return files;

            var rows     = diff.Replace("\r\n", "\n").Split('\n');
            InlineDiffFile current = null;
            var oldNumber = 0;
            var newNumber = 0;
            var inHunk    = false;

            InlineDiffFile Begin()
            {
                var file = new InlineDiffFile();

                files.Add(file);
                inHunk = false;

                return file;
            }

            foreach (var row in rows)
            {
                if (row.StartsWith("diff --git "))
                {
                    current = Begin();
                    current.Name = NameFromGitHeader(row);
                    continue;
                }

                if (!inHunk && row.StartsWith("--- "))
                {
                    current ??= Begin();

                    var raw = row.Substring(4);

                    if (raw.Contains("\t(new file)") || raw.StartsWith("/dev/null")) current.IsNew = true;

                    if (string.IsNullOrEmpty(current.Name)) current.Name = StripPrefix(CutAtTab(raw), "a/");

                    continue;
                }

                if (!inHunk && row.StartsWith("+++ "))
                {
                    current ??= Begin();

                    var raw = row.Substring(4);

                    if (raw.Contains("\t(deleted)") || raw.StartsWith("/dev/null")) current.IsDeleted = true;

                    if (string.IsNullOrEmpty(current.Name) || current.Name == "/dev/null") current.Name = StripPrefix(CutAtTab(raw), "b/");

                    continue;
                }

                if (row.StartsWith("@@"))
                {
                    current ??= Begin();

                    ParseHunkHeader(row, out oldNumber, out newNumber, out var context);

                    // Only the second and later hunks get a separator: the first one starts the file.
                    if (current.Lines.Count > 0 || !string.IsNullOrEmpty(context))
                    {
                        current.Lines.Add(new InlineDiffLine { Kind = HUNK, Text = string.IsNullOrEmpty(context) ? "⋯" : "⋯  " + context });
                    }

                    inHunk = true;
                    continue;
                }

                if (!inHunk)
                {
                    // index / mode / similarity lines between the git header and the first hunk.
                    if (current is object) continue;

                    // A bare run of +/- lines with no hunk header at all.
                    if (row.Length == 0) continue;

                    if (row[0] != ADDED && row[0] != REMOVED && row[0] != SAME) continue;

                    current = Begin();
                    inHunk  = true;
                    oldNumber = 1;
                    newNumber = 1;
                }

                if (row.Length == 0)
                {
                    // A context line whose single space was trimmed away by whatever carried the diff.
                    AddLine(current, SAME, "", ref oldNumber, ref newNumber);
                    continue;
                }

                var kind = row[0];

                if (kind == '\\') continue; // "\ No newline at end of file"

                if (kind != ADDED && kind != REMOVED && kind != SAME) continue;

                AddLine(current, kind, row.Substring(1), ref oldNumber, ref newNumber);
            }

            // The trailing newline of the diff text leaves one empty "context" row at the very end.
            foreach (var file in files)
            {
                while (file.Lines.Count > 0 && file.Lines[file.Lines.Count - 1].Kind == SAME && file.Lines[file.Lines.Count - 1].Text.Length == 0 && file.Lines[file.Lines.Count - 1].Trailing) file.Lines.RemoveAt(file.Lines.Count - 1);

                foreach (var line in file.Lines)
                {
                    if (line.Kind == ADDED)   file.Added++;
                    if (line.Kind == REMOVED) file.Removed++;
                }
            }

            files.RemoveAll(f => f.Lines.Count == 0);

            return files;
        }

        private static void AddLine(InlineDiffFile file, char kind, string text, ref int oldNumber, ref int newNumber)
        {
            var line = new InlineDiffLine { Kind = kind, Text = text, Trailing = text.Length == 0 && kind == SAME };

            if (kind != ADDED)   line.OldNumber = oldNumber++;
            if (kind != REMOVED) line.NewNumber = newNumber++;

            file.Lines.Add(line);
        }

        private static void ParseHunkHeader(string row, out int oldStart, out int newStart, out string context)
        {
            // @@ -12,7 +12,9 @@ optional text
            oldStart = 1;
            newStart = 1;
            context  = null;

            var close = row.IndexOf("@@", 2);

            if (close < 0) return;

            var ranges = row.Substring(2, close - 2).Trim().Split(' ');

            foreach (var range in ranges)
            {
                if (range.Length < 2) continue;

                var comma = range.IndexOf(',');
                var digits = range.Substring(1, (comma < 0 ? range.Length : comma) - 1);
                var parsed = 1;

                if (!int.TryParse(digits, out parsed)) continue;

                if (range[0] == '-') oldStart = parsed;
                if (range[0] == '+') newStart = parsed;
            }

            context = row.Substring(close + 2).Trim();
        }

        private static string NameFromGitHeader(string row)
        {
            // diff --git a/path b/path
            var rest = row.Substring("diff --git ".Length);
            var mid  = rest.IndexOf(" b/");

            if (mid < 0) return StripPrefix(rest, "a/");

            return rest.Substring(mid + 3);
        }

        private static string CutAtTab(string text)
        {
            var tab = text.IndexOf('\t');

            return tab < 0 ? text : text.Substring(0, tab);
        }

        private static string StripPrefix(string text, string prefix) => text.StartsWith(prefix) ? text.Substring(prefix.Length) : text;

        // ───────────────────────── comparing ─────────────────────────

        private static List<string> SplitLines(string text)
        {
            var lines = new List<string>();

            if (string.IsNullOrEmpty(text)) return lines;

            var parts = text.Replace("\r\n", "\n").Split('\n');
            var count = parts.Length;

            // "a\nb\n" is two lines, not three.
            if (count > 0 && parts[count - 1].Length == 0) count--;

            for (var i = 0; i < count; i++) lines.Add(parts[i]);

            return lines;
        }

        private sealed class Op
        {
            public char   Kind;
            public string Text;
            public int    OldNumber;
            public int    NewNumber;
        }

        // The edit script between two line lists, from the longest common subsequence, so a line that did not
        // change is not reported as removed and added again.
        private static List<Op> EditScript(List<string> a, List<string> b)
        {
            var script = new List<Op>();
            var head   = 0;

            while (head < a.Count && head < b.Count && a[head] == b[head]) head++;

            var tail = 0;

            while (tail < a.Count - head && tail < b.Count - head && a[a.Count - 1 - tail] == b[b.Count - 1 - tail]) tail++;

            for (var i = 0; i < head; i++) script.Add(new Op { Kind = SAME, Text = a[i], OldNumber = i + 1, NewNumber = i + 1 });

            var aMid = a.Count - head - tail;
            var bMid = b.Count - head - tail;

            if (aMid > MAX_COMPARED_LINES || bMid > MAX_COMPARED_LINES)
            {
                for (var i = 0; i < aMid; i++) script.Add(new Op { Kind = REMOVED, Text = a[head + i], OldNumber = head + i + 1 });
                for (var j = 0; j < bMid; j++) script.Add(new Op { Kind = ADDED,   Text = b[head + j], NewNumber = head + j + 1 });
            }
            else if (aMid > 0 || bMid > 0)
            {
                // lengths[i, j]: the common subsequence of the last (aMid - i) and (bMid - j) middle lines.
                var width   = bMid + 1;
                var lengths = new int[(aMid + 1) * width];

                for (var i = aMid - 1; i >= 0; i--)
                {
                    for (var j = bMid - 1; j >= 0; j--)
                    {
                        lengths[i * width + j] = a[head + i] == b[head + j]
                            ? lengths[(i + 1) * width + j + 1] + 1
                            : Math.Max(lengths[(i + 1) * width + j], lengths[i * width + j + 1]);
                    }
                }

                var x = 0;
                var y = 0;

                while (x < aMid && y < bMid)
                {
                    if (a[head + x] == b[head + y])
                    {
                        script.Add(new Op { Kind = SAME, Text = a[head + x], OldNumber = head + x + 1, NewNumber = head + y + 1 });
                        x++;
                        y++;
                    }
                    else if (lengths[(x + 1) * width + y] >= lengths[x * width + y + 1])
                    {
                        script.Add(new Op { Kind = REMOVED, Text = a[head + x], OldNumber = head + x + 1 });
                        x++;
                    }
                    else
                    {
                        script.Add(new Op { Kind = ADDED, Text = b[head + y], NewNumber = head + y + 1 });
                        y++;
                    }
                }

                while (x < aMid) { script.Add(new Op { Kind = REMOVED, Text = a[head + x], OldNumber = head + x + 1 }); x++; }
                while (y < bMid) { script.Add(new Op { Kind = ADDED,   Text = b[head + y], NewNumber = head + y + 1 }); y++; }
            }

            for (var i = 0; i < tail; i++)
            {
                script.Add(new Op { Kind = SAME, Text = a[a.Count - tail + i], OldNumber = a.Count - tail + i + 1, NewNumber = b.Count - tail + i + 1 });
            }

            return script;
        }

        private static string RenderHunk(List<Op> script, int start, int stop)
        {
            var oldStart = 0;
            var newStart = 0;
            var oldCount = 0;
            var newCount = 0;

            for (var i = start; i < stop; i++)
            {
                var op = script[i];

                if (op.Kind != ADDED)
                {
                    if (oldStart == 0) oldStart = op.OldNumber;
                    oldCount++;
                }

                if (op.Kind != REMOVED)
                {
                    if (newStart == 0) newStart = op.NewNumber;
                    newCount++;
                }
            }

            // A range that is empty on one side starts one line before where it would have been.
            if (oldStart == 0) oldStart = FirstNumber(script, start, stop, true)  - 1;
            if (newStart == 0) newStart = FirstNumber(script, start, stop, false) - 1;

            var rows = new List<string> { "@@ -" + oldStart + "," + oldCount + " +" + newStart + "," + newCount + " @@" };

            for (var i = start; i < stop; i++)
            {
                rows.Add((script[i].Kind == SAME ? " " : script[i].Kind.ToString()) + script[i].Text);
            }

            return string.Join("\n", rows) + "\n";
        }

        private static int FirstNumber(List<Op> script, int start, int stop, bool old)
        {
            for (var i = start; i < stop; i++)
            {
                var n = old ? script[i].OldNumber : script[i].NewNumber;

                if (n > 0) return n;
            }

            for (var i = stop; i < script.Count; i++)
            {
                var n = old ? script[i].OldNumber : script[i].NewNumber;

                if (n > 0) return n;
            }

            return 1;
        }

        private sealed class InlineDiffLine
        {
            public char   Kind;
            public string Text;
            public int    OldNumber;
            public int    NewNumber;
            public bool   Trailing;
        }

        private sealed class InlineDiffFile
        {
            public string                Name;
            public bool                  IsNew;
            public bool                  IsDeleted;
            public int                   Added;
            public int                   Removed;
            public List<InlineDiffLine>  Lines = new List<InlineDiffLine>();
        }
    }
}
