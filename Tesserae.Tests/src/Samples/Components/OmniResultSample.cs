using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 105, Icon = UIcons.LayoutFluid)]
    public class OmniResultSample : IComponent, ISample
    {
        // What the sample searched for, and so what the excerpts highlight.
        private static readonly string[] QueryTerms = { "brake sensor", "calibration" };

        private readonly IComponent _content;

        // "Selection" section: the rows own the selection, the sample only reports on it.
        private readonly List<OmniResult<Hit>> _selectable = new List<OmniResult<Hit>>();
        private readonly TextBlock             _selectionState;

        // "Commands" section.
        private readonly TextBlock _lastCommand;

        private static readonly Hit[] Hits =
        {
            new Hit("brake-sensor-reports", null, UIcons.Folder, "#6366f1", "Name match",
                "Contains inspection sheets and calibration runs for the BRK-447 brake sensor family, 2022 onward.",
                new[] { "All Files / sample-files", "24 files", "Pius Neuhaus", "2 days ago" }, 0),

            new Hit("BRK-SEN-447 calibration procedure.pdf", "PDF", UIcons.FilePdf, "#ef4444", "3 matches in text",
                "Torque the mount to 12 Nm before starting brake sensor work. Full calibration steps and the BRK-SEN-447 harness diagram sit on page 14 — connector keying changed in revision C.",
                new[] { "sample-files / pdfs / procedures", "2.4 MB", "Pius Neuhaus", "Apr 12, 2024" }, 24),

            new Hit("Sensor drift analysis.xlsx", "XLSX", UIcons.FileExcel, "#16a34a", "2 matches in text",
                "Column F tracks brake sensor drift across 14 units. Values above 0.8 mV trigger a re-calibration request in the CMMS.",
                new[] { "sample-files / analysis", "480 KB", "Marie Lang", "Apr 11, 2024" }, 3),

            new Hit("Line 3 shift handover — Mar 28.docx", "DOCX", UIcons.FileWord, "#3b82f6", "1 match in text",
                "Night shift replaced two brake sensor modules on cell 4. Calibration deferred to day shift; ticket JR-2214 stays open.",
                new[] { "sample-files / handover", "1.1 MB", "Tomas Rieger", "Mar 28, 2024" }, 4),

            new Hit("field-failures / 2024", null, UIcons.Folder, "#6366f1", "Content match in 4 files",
                "Failure reports filed against the brake sensor housing, including three units returned after calibration drift at the Ingolstadt line.",
                new[] { "All Files / field-failures", "112 files", "Quality team", "6 hours ago" }, 0),

            new Hit("Supplier notice — Bismuth BRK-447.pdf", "PDF", UIcons.FilePdf, "#ef4444", "Title match",
                "Bismuth is discontinuing the BRK-447 brake sensor connector; the replacement needs a calibration pass on every line.",
                new[] { "sample-files / suppliers", "320 KB", "Anja Vogt", "Mar 19, 2024" }, 2),

            new Hit("brake-calibration-log.txt", "TXT", UIcons.File, "#94a3b8", "Name + content match",
                "2024-03-28T21:14Z cell-4 brake sensor #7741 calibration OK (0.42 mV). 2024-03-28T21:19Z cell-4 sensor #7742 calibration FAILED.",
                new[] { "sample-files / logs", "88 KB", "System", "Mar 28, 2024" }, 0)
        };

        public OmniResultSample()
        {
            _selectionState = TextBlock("Nothing selected.").Small().Foreground(Theme.Secondary.Foreground);
            _lastCommand    = TextBlock("No command run yet.").Small().Foreground(Theme.Secondary.Foreground);

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(OmniResultSample), UIcons.LayoutFluid, "Search-result rows with highlighted excerpts, a source footer, selection, commands and a page preview")
                .FlatSection(VStack().WS().Children(Overview()))
                .FlatSection(VStack().WS().Children(Everything()))
                .FlatSection(VStack().WS().Children(NoText()))
                .FlatSection(VStack().WS().Children(NoPages()))
                .FlatSection(VStack().WS().Children(TitleOnly()))
                .FlatSection(VStack().WS().Children(Tiles()))
                .FlatSection(VStack().WS().Children(Highlighting()))
                .FlatSection(VStack().WS().Children(Selection()))
                .FlatSection(VStack().WS().Children(Commands()))
                .FlatSection(VStack().WS().Children(Pages()))
                .SeeAlso(typeof(OmniBoxSample), typeof(ContextCardSample), typeof(ResourceCardSample), typeof(CardSample), typeof(DetailsListSample));
        }

        // One feature per card: a subtitle, a line or two saying what to try, then the rows themselves.
        private static Card FeatureCard(string title, string subTitle, string description, params IComponent[] content)
        {
            var stack = VStack().WS().Children(SampleSubTitle(subTitle), TextBlock(description).MB(8));

            foreach (var c in content)
            {
                stack.Add(c);
            }

            return Card(stack).SetTitle(title);
        }

        // ---------- Overview ----------

        private IComponent Overview()
        {
            return FeatureCard("Overview", "One row per hit",
                "OmniResult<T> is the row a search result is drawn as: an icon tile whose background is a wash of the color the glyph is in, a title with a badge saying what matched, an excerpt with the query terms marked in it, and a footer naming the source and whatever metadata belongs beside it. The result it stands for rides along as Result, so one handler shared by a whole list of rows can act on the right hit without a closure per row.",
                TextBlock("Everything past the title is optional: drop the excerpt, the page preview, the footer, or all of them, and the row tightens up accordingly — the four sections below are the same seven hits with progressively less on them. Rows are selectable, with the checkbox beside or over the tile; commands are reached by right-click and, optionally, a [...] button at the row's top-right, with room for a couple of inline commands before it.").Small());
        }

        // ---------- Everything ----------

        private IComponent Everything()
        {
            return FeatureCard("The full row", "Icon, badge, excerpt, footer and a page preview",
                "Hover a row: it lifts onto the hover background, the pages fan out like a macOS Downloads stack, and the [...] button appears at the top-right, level with the title. The fan opens inside a rail wide enough for it, so nothing in the row moves.",
                Results(Hits, withText: true, withPages: true));
        }

        // ---------- Without the excerpt ----------

        private IComponent NoText()
        {
            return FeatureCard("Without the excerpt", "Title, footer and preview only",
                "A result with nothing worth quoting — or a denser list — simply never gets SetText. The row becomes two lines and the page preview keeps its size, so a mixed list stays on one rhythm.",
                Results(Hits.Take(4).ToArray(), withText: false, withPages: true));
        }

        // ---------- Without the preview ----------

        private IComponent NoPages()
        {
            return FeatureCard("Without the page preview", "The excerpt takes the full width",
                "Leave the PagesStack out and the rail goes with it, so the title and the excerpt run to the row's end.",
                Results(Hits.Take(3).ToArray(), withText: true, withPages: false));
        }

        // ---------- Title, badge and footer only ----------

        private IComponent TitleOnly()
        {
            return FeatureCard("Title and footer only", "The compact list",
                "Neither excerpt nor preview: one two-line row per hit, which is what a long result list or a picker wants.",
                Results(Hits, withText: false, withPages: false));
        }

        // ---------- Tiles ----------

        private IComponent Tiles()
        {
            return FeatureCard("Icon tiles", "A glyph, or the file type spelled out",
                "SetIcon(icon, color) puts a UIcons glyph on the tile in that color, over a wash of the same color computed from it — a pale tint under a light theme, a deep one under a dark theme, cached so a list drawing the same handful of file-type colors only computes each once. SetIcon(text, color) spells the type out instead, for a format no glyph says plainly. Any component works too, for a thumbnail or an avatar.",
                VStack().WS().Children(
                    OmniResult(Hits[1], "BRK-SEN-447 calibration procedure.pdf").SetIcon(UIcons.FilePdf, "#ef4444").SetSource("#0061d5", "Box").SetFooterEntries("SetIcon(UIcons.FilePdf, \"#ef4444\")"),
                    OmniResult(Hits[1], "Q3 line review.pptx").SetIcon("PPTX", "#f97316").SetSource("#0061d5", "Box").SetFooterEntries("SetIcon(\"PPTX\", \"#f97316\")"),
                    OmniResult(Hits[2], "Sensor drift analysis.xlsx").SetIcon("XLSX", "#16a34a").SetSource("#0061d5", "Box").SetFooterEntries("SetIcon(\"XLSX\", \"#16a34a\")"),
                    OmniResult(Hits[6], "brake-calibration-log.txt").SetIcon("TXT", "#94a3b8").SetSource("#0061d5", "Box").SetFooterEntries("A grey color stays grey in both themes"),
                    OmniResult(Hits[0], "curiosity-logo.svg").SetIcon(Image("./assets/img/curiosity-logo.svg")).SetSource("#0061d5", "Box").SetFooterEntries("SetIcon(IComponent) — a thumbnail covers the tile"),
                    OmniResult(Hits[0], "brake-sensor-reports").SetIcon(UIcons.Folder).SetSource("#0061d5", "Box").SetFooterEntries("No color: the tile falls back to the theme's own")));
        }

        // ---------- Highlighting ----------

        private IComponent Highlighting()
        {
            var hit = Hits[1];

            return FeatureCard("Highlighting", "Marking the query in the excerpt",
                "The excerpt is plain text, not a component: HighlightWords marks every occurrence of the terms the user searched for, and Highlight takes the Regex a search backend hands back for the job. Matching runs against the text itself and each match is wrapped in its own element, so an excerpt containing angle brackets renders them instead of obeying them.",
                VStack().WS().Children(
                    OmniResult(hit, "HighlightWords(\"brake sensor\", \"calibration\")")
                        .SetIcon(UIcons.FilePdf, "#ef4444")
                        .SetText(hit.Text)
                        .HighlightWords(QueryTerms)
                        .SetSource("#0061d5", "Box"),
                    OmniResult(hit, "Highlight(\"BRK-[A-Z]+-\\\\d+\")")
                        .SetIcon(UIcons.FilePdf, "#ef4444")
                        .SetText(hit.Text)
                        .Highlight("(BRK-[A-Z]+-\\d+)")
                        .SetSource("#0061d5", "Box"),
                    OmniResult(hit, "No highlighter, and markup in the text")
                        .SetIcon(UIcons.FilePdf, "#ef4444")
                        .SetText("A passage that contains <script>alert('hi')</script> and <b>bold</b> markup is shown as the text it is.")
                        .SetSource("#0061d5", "Box"),
                    OmniResult(hit, "TextLines(4) — a longer excerpt")
                        .SetIcon(UIcons.FilePdf, "#ef4444")
                        .SetText(hit.Text + " " + hit.Text)
                        .HighlightWords(QueryTerms)
                        .TextLines(4)
                        .SetSource("#0061d5", "Box")));
        }

        // ---------- Selection ----------

        private IComponent Selection()
        {
            _selectable.Clear();

            var modes = new[]
            {
                OmniResultSelectionMode.OnHoverBeforeIcon,
                OmniResultSelectionMode.OnHoverOverIcon,
                OmniResultSelectionMode.AlwaysBeforeIcon,
                OmniResultSelectionMode.ReplacingIcon
            };

            var sections = VStack().WS();

            for (int m = 0; m < modes.Length; m++)
            {
                var mode = modes[m];
                var list = VStack().WS();

                for (int i = 0; i < 2; i++)
                {
                    var hit = Hits[(m * 2 + i) % Hits.Length];
                    var row = Row(hit, withText: false, withPages: false)
                        .Selectable(mode)
                        .OnSelectionChanged((r, isSelected) => ReportSelection())
                        .OnRangeSelectionRequested(r => SelectRangeTo(r));

                    _selectable.Add(row);
                    list.Add(row);
                }

                sections.Add(TextBlock($"OmniResultSelectionMode.{mode}").Small().SemiBold().MT(m == 0 ? 0 : 12).MB(4));
                sections.Add(list);
            }

            return FeatureCard("Selection", "Four places for the checkbox",
                "Selectable(mode) makes a row selectable. The checkbox sits before the tile or over it, revealed on hover or always visible, or takes the tile's place entirely — and a selected row always shows its checkbox, whatever the mode. Ctrl-clicking a row toggles it too, and shift-clicking one asks for the range from the last row selected: a single card knows nothing about its siblings, so OnRangeSelectionRequested hands that to the host list, which selects them itself (that is what the rows below do, across all four groups).",
                sections,
                _selectionState.MT(8),
                HStack().Gap(8.px()).MT(8).Children(
                    Button("Select all").SetIcon(UIcons.ListCheck).OnClick(() => { foreach (var r in _selectable) r.Selected(); }),
                    Button("Clear selection").SetIcon(UIcons.Broom).OnClick(() => { foreach (var r in _selectable) r.Selected(false); })));
        }

        private void SelectRangeTo(OmniResult<Hit> target)
        {
            var last = _selectable.FindIndex(r => r.IsSelected);

            if (last < 0)
            {
                target.Selected();
                return;
            }

            var to   = _selectable.IndexOf(target);
            var from = last;

            if (to < from)
            {
                var swap = from;
                from = to;
                to   = swap;
            }

            for (int i = from; i <= to; i++)
            {
                _selectable[i].Selected();
            }
        }

        private void ReportSelection()
        {
            var selected = _selectable.Where(r => r.IsSelected).ToArray();

            _selectionState.Text = selected.Length == 0
                ? "Nothing selected."
                : $"{selected.Length} selected: {string.Join(", ", selected.Select(r => r.Result.Title))}";
        }

        // ---------- Commands ----------

        private IComponent Commands()
        {
            return FeatureCard("Commands", "Right-click, a [...] button, and inline commands",
                "OnContextMenu registers what opens the row's commands and says how they are reached: by right-click alone, or also by a [...] button at the top-right — always visible, or revealed on hover. The Func overload builds a ContextMenu the row shows itself, at the pointer or under the button; the Action<OmniResult<T>> overload hands the row to a plain handler, which can still place a menu with ShowMenu. InlineCommands puts a couple of buttons before the [...]; the space they take is reserved either way, so revealing them never shifts the row.",
                VStack().WS().Children(
                    WithMenu(Row(Hits[1], withText: false, withPages: false), OmniResultCommandsMode.RightClickOnly)
                        .SetFooterEntries("RightClickOnly — no button is drawn"),
                    WithMenu(Row(Hits[2], withText: false, withPages: false), OmniResultCommandsMode.ButtonOnHover)
                        .SetFooterEntries("ButtonOnHover — hover the row"),
                    WithMenu(Row(Hits[3], withText: false, withPages: false), OmniResultCommandsMode.ButtonAlwaysVisible)
                        .SetFooterEntries("ButtonAlwaysVisible"),
                    WithMenu(Row(Hits[5], withText: false, withPages: false), OmniResultCommandsMode.ButtonOnHover)
                        .InlineCommands(
                            Button(UIcons.Download).Tooltip("Download").OnClick(() => Report("Download")),
                            Button(UIcons.Share).Tooltip("Share").OnClick(() => Report("Share")))
                        .SetFooterEntries("InlineCommands — shown on hover, before the [...]"),
                    WithMenu(Row(Hits[6], withText: false, withPages: false), OmniResultCommandsMode.ButtonAlwaysVisible)
                        .InlineCommands(OmniResultCommandsVisibility.AlwaysVisible,
                            Button(UIcons.Star).Tooltip("Pin").OnClick(() => Report("Pin")))
                        .SetFooterEntries("InlineCommands(AlwaysVisible)"),
                    Row(Hits[4], withText: false, withPages: false)
                        .OnContextMenu(r => Report($"Right-clicked \"{r.Result.Title}\" (plain handler)"))
                        .SetFooterEntries("The Action<OmniResult<T>> overload — no menu, just a handler")),
                _lastCommand.MT(8));
        }

        // The menu is built on every open, so its items can describe the row as it stands right now.
        private OmniResult<Hit> WithMenu(OmniResult<Hit> row, OmniResultCommandsMode mode)
        {
            return row.OnContextMenu(r => new[]
            {
                ContextMenuItem(r.Result.Title).Header(),
                ContextMenuItem(MenuLine(UIcons.FolderOpen, "Open")).OnClick(() => Report($"Opening {r.Result.Title}")),
                ContextMenuItem(MenuLine(UIcons.Copy, "Copy link")).OnClick(() => Report($"Copied a link to {r.Result.Title}")),
                ContextMenuItem().Divider(),
                ContextMenuItem(MenuLine(UIcons.Trash, "Delete", Theme.Danger.Background)).OnClick(() => Report($"Deleted {r.Result.Title}"))
            }, mode);
        }

        private static IComponent MenuLine(UIcons icon, string text, string color = null)
            => HStack().Children(Icon(icon, color: color), TextBlock(text).ML(8));

        private void Report(string what)
        {
            _lastCommand.Text = what;
            Toast().Information(what);
        }

        // ---------- PagesStack ----------

        private IComponent Pages()
        {
            var thumbnails = new[]
            {
                "./assets/img/box-img.svg",
                "./assets/img/curiosity-logo.svg",
                "./assets/img/box-img.svg",
                "./assets/img/curiosity-logo.svg"
            };

            return FeatureCard("PagesStack", "The page preview on its own",
                "PagesStack is the preview rail: up to five overlapping, slightly rotated pages that fan out on a shallow arc when hovered, with a +N badge over the stack counting the pages it doesn't draw. Given thumbnail urls it draws them (all cropped to one page size); given only a count it draws blank ruled pages. The holder is sized to the width the fan needs and pinned to its right edge, so opening the fan never widens the row — which is why a row can sit right beside it.",
                HStack().WS().Wrap().Gap(32.px()).PT(8).PB(8).AlignItems(ItemAlign.End).Children(
                    PagesLabel("1 page", PagesStack(1)),
                    PagesLabel("3 pages", PagesStack(3)),
                    PagesLabel("5 pages", PagesStack(5)),
                    PagesLabel("24 pages", PagesStack(5).TotalPages(24)),
                    PagesLabel("Thumbnails", PagesStack(thumbnails).TotalPages(9)),
                    PagesLabel("Larger pages", PagesStack(4).PageSize(60, 78)),
                    PagesLabel("MaxVisible(3)", PagesStack(12).MaxVisible(3))),
                TextBlock("Held open with Fanned(), which is how OmniResult makes the stack follow hovering the whole row (PagesFanOnHover, on by default):").Small().MT(8).MB(8),
                PagesLabel("Fanned()", PagesStack(5).TotalPages(19).Fanned()));
        }

        private static IComponent PagesLabel(string label, PagesStack pages)
            => VStack().Children(pages, TextBlock(label).XSmall().Foreground(Theme.Secondary.Foreground).MT(8));

        // ---------- Row helpers ----------

        private IComponent Results(Hit[] hits, bool withText, bool withPages)
        {
            var list = VStack().WS();

            foreach (var hit in hits)
            {
                list.Add(WithMenu(Row(hit, withText, withPages), OmniResultCommandsMode.ButtonOnHover));
            }

            return list;
        }

        private OmniResult<Hit> Row(Hit hit, bool withText, bool withPages)
        {
            var row = OmniResult(hit, hit.Title)
                .SetBadge(hit.Badge)
                .SetSource("#0061d5", "Box")
                .SetFooterEntries(hit.Metadata)
                .OnClick((r, _) => Toast().Information($"Opening {r.Result.Title}"));

            if (hit.IconText is null)
            {
                row.SetIcon(hit.Icon, hit.Color);
            }
            else
            {
                row.SetIcon(hit.IconText, hit.Color);
            }

            if (withText)
            {
                row.SetText(hit.Text).HighlightWords(QueryTerms);
            }

            // A folder has no pages of its own, so it gets no preview even in the sections that show them.
            if (withPages && hit.Pages > 0)
            {
                row.SetPages(PagesStack(hit.Pages > 5 ? 5 : hit.Pages).TotalPages(hit.Pages));
            }

            return row;
        }

        public HTMLElement Render() => _content.Render();

        // The result each row stands for — what a real app would hand the row instead.
        private class Hit
        {
            public string   Title    { get; }
            public string   IconText { get; }
            public UIcons   Icon     { get; }
            public string   Color    { get; }
            public string   Badge    { get; }
            public string   Text     { get; }
            public string[] Metadata { get; }
            public int      Pages    { get; }

            public Hit(string title, string iconText, UIcons icon, string color, string badge, string text, string[] metadata, int pages)
            {
                Title    = title;
                IconText = iconText;
                Icon     = icon;
                Color    = color;
                Badge    = badge;
                Text     = text;
                Metadata = metadata;
                Pages    = pages;
            }
        }
    }
}
