using System;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Inputs, Order = 35, Icon = UIcons.Microphone, Description = "Find the text field to dictate into from anywhere")]
    public class DictationTargetsSample : IComponent, ISample
    {
        private readonly IComponent _content;
        private readonly TextBlock  _status = TextBlock().Id("dictation-targets-status");

        public DictationTargetsSample()
        {
            // Each field is dictated into by this scripted source, so the sample works in any browser.
            var withButton    = TextBox().SetPlaceholder("Text box with a microphone button").WithDictation(ScriptedSpeech).Id("dt-textbox-button");
            var withoutButton = TextBox().SetPlaceholder("Text box with no button").DictationSource(ScriptedSpeech).Id("dt-textbox-plain");
            var area          = TextArea().SetPlaceholder("Text area with a microphone button").WithDictation(ScriptedSpeech).Id("dt-textarea");

            var omni = OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat   = "OmniBox with a microphone button",
                DictationButton   = true,
                DictationProvider = ScriptedSpeech
            }).WS().Id("dt-omnibox");

            // On the page, but not visible: All() has it, Visible() does not.
            var hiddenField = TextBox().SetPlaceholder("Hidden text box").DictationSource(ScriptedSpeech).Id("dt-textbox-hidden");
            var hiddenHost  = VStack().Children(hiddenField).Collapse();

            var showHidden = Toggle().Id("dt-show-hidden").OnChange((s, e) =>
            {
                if (s.IsChecked) hiddenHost.Show();
                else hiddenHost.Collapse();

                Refresh();
            });

            // A click on a button would move focus to it, and the point is to act on the field that had it.
            IComponent KeepFocus(IComponent button)
            {
                button.Render().addEventListener("mousedown", (Event e) => e.preventDefault());
                return button;
            }

            var dictate = Button("Dictate into the current field").Primary().Id("dt-dictate").OnClick((_, __) =>
            {
                var target = DictationTargets.Current();

                if (target is null) Toast().Information("No field to dictate into");
                else target.BeginDictation();

                Refresh();
            });

            var stop = Button("Stop").Id("dt-stop").OnClick((_, __) =>
            {
                foreach (var target in DictationTargets.All()) target.EndDictation();

                Refresh();
            });

            var clear = Button("Clear all").Id("dt-clear").OnClick((_, __) =>
            {
                foreach (var target in DictationTargets.All()) target.Text = string.Empty;

                Refresh();
            });

            // The page is a single document, so the listeners outlive this sample; they do nothing once it is gone.
            document.addEventListener("focusin",  (Event _) => { if (_status.Render().isConnected) Refresh(); });
            document.addEventListener("focusout", (Event _) => window.setTimeout(__ => { if (_status.Render().isConnected) Refresh(); }, 0));

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(DictationTargetsSample), UIcons.Microphone, "Find the text field to dictate into, from anywhere on the page")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Every TextBox, TextArea and OmniBox on the page is an IDictationTarget, wherever it was built, and DictationTargets finds them: each one puts itself in a hidden field of its root element when it is created, so a query is one querySelectorAll and only ever returns what is on the page right now."),
                        TextBlock("An application that transcribes speech - behind a shortcut, a push-to-talk button, a voice command - does not need to hold on to the fields. DictationTargets.Current() returns the field that has keyboard focus, and when there is none the first visible one that takes text. Then target.BeginDictation() starts dictating into it, whether or not the field shows a microphone button.").MT(6))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Fields"),
                        Label("Text box with a microphone button").SetContent(withButton),
                        Label("Text box with no button").SetContent(withoutButton),
                        Label("Text area with a microphone button").SetContent(area),
                        Label("OmniBox with a microphone button").SetContent(omni),
                        Label("Hidden text box").SetContent(HStack().AlignItemsCenter().Children(showHidden, TextBlock("Show it").ML(8))),
                        hiddenHost,
                        SampleSubTitle("Dictate into the current field"),
                        TextBlock("Click into a field, then press the button: the buttons do not take focus, so the field you were in is the one that is dictated into. With no field focused, the first visible one is used."),
                        HStack().Children(KeepFocus(dictate), KeepFocus(stop), KeepFocus(clear)),
                        SampleSubTitle("What DictationTargets sees"),
                        _status)).SetTitle("Try it")));

            // The page is only searched once it is on the document.
            DomObserver.WhenMounted(_status.Render(), Refresh);
        }

        private void Refresh()
        {
            var focused = DictationTargets.Focused();
            var current = DictationTargets.Current();

            _status.Text = $"Focused: {Describe(focused)} | Current: {Describe(current)} | Visible: {DictationTargets.Visible().Count} of {DictationTargets.All().Count} | Dictating: {DictationTargets.All().Count(t => t.IsDictating)}";
        }

        private static string Describe(IDictationTarget target)
        {
            switch (target)
            {
                case null:        return "none";
                case TextBox t:   return "TextBox \"" + t.Placeholder + "\"";
                case TextArea a:  return "TextArea \"" + a.Placeholder + "\"";
                case OmniBox _:   return "OmniBox";
                default:          return target.GetType().Name;
            }
        }

        private static Action ScriptedSpeech(DictationSession session)
        {
            var words  = new[] { "this", "was", "dictated", "into", "the", "current", "field" };
            var heard  = 0;
            var handle = 0.0;

            Action step = null;
            step = () =>
            {
                heard++;
                session.SetTranscript(string.Join(" ", words.Take(heard)), heard == words.Length);
                if (heard == words.Length) session.End();
                else handle = window.setTimeout(_ => step(), 250);
            };

            handle = window.setTimeout(_ => step(), 250);
            return () => window.clearTimeout(handle);
        }

        public HTMLElement Render() => _content.Render();
    }
}
