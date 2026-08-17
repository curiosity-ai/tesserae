using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 32, Icon = UIcons.Disk)]
    public class UnsavedChangesGuardSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public UnsavedChangesGuardSample()
        {
            var isDirty    = new SettableObservable<bool>(false);
            var status     = TextBlock("Clean ✔");
            var attemptLog = TextBlock("");
            const string key = "sample-editor";

            var textBox = TextBox("");
            textBox.OnInput((s, e) =>
            {
                isDirty.Value = textBox.Text.Length > 0;
                status.Text   = isDirty.Value ? "Dirty — has unsaved changes ●" : "Clean ✔";
            });

            UnsavedChangesGuard.Track(
                key:     key,
                name:    () => "Sample editor",
                isDirty: () => isDirty.Value,
                save: () =>
                {
                    isDirty.Value = false;
                    status.Text   = "Clean ✔ (just saved)";
                    return System.Threading.Tasks.Task.FromResult(true);
                });

            var btnTryLeave = Button("Simulate navigating away").OnClick((s, e) =>
            {
                var allowed = UnsavedChangesGuard.CanNavigateAway(new Router.State("#/elsewhere"));
                attemptLog.Text = allowed
                    ? "Nothing was dirty — navigation would proceed immediately."
                    : "Navigation was blocked — the confirmation dialog took over.";
            });

            // TabSaveIndicator side: a Pivot tab shows the same "*" a real hosting
            // view (e.g. one tab per open editor) would toggle.
            const string tabId  = "tab-sample-doc";
            var tabTextBox      = TextBox("");
            tabTextBox.OnInput((s, e) =>
            {
                if (tabTextBox.Text.Length > 0) TabSaveIndicator.MarkDirty(tabId); else TabSaveIndicator.MarkClean(tabId);
            });

            TabSaveIndicator.OnSave(tabId, () =>
            {
                TabSaveIndicator.MarkClean(tabId);
                return System.Threading.Tasks.Task.FromResult(true);
            });

            var pivot = Pivot()
               .Pivot("doc", () => TextBlock("Document").Id(tabId), () => Card(VStack().WS().Children(
                    TextBlock("Typing below marks this tab dirty; the pivot title shows a \"*\" via TabSaveIndicator.MarkDirty."),
                    tabTextBox)));

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(UnsavedChangesGuardSample), UIcons.Disk, "Warn before losing an editor's unsaved changes")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("UnsavedChangesGuard stops an editor's changes from being lost silently: a beforeunload listener warns on tab close/reload, and — once wired into Router.OnBeforeNavigate — CanNavigateAway blocks in-app navigation until the user saves, discards, or stays. TabSaveIndicator is the companion for editors hosted as Pivot tabs: it toggles a \"*\" on the tab title and lets the guard save a tab without knowing what kind of editor it holds."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Always pair Track(...) with Forget(key) on the editor's close path, and TrackOpenTabs()/ForgetOpenTabs() on the hosting view's mount/removal — an editor that never calls Forget keeps the guard asking about it forever. Router only keeps one OnBeforeNavigate handler, so call CanNavigateAway explicitly from inside whatever handler the app already registers."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Track() a single editor"),
                        VStack().Gap(8.px()).Children(
                            Label("Type to make this editor dirty").SetContent(textBox),
                            status,
                            btnTryLeave,
                            attemptLog))).SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("TabSaveIndicator on a Pivot tab"),
                        pivot)).SetTitle("Usage")))
               .SeeAlso(typeof(PivotSample), typeof(ValidatorSample), typeof(SaveButtonSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
