using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// One sheet in the <see cref="ModalStack"/>: the modal, the key it was pushed under, and the name the
    /// chain shows for it.
    /// </summary>
    [Transpose.Name("tss.ModalStackEntry")]
    public sealed class ModalStackEntry
    {
        internal ModalStackEntry(string key, string name, Modal modal)
        {
            Key   = key;
            Name  = name;
            Modal = modal;
            Sheet = modal.StylingContainer;
        }

        /// <summary>Gets what this sheet was pushed under - what makes it the same sheet on a later push.</summary>
        public string Key { get; }

        /// <summary>Gets the name the chain shows for this sheet - its title, in one short line.</summary>
        public string Name { get; internal set; }

        /// <summary>Gets the modal this sheet shows.</summary>
        public Modal Modal { get; }

        internal HTMLElement Sheet { get; }

        internal HTMLElement Tab;
    }

    /// <summary>
    /// A stack of modals shown as a deck of sheets: the newest one in front, the ones it was opened from
    /// peeking out behind it, each a little smaller and quieter than the one in front of it.
    /// <para>
    /// A sheet is pushed under a key (<see cref="Push(string, string, Modal)"/>); pushing a key that is
    /// already in the stack rewinds to it rather than opening a second copy of the same thing. Clicking a
    /// peeking sheet goes back to it, Escape closes the sheet in front, and clicking the backdrop dismisses
    /// the whole chain. Past <see cref="MaxDepth"/> sheets the oldest one is dropped, so a chain of
    /// this-led-to-that never grows without end.
    /// </para>
    /// <para>
    /// The stack takes the modal's own surface and shows it itself, so <see cref="Modal.Show"/> is not what
    /// opens a stacked modal - <see cref="Push(string, string, Modal)"/> is. Everything else about the modal
    /// still works: <see cref="Modal.Hide"/> pops it, and its show and hide handlers run as they would have.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ModalStack")]
    public static class ModalStack
    {
        /// <summary>How many sheets the stack keeps before it starts dropping the oldest one.</summary>
        public const int MaxDepth = 4;

        // How far up each sheet behind the front one is lifted, and how much of its size and of how
        // strongly it reads it gives up per step back - enough to say "there is something behind this".
        private const double PeekOffset  = 34;
        private const double PeekScale   = 0.028;
        private const double PeekFade    = 0.18;

        private static readonly List<ModalStackEntry> _entries = new List<ModalStackEntry>();

        private static HTMLElement _root;
        private static HTMLElement _scrim;
        private static HTMLElement _sheets;
        private static HTMLElement _trail;

        private static bool           _truncated;
        private static Action<Event>  _onKeyDown;

        /// <summary>
        /// Raised whenever the chain changes - a sheet pushed, popped, replaced or dropped - so a host can
        /// keep the route (or anything else naming what is open) in step with it.
        /// </summary>
        public static event Action Changed;

        /// <summary>Gets how many sheets are open.</summary>
        public static int Depth => _entries.Count;

        /// <summary>Gets a value indicating whether nothing is open.</summary>
        public static bool IsEmpty => _entries.Count == 0;

        /// <summary>Gets the sheet in front, or null when nothing is open.</summary>
        public static ModalStackEntry Top => _entries.Count == 0 ? null : _entries[_entries.Count - 1];

        /// <summary>
        /// Gets a value indicating whether the chain has had its oldest sheets dropped to stay within
        /// <see cref="MaxDepth"/> - which is worth saying in a breadcrumb, and nowhere else.
        /// </summary>
        public static bool IsTruncated => _truncated;

        /// <summary>Gets the open sheets, oldest first.</summary>
        public static IReadOnlyList<ModalStackEntry> Entries => _entries;

        /// <summary>
        /// Returns a value indicating whether a sheet is open under the given key.
        /// </summary>
        public static bool Contains(string key) => Find(key) is object;

        /// <summary>
        /// Returns the sheet open under the given key, or null when there is none.
        /// </summary>
        public static ModalStackEntry Get(string key) => Find(key);

        /// <summary>
        /// Returns a value indicating whether the given modal is one of the open sheets - which is what
        /// tells a modal that answers Escape itself to leave the key to the stack instead.
        /// </summary>
        public static bool IsStacked(Modal modal)
        {
            if (modal is null) return false;

            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Modal == modal) return true;
            }

            return false;
        }

        /// <summary>
        /// Opens the given modal as the sheet in front, under the given key and showing the given name in
        /// the chain. Pushing a key that is already open rewinds to it instead - a chain that leads back to
        /// something already in it goes back to that sheet rather than opening a second copy of it.
        /// </summary>
        public static void Push(string key, string name, Modal modal)
        {
            if (modal is null) return;

            if (TryRewindTo(key)) return;

            // Past the depth the deck can show, the oldest sheet goes - which is why a breadcrumb built from
            // the chain has to say it is only part of one.
            while (_entries.Count >= MaxDepth)
            {
                var oldest = _entries[0];

                _entries.RemoveAt(0);
                Detach(oldest);

                _truncated = true;
            }

            var entry = new ModalStackEntry(key, name, modal);

            _entries.Add(entry);

            modal.OnHide(m => Remove(m));

            EnsureRoot();

            _sheets.appendChild(entry.Sheet);

            Arrange();

            modal.RaiseOnShow();

            Changed?.Invoke();
        }

        /// <summary>
        /// Swaps the sheet in front for another one, keeping the chain behind it as it was - what stepping
        /// through a list of results while one of them is open does. Pushes the modal when nothing is open.
        /// </summary>
        public static void Replace(string key, string name, Modal modal)
        {
            if (modal is null) return;

            if (_entries.Count == 0)
            {
                Push(key, name, modal);
                return;
            }

            var replaced = _entries[_entries.Count - 1];

            _entries.RemoveAt(_entries.Count - 1);

            replaced.Modal.RaiseOnHide();
            Detach(replaced);

            var entry = new ModalStackEntry(key, name, modal);

            _entries.Add(entry);

            modal.OnHide(m => Remove(m));

            EnsureRoot();

            _sheets.appendChild(entry.Sheet);

            Arrange();

            modal.RaiseOnShow();

            Changed?.Invoke();
        }

        /// <summary>
        /// Renames the sheet open under the given key, for a sheet whose title is only known once its
        /// content has loaded.
        /// </summary>
        public static void Rename(string key, string name)
        {
            var entry = Find(key);

            if (entry is null) return;

            entry.Name = name;

            Arrange();

            Changed?.Invoke();
        }

        /// <summary>
        /// Goes back to the sheet open under the given key, closing everything opened from it. Returns false
        /// - and changes nothing - when no sheet is open under that key.
        /// </summary>
        public static bool TryRewindTo(string key)
        {
            var entry = Find(key);

            if (entry is null) return false;

            PopTo(key);

            return true;
        }

        /// <summary>
        /// Closes the sheet in front, going back to the one it was opened from.
        /// </summary>
        public static void Pop()
        {
            if (_entries.Count == 0) return;

            _entries[_entries.Count - 1].Modal.Hide();
        }

        /// <summary>
        /// Closes everything opened from the sheet under the given key, leaving that sheet in front.
        /// </summary>
        public static void PopTo(string key)
        {
            var index = IndexOf(key);

            if (index < 0) return;

            while (_entries.Count > index + 1)
            {
                _entries[_entries.Count - 1].Modal.Hide();
            }
        }

        /// <summary>
        /// Closes the sheet showing the given modal, wherever it is in the chain, leaving the rest of the
        /// chain as it was.
        /// </summary>
        public static void Remove(Modal modal)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Modal != modal) continue;

                var entry = _entries[i];

                _entries.RemoveAt(i);
                Detach(entry);

                if (_entries.Count == 0) _truncated = false;

                Arrange();

                Changed?.Invoke();

                return;
            }
        }

        /// <summary>
        /// Closes the whole chain.
        /// </summary>
        public static void Clear()
        {
            while (_entries.Count > 0)
            {
                _entries[_entries.Count - 1].Modal.Hide();
            }
        }

        private static ModalStackEntry Find(string key)
        {
            var index = IndexOf(key);

            return index < 0 ? null : _entries[index];
        }

        private static int IndexOf(string key)
        {
            if (string.IsNullOrEmpty(key)) return -1;

            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Key == key) return i;
            }

            return -1;
        }

        private static void Detach(ModalStackEntry entry)
        {
            if (entry.Tab is object && entry.Tab.parentElement is object) entry.Tab.parentElement.removeChild(entry.Tab);

            entry.Tab = null;

            RestoreSheet(entry.Sheet);

            if (entry.Sheet.parentElement is object) entry.Sheet.parentElement.removeChild(entry.Sheet);
        }

        private static void EnsureRoot()
        {
            if (_root is object && _root.IsMounted()) return;

            _scrim  = Div(Att("tss-modalstack-scrim"));
            _trail  = Div(Att("tss-modalstack-trail"));
            _sheets = Div(Att("tss-modalstack-sheets"));

            _scrim.addEventListener("click", _ => Clear());

            _root = Div(Att("tss-layer tss-fade tss-fade-instant tss-modalstack"), _scrim, _sheets, _trail);

            // How much of a sheet behind clears the one in front is what its header is squeezed into, so
            // the two follow one number rather than drifting apart.
            _root.style.setProperty("--tss-modalstack-peek-strip", $"{PeekOffset}px");

            _root.style.zIndex = Layers.PushLayer(_root);

            document.body.appendChild(_root);
            document.body.style.overflowY = "hidden";

            window.requestAnimationFrame(_ => _root?.classList.add("tss-show"));

            if (_onKeyDown is null)
            {
                _onKeyDown = e =>
                {
                    if (_entries.Count == 0) return;

                    var keyboardEvent = e.As<KeyboardEvent>();

                    if (keyboardEvent.key != "Escape") return;

                    // A menu, a dropdown or a dialog opened over the sheet in front answers Escape first -
                    // dismissing it must not also close the sheet it was opened from.
                    if (IsCovered()) return;

                    StopEvent(keyboardEvent);

                    Pop();
                };
            }

            document.addEventListener("keydown", _onKeyDown);
        }

        // Whether something is showing above the deck: another layer, or a popover of Tippy's, whose own
        // z-index puts it in front of the sheet the user is looking at.
        private static bool IsCovered()
        {
            if (_root is null) return false;

            if (!int.TryParse(_root.style.zIndex, out var mine)) return false;

            foreach (HTMLElement element in document.querySelectorAll(".tss-layer, [data-tippy-root]"))
            {
                if (element == _root) continue;

                if (!int.TryParse(element.style.zIndex, out var zIndex) || zIndex <= mine) continue;

                // A .tss-layer hides itself and lets its content opt back in, so its own visibility says
                // nothing about whether it is open - being in the document at all is what says that. A
                // Tippy popover, on the other hand, stays parked in the document between showings.
                if (!element.classList.contains("tss-layer") && window.getComputedStyle(element).visibility == "hidden") continue;

                return true;
            }

            return false;
        }

        private static void RemoveRoot()
        {
            if (_root is null) return;

            document.removeEventListener("keydown", _onKeyDown);

            if (_root.parentElement is object) _root.parentElement.removeChild(_root);

            document.body.style.overflowY = "";

            _root   = null;
            _scrim  = null;
            _sheets = null;
            _trail  = null;
        }

        private static void Arrange()
        {
            if (_entries.Count == 0)
            {
                RemoveRoot();
                return;
            }

            var count = _entries.Count;

            for (int i = 0; i < count; i++)
            {
                var entry = _entries[i];
                var depth = count - 1 - i;

                if (depth == 0)
                {
                    entry.Sheet.classList.remove("tss-modalstack-peek");
                    entry.Sheet.classList.add("tss-modalstack-sheet", "tss-modalstack-front");
                    entry.Sheet.style.transform = "";
                    entry.Sheet.style.zIndex    = "40";
                    entry.Sheet.style.removeProperty("--tss-modalstack-peek-fade");

                    if (entry.Tab is object)
                    {
                        if (entry.Tab.parentElement is object) entry.Tab.parentElement.removeChild(entry.Tab);

                        entry.Tab = null;
                    }

                    RestoreSheet(entry.Sheet);
                }
                else
                {
                    entry.Sheet.classList.remove("tss-modalstack-front");
                    entry.Sheet.classList.add("tss-modalstack-sheet", "tss-modalstack-peek");
                    entry.Sheet.style.transform = $"translateY(-{PeekOffset * depth}px) scale({1 - (PeekScale * depth)})";
                    entry.Sheet.style.zIndex    = $"{30 - depth}";

                    // The sheet itself stays solid - a deck is cards, not glass - and it is what the sheet
                    // still shows (its title) that fades with how far back it is.
                    entry.Sheet.style.setProperty("--tss-modalstack-peek-fade", $"{1 - (PeekFade * depth)}");

                    EnsureTab(entry);
                    MuteSheet(entry.Sheet, entry.Tab);
                }
            }

            RenderTrail();

            _entries[count - 1].Sheet.focus();
        }

        // A sheet behind the front one is out of reach for everything but going back to it: the way back is
        // a real button covering it, and everything under it stops taking focus, clicks and screen readers.
        private static void EnsureTab(ModalStackEntry entry)
        {
            if (entry.Tab is object)
            {
                entry.Tab.setAttribute("aria-label", BackLabel(entry.Name));
                entry.Tab.setAttribute("title", BackLabel(entry.Name));
                return;
            }

            var key = entry.Key;

            var tab = UI.Button(Att("tss-modalstack-tab", type: "button", ariaLabel: BackLabel(entry.Name), title: BackLabel(entry.Name)));

            tab.addEventListener("click", e =>
            {
                StopEvent(e);

                PopTo(key);
            });

            entry.Tab = tab;

            entry.Sheet.appendChild(tab);
        }

        private static string BackLabel(string name) => string.IsNullOrWhiteSpace(name) ? "Back" : "Back to " + name;

        private static void MuteSheet(HTMLElement sheet, HTMLElement tab)
        {
            for (uint i = 0; i < sheet.children.length; i++)
            {
                var child = (HTMLElement)sheet.children[i];

                if (child == tab) continue;

                child.setAttribute("inert",       "");
                child.setAttribute("aria-hidden", "true");
            }
        }

        private static void RestoreSheet(HTMLElement sheet)
        {
            for (uint i = 0; i < sheet.children.length; i++)
            {
                var child = (HTMLElement)sheet.children[i];

                child.removeAttribute("inert");
                child.removeAttribute("aria-hidden");
            }
        }

        // What led to what, in one line above the deck. Only the label says the chain was truncated - the
        // deck itself shows what it has.
        private static void RenderTrail()
        {
            ClearChildren(_trail);

            if (_entries.Count < 2)
            {
                _trail.style.display = "none";
                return;
            }

            _trail.style.display = "";

            if (_truncated) _trail.appendChild(Span(Att("tss-modalstack-trail-more", text: "…")));

            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];

                if (i > 0 || _truncated) _trail.appendChild(Span(Att("tss-modalstack-trail-separator", text: "›")));

                if (i == _entries.Count - 1)
                {
                    _trail.appendChild(Span(Att("tss-modalstack-trail-current", text: entry.Name ?? string.Empty)));
                    continue;
                }

                var key  = entry.Key;
                var step = UI.Button(Att("tss-modalstack-trail-step", type: "button", text: entry.Name ?? string.Empty));

                step.addEventListener("click", e =>
                {
                    StopEvent(e);

                    PopTo(key);
                });

                _trail.appendChild(step);
            }
        }
    }
}
