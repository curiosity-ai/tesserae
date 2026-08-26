using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Search, Order = 60, Icon = UIcons.HighlighterLine, Description = "Mark keyword matches inside rendered content")]
    public class MarkHighlighterSample : IComponent, ISample
    {
        private readonly IComponent _content;

        private readonly List<MarkedMatch> _matches = new List<MarkedMatch>();
        private          int         _currentMatch;
        private          string      _currentTerm = "";
        private          TextBlock   _matchSummary;
        private          HTMLElement _searchRoot;

        private CheckBox _wholeWord;
        private CheckBox _separateWords;
        private CheckBox _acrossElements;
        private CheckBox _wildcards;
        private CheckBox _useElements;

        private CancellationTokenSource _searchCTS;

        public MarkHighlighterSample()
        {
            var document = VStack().WS().Children(
                TextBlock("The Tesserae toolkit").MediumPlus().SemiBold(),
                TextBlock("Tesserae is a UI toolkit for building web applications in C#. A Tesserae application composes components, and each component renders plain DOM elements.").PT(8),
                TextBlock("Highlighting works across element boundaries too: this paragraph mentions the toolkit, the toolkit's components, and the café down the street - searching for 'cafe' finds the accented spelling as well.").PT(8),
                Raw(SplitPhraseParagraph()).PT(8),
                TextBlock("Justified documents carry soft hyphens: this docu­ment only matches 'document' when joiners are ignored.").PT(8),
                TextBlock("Repeated words are all found: toolkit, toolkit, toolkit.").PT(8));

            _searchRoot   = document.Render();
            _matchSummary = TextBlock("no matches").TextCenter().NoWrap().W(80);

            var previous = Button().SetIcon(UIcons.AngleLeft).Id("mark-highlighter-previous").OnClick(() => FocusMatch(_currentMatch - 1));
            var next     = Button().SetIcon(UIcons.AngleRight).Id("mark-highlighter-next").OnClick(() => FocusMatch(_currentMatch + 1));

            var searchBox = SearchBox("Find in document...").SearchAsYouType().OnSearch((sb, term) =>
            {
                _currentTerm = term;
                DoSearchAsync().FireAndForget();
            });

            _wholeWord      = CheckBox("Whole word").Id("mark-highlighter-whole-word").OnChange((s, e) => DoSearchAsync().FireAndForget());
            _separateWords  = CheckBox("Separate words").Id("mark-highlighter-separate-words").OnChange((s, e) => DoSearchAsync().FireAndForget());
            _acrossElements = CheckBox("Across elements").Id("mark-highlighter-across-elements").OnChange((s, e) => DoSearchAsync().FireAndForget());
            _wildcards      = CheckBox("Wildcards (* ?)").Id("mark-highlighter-wildcards").OnChange((s, e) => DoSearchAsync().FireAndForget());
            _useElements    = CheckBox("Use mark elements").Id("mark-highlighter-use-elements").OnChange((s, e) => DoSearchAsync().FireAndForget());

            var backendNote = TextBlock(MarkHighlighter.IsHighlightApiSupported
                ? "This browser supports the CSS Custom Highlight API - matches are painted through CSS.highlights with no DOM mutation, unless 'Use mark elements' forces the classic wrappers."
                : "This browser has no CSS Custom Highlight API - matches are wrapped in mark elements.").XSmall();

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(MarkHighlighterSample), UIcons.HighlighterLine, "A helper to mark keyword matches in the DOM")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("MarkHighlighter marks every occurrence of a keyword inside a DOM subtree by wrapping the matching text in mark elements - same-origin iframes included - and unmarks them again by unwrapping. Matching is case-insensitive, folds diacritics (searching 'cafe' finds 'café'), and merges whitespace runs. MarkOptions adds whole-word boundaries, per-word matching, wildcards, joiner folding, a minimum keyword length, and matching across element boundaries."),
                    TextBlock("MarkAsync marks and reports each mark element through a callback; UnmarkAsync removes all marks; FocusResult moves the focused-match highlight between them. Passes on the same root are serialized, so calling on every keystroke is safe. RegExpCreator builds the underlying regular expression and DOMIterator walks the subtree; both are usable on their own."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Find in document"),
                    TextBlock("Type in the box below to mark matches in the text; use the arrows to move the focused match. The soft-hyphen paragraph is matched with IgnoreJoiners, which this demo always enables; try 'web applications' with and without 'Across elements', or 'tool*' with wildcards."),
                    HStack().AlignItemsCenter().PT(8).Children(searchBox.W(250), previous.PL(8), _matchSummary, next),
                    HStack().AlignItemsCenter().PT(8).Children(_wholeWord, _separateWords.PL(12), _acrossElements.PL(12), _wildcards.PL(12), _useElements.PL(12)),
                    backendNote.PT(8),
                    Raw(_searchRoot).PT(16)
                )).SetTitle("Usage")))
               .SeeAlso(typeof(SearchBoxSample), typeof(OmniResultSample), typeof(SearchableListSample));
        }

        private static HTMLElement SplitPhraseParagraph()
        {
            var p = document.createElement("p");
            p.innerHTML = "Cross-element matching: with 'Across elements' on, searching for 'web applications' also finds <strong>web</strong> applications split by inline tags.";
            return p;
        }

        private MarkOptions BuildOptions() => new MarkOptions
        {
            WholeWord          = _wholeWord.IsChecked,
            SeparateWordSearch = _separateWords.IsChecked,
            AcrossElements     = _acrossElements.IsChecked,
            Wildcards          = _wildcards.IsChecked,
            IgnoreJoiners      = true,
            UseHighlightApi    = _useElements.IsChecked ? false : (bool?)null
        };

        private async Task DoSearchAsync()
        {
            _searchCTS?.Cancel();
            _searchCTS = new CancellationTokenSource();

            _matches.Clear();
            _currentMatch = 0;
            await MarkHighlighter.UnmarkAsync(_searchRoot, cancellationToken: _searchCTS.Token);

            if (!string.IsNullOrWhiteSpace(_currentTerm))
            {
                await MarkHighlighter.MarkAsync(_searchRoot, _currentTerm, BuildOptions(), match => _matches.Add(match), _searchCTS.Token);
            }

            if (_matches.Count > 0)
            {
                MarkHighlighter.FocusResult(_searchRoot, _matches[0], scrollIntoViewIfNeeded: false);
            }
            RefreshMatchSummary();
        }

        private void FocusMatch(int index)
        {
            if (_matches.Count == 0) return;

            _currentMatch = Math.Max(0, Math.Min(index, _matches.Count - 1));
            MarkHighlighter.FocusResult(_searchRoot, _matches[_currentMatch], scrollIntoViewIfNeeded: true);
            RefreshMatchSummary();
        }

        private void RefreshMatchSummary()
        {
            switch (_matches.Count)
            {
                case 0: _matchSummary.Text  = "no matches"; break;
                case 1: _matchSummary.Text  = "1 match"; break;
                default: _matchSummary.Text = (_currentMatch + 1) + " / " + _matches.Count; break;
            }
        }

        public HTMLElement Render() => _content.Render();
    }
}
