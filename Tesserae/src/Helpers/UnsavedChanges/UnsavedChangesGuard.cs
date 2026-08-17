using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TNT;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static TNT.T;

namespace Tesserae
{
    /// <summary>
    /// Stops an editor carrying unsaved changes from being thrown away without the user
    /// saying so. Two ways of losing one are covered:
    ///
    /// <list type="bullet">
    /// <item>reloading / closing the browser tab — a <c>beforeunload</c> listener asks
    /// the browser to show its native "leave site?" prompt;</item>
    /// <item>navigating to another route — <see cref="CanNavigateAway"/> (wire it into
    /// <see cref="Router.OnBeforeNavigate"/>) cancels the navigation, asks whether to
    /// save, and only then lets it through.</item>
    /// </list>
    ///
    /// There are two ways to feed it. A modal or a standalone editor registers itself with
    /// <see cref="Track"/> and drops out with <see cref="Forget"/>. A view hosting editors
    /// as pivot tabs instead calls <see cref="TrackOpenTabs"/>, which reads the dirty tabs
    /// straight off the DOM via <see cref="TabSaveIndicator"/> — tabs come and go faster
    /// than registrations can be kept honest, and a torn-down tab can't leave a stale entry
    /// behind that way.
    ///
    /// Closing a modal directly (its own close button, a light-dismiss click) is NOT covered
    /// here — that path has to ask its own confirmation before the modal hides, since by the
    /// time it is gone there is nothing left to intercept.
    /// </summary>
    [Transpose.Name("tss.UnsavedChangesGuard")]
    public static class UnsavedChangesGuard
    {
        private static Action<Event> _beforeUnload;

        // Editors that registered themselves, keyed so re-opening the same one replaces it.
        private static readonly Dictionary<string, TrackedEditor> _tracked = new Dictionary<string, TrackedEditor>();

        // Whether to also treat the dirty pivot tabs found in the DOM as unsaved editors.
        private static bool _watchingOpenTabs;

        // The navigation the user already confirmed: the router hook has to cancel the
        // first attempt (it can't await the dialog), so the re-issued navigation needs a
        // way past the guard while the editors are still dirty.
        private static string _confirmedTarget;
        private static bool   _asking;

        /// <summary>
        /// Registers a single editor for as long as it is on screen. <paramref name="key"/>
        /// identifies it (re-opening the same editor replaces its entry),
        /// <paramref name="name"/> is what the user is told has unsaved changes, and
        /// <paramref name="save"/> returns whether the save actually went through.
        /// Always pair this with <see cref="Forget"/> on the editor's close / removal path.
        /// </summary>
        public static void Track(string key, Func<string> name, Func<bool> isDirty, Func<Task<bool>> save)
        {
            if (string.IsNullOrEmpty(key) || isDirty is null) return;

            _tracked[key] = new TrackedEditor { Name = name, IsDirty = isDirty, Save = save };
            RefreshUnloadListener();
        }

        /// <summary>Drops a <see cref="Track"/>ed editor — call it when that editor closes.</summary>
        public static void Forget(string key)
        {
            if (string.IsNullOrEmpty(key) || !_tracked.Remove(key)) return;
            RefreshUnloadListener();
        }

        /// <summary>Starts watching the open editor tabs — call it when the hosting view is mounted.</summary>
        public static void TrackOpenTabs()
        {
            _watchingOpenTabs = true;
            RefreshUnloadListener();
        }

        /// <summary>Stops watching the open editor tabs — call it when the hosting view is removed.</summary>
        public static void ForgetOpenTabs()
        {
            _watchingOpenTabs = false;
            _confirmedTarget  = null;
            RefreshUnloadListener();
        }

        /// <summary>
        /// A <see cref="Router.OnBeforeNavigate"/> answer. Returns true when nothing would be
        /// lost; otherwise cancels this navigation and asks the user what to do with the
        /// unsaved editors, re-issuing the navigation once they have decided.
        /// </summary>
        public static bool CanNavigateAway(Router.State target)
        {
            if (target is null) return true;

            if (_confirmedTarget is object && SameRoute(_confirmedTarget, target.FullPath))
            {
                _confirmedTarget = null;
                return true;
            }

            var unsaved = CollectUnsaved();

            if (unsaved.Count == 0) return true;
            if (_asking) return false;

            AskThenNavigateAsync(target.FullPath, unsaved).FireAndForget();
            return false;
        }

        /// <summary>Every editor that currently holds changes the user has not saved.</summary>
        private static List<UnsavedEditor> CollectUnsaved()
        {
            var unsaved = new List<UnsavedEditor>();

            if (_watchingOpenTabs)
            {
                foreach (var dirtyTabId in TabSaveIndicator.DirtyTabIds())
                {
                    var tabIndicatorId = dirtyTabId;

                    unsaved.Add(new UnsavedEditor(
                        TabSaveIndicator.TitleOf(tabIndicatorId),
                        TabSaveIndicator.CanSave(tabIndicatorId),
                        () => TabSaveIndicator.SaveAsync(tabIndicatorId)));
                }
            }

            foreach (var editor in _tracked.Values)
            {
                if (!editor.IsDirty()) continue;
                unsaved.Add(new UnsavedEditor(editor.Name?.Invoke(), editor.Save is object, editor.Save));
            }

            return unsaved;
        }

        /// <summary>
        /// The <c>beforeunload</c> listener only needs to exist while something is being
        /// watched, so adding and removing it follows the two registration paths.
        /// </summary>
        private static void RefreshUnloadListener()
        {
            var needed = _watchingOpenTabs || _tracked.Count > 0;

            if (needed == (_beforeUnload is object)) return;

            if (needed)
            {
                _beforeUnload = ev =>
                {
                    if (CollectUnsaved().Count == 0) return;
                    ev.preventDefault();
                    // Some browsers still need `returnValue` set to trigger the prompt.
                    Transpose.Script.Write("{0}.returnValue = ''", ev);
                };

                window.addEventListener("beforeunload", _beforeUnload);
            }
            else
            {
                window.removeEventListener("beforeunload", _beforeUnload);
                _beforeUnload = null;
            }
        }

        private static async Task AskThenNavigateAsync(string targetFullPath, List<UnsavedEditor> unsaved)
        {
            _asking = true;

            try
            {
                var response = await Dialog(
                        title: TextBlock("Unsaved changes".t()).SemiBold(),
                        content: TextBlock(DescribeUnsaved(unsaved)))
                   .Dark()
                   .YesNoCancelAsync(
                        btnYes: b =>
                        {
                            b.SetText("Save and leave".t()).SetIcon(UIcons.Disk).Primary();
                            if (!unsaved.All(e => e.CanSave)) b.Disabled();
                            return b;
                        },
                        btnNo: b => b.SetText("Leave without saving".t()).Danger(),
                        btnCancel: b => b.SetText("Stay here".t()));

                if (response != Dialog.Response.Yes && response != Dialog.Response.No) return;
                if (response == Dialog.Response.Yes && !await SaveAllAsync(unsaved)) return;

                _confirmedTarget = targetFullPath;
                Router.Navigate(targetFullPath);
            }
            finally
            {
                _asking = false;
            }
        }

        /// <summary>
        /// Saves every unsaved editor, stopping at the first one that refuses. Editors say
        /// what went wrong themselves, but not all of them do — so name the one we stopped
        /// on, otherwise "Save and leave" would look like it did nothing.
        /// </summary>
        private static async Task<bool> SaveAllAsync(List<UnsavedEditor> unsaved)
        {
            foreach (var editor in unsaved)
            {
                bool saved;

                // A save that throws is a save that didn't happen: report it and stay put,
                // rather than letting the rejection escape and navigating away regardless.
                try
                {
                    saved = editor.Save is object && await editor.Save();
                }
                catch (Exception e)
                {
                    Toast().Error("Failed to save".t(), e.Message);
                    return false;
                }

                if (saved) continue;

                Toast().Warning("Not saved".t(), string.IsNullOrEmpty(editor.Name)
                    ? "An editor could not be saved, so you are still on this page.".t()
                    : string.Format("\"{0}\" could not be saved, so you are still on this page.".t(), editor.Name));

                return false;
            }

            return true;
        }

        /// <summary>
        /// Whether two full URLs address the same route. Only the hash carries the route, and
        /// the browser may hand back a differently-spelled absolute URL for the same one.
        /// </summary>
        private static bool SameRoute(string a, string b)
        {
            if (a == b) return true;

            var hashA = a.IndexOf('#');
            var hashB = b.IndexOf('#');

            return hashA >= 0 && hashB >= 0 && a.Substring(hashA) == b.Substring(hashB);
        }

        private static string DescribeUnsaved(List<UnsavedEditor> unsaved)
        {
            var names = new List<string>(unsaved.Count);

            foreach (var editor in unsaved)
            {
                if (!string.IsNullOrEmpty(editor.Name)) names.Add(editor.Name);
            }

            if (names.Count == 0) return "You have unsaved changes that will be lost if you leave this page. Do you want to save them first?".t();
            if (names.Count == 1) return string.Format("\"{0}\" has unsaved changes that will be lost if you leave this page. Do you want to save it first?".t(), names[0]);

            return string.Format("{0} have unsaved changes that will be lost if you leave this page. Do you want to save them first?".t(), string.Join(", ", names.Select(n => "\"" + n + "\"")));
        }

        private sealed class TrackedEditor
        {
            public Func<string>     Name    { get; set; }
            public Func<bool>       IsDirty { get; set; }
            public Func<Task<bool>> Save    { get; set; }
        }

        /// <summary>One editor holding unsaved changes, flattened from whichever source found it.</summary>
        private sealed class UnsavedEditor
        {
            public UnsavedEditor(string name, bool canSave, Func<Task<bool>> save)
            {
                Name    = name;
                CanSave = canSave;
                Save    = save;
            }

            public string           Name    { get; }
            public bool             CanSave { get; }
            public Func<Task<bool>> Save    { get; }
        }
    }
}
