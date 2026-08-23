using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Search, Order = 50, Icon = UIcons.Document, Description = "Search-result rows with highlighted excerpts")]
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

        // "Source" section.
        private readonly TextBlock _sourceState;

        // "Modals" section: the rows are kept so stepping and stacking can reach them by index.
        private readonly List<OmniResult<Hit>> _modalRows = new List<OmniResult<Hit>>();

        // The two rows that open with the tile on the modal's title line - outside the chain above, so
        // stepping through it is unaffected by them.
        private readonly List<OmniResult<Hit>> _inTitleModalRows = new List<OmniResult<Hit>>();

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
            _sourceState    = TextBlock("No source clicked yet.").Small().Foreground(Theme.Secondary.Foreground);

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(OmniResultSample), UIcons.LayoutFluid, "Search-result rows with highlighted excerpts, a source footer, selection, commands and a page preview")
                .FlatSection(VStack().WS().Children(Overview()))
                .FlatSection(VStack().WS().Children(Everything()))
                .FlatSection(VStack().WS().Children(NoText()))
                .FlatSection(VStack().WS().Children(NoPages()))
                .FlatSection(VStack().WS().Children(TitleOnly()))
                .FlatSection(VStack().WS().Children(Tiles()))
                .FlatSection(VStack().WS().Children(TilesInHeader()))
                .FlatSection(VStack().WS().Children(Identifiers()))
                .FlatSection(VStack().WS().Children(Content()))
                .FlatSection(VStack().WS().Children(Modals()))
                .FlatSection(VStack().WS().Children(Sources()))
                .FlatSection(VStack().WS().Children(Contributions()))
                .FlatSection(VStack().WS().Children(Highlighting()))
                .FlatSection(VStack().WS().Children(Selection()))
                .FlatSection(VStack().WS().Children(Commands()))
                .FlatSection(VStack().WS().Children(Pages()))
                .FlatSection(VStack().WS().Children(InlinePaginationSection()))
                .FlatSection(VStack().WS().Children(InlineLabels()))
                .SeeAlso(typeof(OmniBoxSample), typeof(PagesStackSample), typeof(InlineLabelSample), typeof(ContextCardSample), typeof(ResourceCardSample), typeof(CardSample), typeof(DetailsListSample));
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
                "SetIcon(icon, color) puts a UIcons glyph on the tile in that color, over a wash of the same color computed from it — a pale tint under a light theme, a deep one under a dark theme, cached so a list drawing the same handful of file-type colors only computes each once. SetIcon(text, color) spells the type out instead, for a format no glyph says plainly, at any of the standard TextSize sizes. Any component works too, for a thumbnail or an avatar.",
                VStack().WS().Children(
                    OmniResult(Hits[1], "BRK-SEN-447 calibration procedure.pdf").SetIcon(UIcons.FilePdf, "#ef4444").SetSource("#0061d5", "Box").SetFooterEntries("SetIcon(UIcons.FilePdf, \"#ef4444\")"),
                    OmniResult(Hits[1], "Q3 line review.pptx").SetIcon("PPTX", "#f97316").SetSource("#0061d5", "Box").SetFooterEntries("SetIcon(\"PPTX\", \"#f97316\")"),
                    OmniResult(Hits[2], "Sensor drift analysis.xlsx").SetIcon("XLSX", "#16a34a").SetSource("#0061d5", "Box").SetFooterEntries("SetIcon(\"XLSX\", \"#16a34a\")"),
                    OmniResult(Hits[2], "Line 4 sensor events.jsonl").SetIcon("JSONL", "#0ea5e9", TextSize.Tiny).SetSource("#0061d5", "Box").SetFooterEntries("SetIcon(\"JSONL\", \"#0ea5e9\", TextSize.Tiny) — a longer type at a smaller size"),
                    OmniResult(Hits[6], "brake-calibration-log.txt").SetIcon("TXT", "#94a3b8").SetSource("#0061d5", "Box").SetFooterEntries("A grey color stays grey in both themes"),
                    OmniResult(Hits[0], "curiosity-logo.svg").SetIcon(Image("./assets/img/curiosity-logo.svg")).SetSource("#0061d5", "Box").SetFooterEntries("SetIcon(IComponent) — a thumbnail covers the tile"),
                    OmniResult(Hits[0], "brake-sensor-reports").SetIcon(UIcons.Folder).SetSource("#0061d5", "Box").SetFooterEntries("No color: the tile falls back to the theme's own")));
        }

        // ---------- The tile in the header ----------

        private IComponent TilesInHeader()
        {
            var quiet = VStack().WS().Children(
                HeaderIconRow(Hits[1], "BRK-SEN-447 calibration procedure.pdf").SetId("JR-2214"),
                HeaderIconRow(Hits[2], "Sensor drift analysis.xlsx"),
                HeaderIconRow(Hits[4], "field-failures / 2024"));

            var selectable = VStack().WS().Children(
                Row(Hits[3], withText: true, withPages: false).Selectable(OmniResultSelectionMode.AlwaysBeforeHeaderIcon),
                Row(Hits[5], withText: true, withPages: false).Selectable(OmniResultSelectionMode.AlwaysBeforeHeaderIcon)
                    .SetIconBadge(Icon(UIcons.Star, UIconsWeight.Solid, color: "#f0c000"), OmniResultBadgeCorner.TopRight),
                Row(Hits[6], withText: true, withPages: false).Selectable(OmniResultSelectionMode.AlwaysBeforeHeaderIcon));

            //A tile that spells its type out is a label on this line rather than a square, so the type name
            //is not limited to the three or four letters that fit a square: it grows with the text.
            var spelledOut = VStack().WS().Children(
                HeaderIconRow(Hits[1], "Q3 line review.pptx").SetIcon("PPTX", "#f97316"),
                HeaderIconRow(Hits[2], "sensor-events-2024-03.parquet").SetIcon("PARQUET", "#8b5cf6"),
                HeaderIconRow(Hits[3], "Calibration runbook.markdown").SetIcon("MARKDOWN", "#0ea5e9"),
                Row(Hits[6], withText: true, withPages: false).SetTitle("line-4-sensor-events.jsonlines").SetIcon("JSONLINES", "#0d9488")
                    .Selectable(OmniResultSelectionMode.AlwaysBeforeHeaderIcon),
                Row(Hits[5], withText: true, withPages: false).SetTitle("Bismuth supplier notice.spreadsheetml").SetIcon("SPREADSHEETML", "#16a34a")
                    .Selectable(OmniResultSelectionMode.AlwaysBeforeHeaderIcon));

            return FeatureCard("The tile in the header", "Leading the title instead of the row",
                "Two modes draw the tile small, at the start of the header line, before the identifier and the title - so the excerpt, the footer and the contribution bar start at the row's own left edge rather than indented past a 34px tile. HiddenBeforeHeaderIcon draws no checkbox at all, which is what a list that is never selected from wants: set it with SelectionMode(mode), which lays the row out without making it selectable. AlwaysBeforeHeaderIcon keeps a checkbox in its own column at the start of the row, always visible.",
                TextBlock("OmniResultSelectionMode.HiddenBeforeHeaderIcon - no checkbox anywhere").Small().SemiBold().MB(4),
                quiet,
                TextBlock("OmniResultSelectionMode.AlwaysBeforeHeaderIcon - the checkbox is always visible").Small().SemiBold().MT(12).MB(4),
                selectable,
                TextBlock("A tile that spells the file type out rather than drawing a glyph").Small().SemiBold().MT(12).MB(4),
                spelledOut,
                TextBlock("On the header line the tile sits beside words rather than in a column of its own, so a text tile is a label there instead of a square: it keeps the title's height and grows sideways with its text, so nothing has to reach for a smaller TextSize to make a long type name fit. The square tile is the floor, so a three- or four-letter type still comes out about as square as the glyph tiles beside it. The starred row above shows a corner badge pinned to the smaller tile.").Small().MT(8));
        }

        // A row laid out with its tile in the header and no checkbox at all - SelectionMode() sets the
        // layout without making the row selectable.
        private OmniResult<Hit> HeaderIconRow(Hit hit, string title)
        {
            return Row(hit, withText: true, withPages: false)
                .SetTitle(title)
                .SelectionMode(OmniResultSelectionMode.HiddenBeforeHeaderIcon);
        }

        // ---------- Identifiers ----------

        private IComponent Identifiers()
        {
            return FeatureCard("Identifiers", "A number or a key before the title",
                "SetId puts an identifier before the title - an issue number, a ticket key, a row number - drawn the quiet way an identifier reads, with a chevron pointing at the title. It never shrinks, so a long title ellipsizes before the identifier does, and an empty one drops both the identifier and the chevron.",
                VStack().WS().Children(
                    Row(Hits[1], withText: false, withPages: false).SetId("JR-2214"),
                    Row(Hits[2], withText: false, withPages: false).SetId("4471"),
                    Row(Hits[3], withText: false, withPages: false).SetId("OPS-88").SetBadge("Blocked"),
                    Row(Hits[5], withText: false, withPages: false)),
                TextBlock("The last row has no identifier, so it starts at its title.").Small().MT(8));
        }

        // ---------- Rich content ----------

        private IComponent Content()
        {
            return FeatureCard("Rich content", "When an excerpt isn't enough",
                "SetContent puts a component of your own under the excerpt, in the text column: a thumbnail, a quoted message, a table of the fields that matched. ContentMaxHeight caps how tall it may grow and fades whatever runs past it out, rather than cutting it off - so a clipped preview reads as \"there is more\" instead of as a rendering fault.",
                VStack().WS().Children(
                    Row(Hits[1], withText: true, withPages: false)
                        .SetContent(HStack().Gap(8.px()).PT(4).Children(
                            Badge("page 14").Pill(),
                            Badge("revision C").Pill(),
                            Badge("BRK-SEN-447").Pill())),
                    Row(Hits[2], withText: false, withPages: false)
                        .SetContent(VStack().WS().PT(4).Children(
                            TextBlock("Column F · drift across 14 units").Small(),
                            TextBlock("Column G · re-calibration requested").Small(),
                            TextBlock("Column H · CMMS ticket").Small(),
                            TextBlock("Column I · signed off by").Small(),
                            TextBlock("Column J · next due").Small(),
                            TextBlock("Column K · notes").Small()))
                        .ContentMaxHeight(56.px())),
                TextBlock("The second row's content is capped at 56px, so it fades out where it is cut.").Small().MT(8));
        }

        // ---------- Modals ----------

        private IComponent Modals()
        {
            for (int i = 0; i < 3; i++)
            {
                _modalRows.Add(ModalRow(Hits[i + 1], i));
            }

            var rows = VStack().WS();

            foreach (var row in _modalRows)
            {
                rows.Add(row);
            }

            _inTitleModalRows.Add(InTitleModalRow(Hits[1], "BRK-SEN-447 calibration procedure.pdf", null, null));
            _inTitleModalRows.Add(InTitleModalRow(Hits[2], "sensor-events-2024-03.parquet", "PARQUET", "#8b5cf6"));

            var inTitleRows = VStack().WS();

            foreach (var row in _inTitleModalRows)
            {
                inTitleRows.Add(row);
            }

            return FeatureCard("Opening as a modal", "The row carries its own full view",
                "SetModalContent gives the row the full view of the thing it stands for, and ToModal builds a Modal showing it: the row's identifier, chevron and title for its header - plus the tile and the source line when ModalKeepsIcon and ModalKeepsFooter ask for them - a standard set of commands at the end of that header, and the keyboard shortcuts it answers along its bottom edge. The Func overload builds the content on open, so a list of a thousand rows pays for none of them until one is asked for.",
                rows,
                TextBlock("The header's commands are whatever the row was configured for: OpenInSource adds the named button (and hangs the rest off the arrow beside it), ModalNavigation adds the arrows and \"2 of 3\" between them, ModalCommands adds [...], and the full-screen and close buttons are always there. Open one and try Esc, the arrow keys, Ctrl+Enter and Shift+Enter. \"Open a related result\" inside pushes a second sheet onto the stack - go three deep and the ones behind peek out above it; click one to go back to it, or the backdrop to dismiss the chain.").Small().MT(8),
                SampleSubTitle("The tile on the modal's title line").MT(16),
                TextBlock("ModalKeepsIconInTitle is ModalKeepsIcon's other placement: the tile is drawn small, on the title's own line, before the identifier and the title - the way the two header-icon modes draw it in the row - so the source line under the title starts where the tile does rather than being indented past it, and a row laid out that way opens as the same row enlarged. A tile that spells its type out grows with its text there too. Open these two and compare their headers with the three above.").MB(8),
                inTitleRows);
        }

        // The same modal as above, minus the navigation chain, on a row laid out with its tile in the
        // header - and opening with the tile on the modal's title line to match.
        private OmniResult<Hit> InTitleModalRow(Hit hit, string title, string iconText, string iconColor)
        {
            var row = Row(hit, withText: true, withPages: false)
                .SetTitle(title)
                .SetId("JR-2214")
                .SelectionMode(OmniResultSelectionMode.HiddenBeforeHeaderIcon)
                .SetModalContent(r => Task.FromResult<IComponent>(InTitleModalBody(r)))
                .ModalSize(60.vw(), 60.vh())
                .ModalKeepsIconInTitle()
                .ModalKeepsFooter()
                .OpenInSource("Open in Box", inNewTab => Toast().Information(inNewTab ? $"Opening \"{title}\" in a new tab" : $"Opening \"{title}\" in Box"), UIcons.ArrowUpRightFromSquare);

            if (iconText is object) row.SetIcon(iconText, iconColor);

            return row.OnClick((r, _) => ModalStack.Push($"in-title-{title}", r.Title, r.ToModal()));
        }

        private IComponent InTitleModalBody(OmniResult<Hit> result)
        {
            var metadata = result.Result.Metadata;

            return VStack().WS().P(16).Children(
                DetailsGrid()
                    .Row("Location", metadata.Length > 0 ? metadata[0] : null)
                    .Row("Size",     metadata.Length > 1 ? metadata[1] : null)
                    .Row("Owner",    metadata.Length > 2 ? metadata[2] : null)
                    .Row("Modified", metadata.Length > 3 ? metadata[3] : null),
                TextBlock(result.Result.Text).MT(16));
        }

        private OmniResult<Hit> ModalRow(Hit hit, int index)
        {
            var row = Row(hit, withText: true, withPages: false)
                .SetId("JR-2214")
                .SetModalContent(r => Task.FromResult<IComponent>(ModalBody(r, index)))
                .ModalSize(60.vw(), 60.vh())
                .ModalKeepsIcon()
                .ModalKeepsFooter()
                .OpenInSource("Open in Box", inNewTab => Toast().Information(inNewTab ? $"Opening \"{hit.Title}\" in a new tab" : $"Opening \"{hit.Title}\" in Box"), UIcons.ArrowUpRightFromSquare)
                .OpenInSource("Open on the web", _ => new System.Uri("https://github.com/curiosity-ai/tesserae"), UIcons.Globe)
                .ModalCommands(r => r.ShowMenu(ContextMenu().Items(
                    ContextMenuItem("Pin").OnClick(() => Toast().Information($"Pinned \"{r.Result.Title}\"")),
                    ContextMenuItem("Share").OnClick(() => Toast().Information($"Shared \"{r.Result.Title}\"")),
                    ContextMenuItem("Download").OnClick(() => Toast().Information($"Downloading \"{r.Result.Title}\"")))))
                .ModalNavigation(
                    index > 0 ? new Action<OmniResult<Hit>>(_ => StepModalTo(index - 1)) : null,
                    index < 2 ? new Action<OmniResult<Hit>>(_ => StepModalTo(index + 1)) : null,
                    index + 1,
                    3);

            return row.OnClick((_, __) => OpenModal(index, replaceTop: false));
        }

        private IComponent ModalBody(OmniResult<Hit> result, int index)
        {
            var metadata = result.Result.Metadata;

            return VStack().WS().P(16).Children(
                DetailsGrid()
                    .Row("Location", metadata.Length > 0 ? metadata[0] : null)
                    .Row("Size",     metadata.Length > 1 ? metadata[1] : null)
                    .Row("Owner",    metadata.Length > 2 ? metadata[2] : null)
                    .Row("Modified", metadata.Length > 3 ? metadata[3] : null)
                    .Row("Pages",    Badge($"{result.Result.Pages}").Pill()),
                TextBlock(result.Result.Text).MT(16),
                Button("Open a related result").Primary().MT(16).OnClick(() => OpenModal((index + 1) % _modalRows.Count, replaceTop: false)));
        }

        // Stepping through the results swaps the sheet in front and leaves the chain behind it alone;
        // opening something out of one puts a new sheet on top of it.
        private void StepModalTo(int index) => OpenModal(index, replaceTop: true);

        private void OpenModal(int index, bool replaceTop)
        {
            var row   = _modalRows[index];
            var modal = row.ToModal();

            if (modal is null) return;

            if (replaceTop)
            {
                ModalStack.Replace($"hit-{index}", row.Title, modal);
            }
            else
            {
                ModalStack.Push($"hit-{index}", row.Title, modal);
            }
        }

        // ---------- Sources ----------

        private IComponent Sources()
        {
            return FeatureCard("Sources", "Naming where a result came from, and scoping to it",
                "SetSource(color, text) puts a small rounded square in that color and the source's name at the start of the footer. Passing a handler as well makes the source clickable — scoping the search to it is the usual thing to do — without the click counting as opening the result: it takes a tab stop of its own, answers Enter and Space, and underlines while hovered. OnSourceClick(handler) sets the same handler on its own, and OnSourceClick(null) makes the source plain text again.",
                VStack().WS().Children(
                    OmniResult(Hits[1], "A clickable source — click \"Box\"")
                        .SetIcon(UIcons.FilePdf, "#ef4444")
                        .SetSource("#0061d5", "Box", r => ReportSource("Box", r))
                        .SetFooterEntries(Hits[1].Metadata)
                        .OnClick((r, _) => Toast().Information($"Opening {r.Result.Title}")),
                    OmniResult(Hits[2], "Another source, another color")
                        .SetIcon("XLSX", "#16a34a")
                        .SetSource("#1a73e8", "Drive", r => ReportSource("Drive", r))
                        .SetFooterEntries(Hits[2].Metadata)
                        .OnClick((r, _) => Toast().Information($"Opening {r.Result.Title}")),
                    OmniResult(Hits[3], "A plain source — nothing to click")
                        .SetIcon("DOCX", "#3b82f6")
                        .SetSource("#7b83eb", "Teams")
                        .SetFooterEntries(Hits[3].Metadata)
                        .OnClick((r, _) => Toast().Information($"Opening {r.Result.Title}")),
                    OmniResult(Hits[6], "No source at all — the footer starts with its first entry")
                        .SetIcon("TXT", "#94a3b8")
                        .SetFooterEntries(Hits[6].Metadata),
                    OmniResult(Hits[0], "A marker of your own, and one on the tile's corner")
                        .SetIcon(UIcons.Folder, "#6366f1")
                        .SetIconBadge(Image("./assets/img/box-img.svg"))
                        .SetSource(Image("./assets/img/box-img.svg"), "Box", r => ReportSource("Box", r))
                        .SetFooterEntries(Hits[0].Metadata)),
                _sourceState.MT(8));
        }

        private void ReportSource(string source, OmniResult<Hit> row)
        {
            _sourceState.Text = $"Scoping the search to {source}, from \"{row.Result.Title}\".";
            Toast().Information(_sourceState.Text);
        }

        // ---------- Contribution bars ----------

        private IComponent Contributions()
        {
            return FeatureCard("Contribution bar", "What the score is made of, under the footer",
                "SetContributionBar attaches a ContributionBar below the footer, spanning the text column so it lines up with the title and the excerpt rather than running under the icon and the pages rail. It is the row's place for a relevance breakdown: how much of the score came from the title, the content, how recent the document is, how often it is opened. Clicking the bar's own toggle never counts as opening the result.",
                VStack().WS().Children(
                    Row(Hits[1], withText: false, withPages: false)
                        .SetContributionBar(Relevance()),
                    Row(Hits[2], withText: false, withPages: false)
                        .SetContributionBar(Relevance().ShowValues(false)),
                    Row(Hits[3], withText: false, withPages: false)
                        .SetContributionBar(Relevance().HideLegend().Thickness(6.px()))),
                TextBlock("Collapsable(), for a list that should read as one line per result until a breakdown is asked for — the first expands in place, the second shows it in a popover on hover:").Small().MT(12).MB(8),
                VStack().WS().Children(
                    Row(Hits[5], withText: false, withPages: false)
                        .SetContributionBar(Relevance().Collapsable()),
                    Row(Hits[6], withText: false, withPages: false)
                        .SetContributionBar(Relevance().Collapsable(reveal: ContributionBarReveal.Tooltip))),
                TextBlock("On a full row, under the excerpt's footer and beside the page preview:").Small().MT(12).MB(8),
                VStack().WS().Children(
                    Row(Hits[1], withText: true, withPages: true)
                        .SetContributionBar(Relevance().Collapsable())));
        }

        // The same breakdown every row in the section shows, freshly built per row.
        private static ContributionBar Relevance()
        {
            return ContributionBar()
                .Add("Title match", 42)
                .Add("Content match", 31)
                .Add("Recency", 18)
                .Add("Opened often", 9)
                .Max(100)
                .Decimals(0);
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
                OmniResultSelectionMode.ReplacingIcon,
                OmniResultSelectionMode.AlwaysBeforeHeaderIcon,
                OmniResultSelectionMode.HiddenBeforeHeaderIcon
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

                    //The first row of each group is badged, so what a corner badge does when the checkbox
                    //takes the tile's place is visible in every mode.
                    if (i == 0) row.SetIconBadge(Icon(UIcons.Star, UIconsWeight.Solid, color: "#f0c000"), OmniResultBadgeCorner.TopRight);

                    _selectable.Add(row);
                    list.Add(row);
                }

                sections.Add(TextBlock($"OmniResultSelectionMode.{mode}").Small().SemiBold().MT(m == 0 ? 0 : 12).MB(4));
                sections.Add(list);
            }

            return FeatureCard("Selection", "Six places for the checkbox, and two for the tile",
                "Selectable(mode) makes a row selectable. The checkbox sits before the tile or over it, revealed on hover or always visible, or takes the tile's place entirely — and a selected row always shows its checkbox, whatever the mode. A corner badge marks the result rather than the tile, so it follows the checkbox where the checkbox stands in for the tile: the starred row in each group keeps its star over the checkbox. The last two modes move the tile itself: it leads the header line, drawn small before the title, so the excerpt and the footer start at the row's own left edge instead of indented past a 34px tile — with the checkbox always visible beside it, or with no checkbox at all. Ctrl-clicking a row toggles it too, and shift-clicking one asks for the range from the last row selected: a single card knows nothing about its siblings, so OnRangeSelectionRequested hands that to the host list, which selects them itself (that is what the rows below do, across all six groups).",
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

            var slides = new[]
            {
                "./assets/img/slide-16-9.svg",
                "./assets/img/slide-16-9.svg",
                "./assets/img/slide-16-9.svg",
                "./assets/img/slide-16-9.svg"
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
                TextBlock("The pages take their shape from the document: the first thumbnail that loads wider than it is tall turns the whole stack landscape, keeping the long side of the page size and taking the short one from the thumbnail's aspect ratio — so a deck of slides isn't drawn as a pile of portrait pages. MatchThumbnailShape(false) keeps the configured size whatever loads:").Small().MT(8).MB(8),
                HStack().WS().Wrap().Gap(32.px()).PB(8).AlignItems(ItemAlign.End).Children(
                    PagesLabel("16:9 slides", PagesStack(slides).TotalPages(18)),
                    PagesLabel("Larger slides", PagesStack(slides).PageSize(60, 78)),
                    PagesLabel("MatchThumbnailShape(false)", PagesStack(slides).MatchThumbnailShape(false))),
                TextBlock("The reshaping is a rail the row was already holding open for the fan, so a deck previews in a result row the same way a document does:").Small().MT(8).MB(8),
                OmniResult(Hits[1], "Q3 line review.pptx")
                   .SetIcon("PPTX", "#f97316")
                   .SetSource("#0061d5", "Box")
                   .SetFooterEntries("18 slides", "4.1 MB", "Sep 30, 2024")
                   .SetPages(PagesStack(slides).TotalPages(18).OnPageClick(page => Toast().Information($"Opening slide {page + 1}"))),
                TextBlock("Held open with Fanned(), which is how OmniResult makes the stack follow hovering the whole row (PagesFanOnHover, on by default). OnPageClick makes each drawn page open the document at itself — the click is the page's alone, so it never also counts as a click on the row:").Small().MT(8).MB(8),
                HStack().WS().Wrap().Gap(32.px()).AlignItems(ItemAlign.End).Children(
                    PagesLabel("Fanned()", PagesStack(5).TotalPages(19).Fanned()),
                    PagesLabel("OnPageClick", PagesStack(thumbnails).TotalPages(9).OnPageClick(page => Toast().Information($"Opening page {page + 1}")))));
        }

        // ---------- Inline labels ----------

        private IComponent InlineLabels()
        {
            return FeatureCard("InlineLabel", "What a footer is a line of",
                "The footer's entries are InlineLabels: an optional mark - a glyph, an image, or a rounded square of colour - followed by optional text, drawn small and separated by the dot the footer puts between them. The same label outside a footer is a compact button instead; the InlineLabel sample has the whole set.",
                OmniResult(Hits[1], Hits[1].Title)
                    .SetIcon("PDF", "#ef4444")
                    .SetSource("#0061d5", "Box")
                    .SetFooterEntries(
                        InlineLabel("sample-files / pdfs").SetIcon(UIcons.Folder).OnClick(_ => Toast().Information("Opening the folder")),
                        InlineLabel("2.4 MB"),
                        InlineLabel("Pius Neuhaus").SetIcon(UIcons.User),
                        InlineLabel("Apr 12, 2024")));
        }

        // ---------- Inline pagination ----------

        private IComponent InlinePaginationSection()
        {
            var stepped = InlinePagination(3, 7)
               .OnPrevious(p => Step(p, -1))
               .OnNext(p => Step(p, +1));

            return FeatureCard("InlinePagination", "Stepping through a set, in one pill",
                "InlinePagination is the \"3 of 7\" control the modal's header uses for its previous/next arrows, and it stands on its own wherever a toolbar needs to step through something one at a time - a lightbox, an editor, a preview. Each chevron is enabled by having a handler, so leaving one out is how the first and the last of a set say so; the position and the count only write the label.",
                HStack().WS().Wrap().Gap(24.px()).AlignItemsCenter().PT(8).PB(8).Children(
                    InlinePagination(3, 7).OnPrevious(_ => { }).OnNext(_ => { }),
                    InlinePagination(1, 7).OnNext(_ => { }),
                    InlinePagination(7, 7).OnPrevious(_ => { }),
                    InlinePagination().OnPrevious(_ => { }).OnNext(_ => { }),
                    InlinePagination(3, 7).SetFormat((position, count) => $"{position} / {count}").OnPrevious(_ => { }).OnNext(_ => { }),
                    InlinePagination().SetLabel("March").OnPrevious(_ => { }).OnNext(_ => { })),
                TextBlock("In order: both ways, the first of a set, the last of it, no count at all, another format, and a label of the host's own. The one below actually steps:").Small().MT(8).MB(8),
                stepped);
        }

        private static void Step(InlinePagination pagination, int by)
        {
            var next = pagination.Position + by;

            if (next < 1 || next > pagination.Count) return;

            pagination.SetPosition(next, pagination.Count)
               .OnPrevious(next > 1 ? new Action<InlinePagination>(p => Step(p, -1)) : null)
               .OnNext(next < pagination.Count ? new Action<InlinePagination>(p => Step(p, +1)) : null);
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
                .SetSource("#0061d5", "Box", r => Toast().Information($"Scoping the search to Box, from \"{r.Result.Title}\""))
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
