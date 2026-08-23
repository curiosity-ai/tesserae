using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A modal listing an application's keyboard shortcuts, grouped into titled sections, each row a
    /// description on the left and the keys on the right as <see cref="KeyboardShortcut"/> chips.
    /// <para>
    /// A shortcut is declared here with the same key names <see cref="KeyboardShortcut.Matches"/> tests, so
    /// a guide can also answer the presses it advertises: give an entry an action with
    /// <see cref="OnPressed"/> and call <see cref="Handle"/> from the application's keydown handler.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ShortcutGuide")]
    public sealed class ShortcutGuide : IComponent, ISpecialCaseStyling
    {
        private readonly Modal       _modal;
        private readonly TextBlock   _title;
        private readonly HTMLElement _sections;
        private readonly List<Entry> _entries = new List<Entry>();

        private HTMLElement _currentSection;

        /// <summary>
        /// Gets the styling container for the guide, so the sizing helpers reach the modal itself.
        /// </summary>
        public HTMLElement StylingContainer => _modal.StylingContainer;

        /// <summary>
        /// Gets whether a sizing helper applied to this component should tag it so a wrapper-building container hoists the style onto the wrapper.
        /// </summary>
        public bool PropagateStylesToWrapper => _modal.PropagateStylesToWrapper;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="title">The modal's title.</param>
        public ShortcutGuide(string title = "Keyboard shortcuts")
        {
            _title    = TextBlock(title).Large().SemiBold();
            _sections = Div(Att("tss-shortcut-guide"));

            _modal = Modal(_title)
               .LightDismiss()
               .Content(Raw(_sections));

            Width(560.px());
        }

        /// <summary>
        /// Starts a new section. Every shortcut added after this call is listed under it, until the next
        /// <see cref="Section"/>.
        /// </summary>
        /// <param name="title">The section's title, e.g. "General". Pass null or empty for an untitled section.</param>
        public ShortcutGuide Section(string title)
        {
            _currentSection = Div(Att("tss-shortcut-guide-section"));

            if (!string.IsNullOrWhiteSpace(title))
            {
                _currentSection.appendChild(Div(Att("tss-shortcut-guide-section-title", text: title)));
            }

            _sections.appendChild(_currentSection);
            return this;
        }

        /// <summary>
        /// Adds a shortcut to the current section, creating an untitled one if <see cref="Section"/> has not
        /// been called yet.
        /// </summary>
        /// <param name="description">What the shortcut does, e.g. "Quick chat or search".</param>
        /// <param name="keys">The keys, in the names <see cref="KeyboardShortcut"/> takes, e.g. "Ctrl", "K".</param>
        public ShortcutGuide Shortcut(string description, params string[] keys)
        {
            if (_currentSection is null) Section(null);

            var row = Div(Att("tss-shortcut-guide-row"), Div(Att("tss-shortcut-guide-description", text: description)));
            row.appendChild(new KeyboardShortcut(keys).Render());
            _currentSection.appendChild(row);

            _entries.Add(new Entry(keys));
            return this;
        }

        /// <summary>
        /// Sets the action the last added shortcut runs when <see cref="Handle"/> sees it pressed. A shortcut
        /// without one is listed but not answered - which is right when whatever owns the key already
        /// handles it.
        /// </summary>
        public ShortcutGuide OnPressed(Action action)
        {
            if (_entries.Count == 0) throw new InvalidOperationException("Call Shortcut(...) before OnPressed(...) - the action belongs to the shortcut added last.");

            _entries[_entries.Count - 1].Action = action;
            return this;
        }

        /// <summary>
        /// Runs the action of the first listed shortcut that <paramref name="e"/> matches, and answers whether
        /// one did - so a caller can stop the event only when the press was taken.
        /// </summary>
        public bool Handle(KeyboardEvent e)
        {
            foreach (var entry in _entries)
            {
                if (entry.Action is null) continue;
                if (!KeyboardShortcut.Matches(e, entry.Keys)) continue;

                entry.Action();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the title of the component.
        /// </summary>
        public ShortcutGuide SetTitle(string title)
        {
            _title.Text = title;
            return this;
        }

        /// <summary>
        /// Sets the width of the component.
        /// </summary>
        public ShortcutGuide Width(UnitSize width)
        {
            _modal.Width(width);
            return this;
        }

        /// <summary>
        /// Sets the width of the component.
        /// </summary>
        public ShortcutGuide W(UnitSize width) => Width(width);

        /// <summary>
        /// Enables light-dismiss behaviour (clicking outside the guide closes it). It is on by default.
        /// </summary>
        public ShortcutGuide LightDismiss()
        {
            _modal.LightDismiss();
            return this;
        }

        /// <summary>
        /// Removes / disables the light dismiss on the component.
        /// </summary>
        public ShortcutGuide NoLightDismiss()
        {
            _modal.NoLightDismiss();
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the guide is shown.
        /// </summary>
        public ShortcutGuide OnShow(Modal.OnShowHandler onShow)
        {
            _modal.OnShow(onShow);
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the guide is hidden.
        /// </summary>
        public ShortcutGuide OnHide(Modal.OnHideHandler onHide)
        {
            _modal.OnHide(onHide);
            return this;
        }

        /// <summary>
        /// Gets a value indicating whether the guide is currently shown.
        /// </summary>
        public bool IsVisible => _modal.IsVisible;

        /// <summary>
        /// Shows the component.
        /// </summary>
        public ShortcutGuide Show()
        {
            _modal.Show();
            return this;
        }

        /// <summary>
        /// Hides the component.
        /// </summary>
        public ShortcutGuide Hide(Action onHidden = null)
        {
            _modal.Hide(onHidden);
            return this;
        }

        /// <summary>
        /// Shows the guide if it is hidden and hides it if it is shown, which is what the shortcut that opens
        /// it usually does.
        /// </summary>
        public ShortcutGuide Toggle()
        {
            if (IsVisible) return Hide();
            return Show();
        }

        /// <summary>
        /// Returns the guide as a component to place in the page, instead of showing it as a modal layer.
        /// </summary>
        public IComponent ShowEmbedded() => _modal.ShowEmbedded();

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _modal.Render();

        private sealed class Entry
        {
            public Entry(string[] keys)
            {
                Keys = keys;
            }

            public readonly string[] Keys;
            public          Action   Action;
        }
    }
}
