using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A form input that lets the user assemble a list of short string values ("tags" or "chips") by
    /// typing them in and confirming each with a delimiter key (default: <c>Enter</c> or <c>,</c>).
    /// Tags can be removed with the backspace key when the entry field is empty, or by clicking the
    /// small "×" affordance next to each tag.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="TagsInput"/> is the canonical primitive for free-form multi-value fields such as
    /// "categories", "recipients", "labels" or "keywords". When the values must come from a fixed
    /// set, prefer the existing <see cref="Picker{T}"/> component instead.
    /// </para>
    /// <para>
    /// The current list is available synchronously via <see cref="Tags"/>, and via a reactive
    /// <see cref="IObservable{T}"/> through <see cref="AsObservable"/> for binding into other components.
    /// </para>
    /// </remarks>
    [H5.Name("tss.TagsInput")]
    public sealed class TagsInput : IComponent, IHasMarginPadding, IBindableListComponent<string>
    {
        private readonly HTMLDivElement                            _container;
        private readonly HTMLInputElement                          _input;
        private readonly List<string>                              _tags = new List<string>();
        private readonly SettableObservable<IReadOnlyList<string>> _observable;
        private          Action<string>                            _onTagAdded;
        private          Action<string>                            _onTagRemoved;
        private          Action                                    _onChange;
        private          bool                                      _allowDuplicates = false;
        private          int                                       _maxTags         = int.MaxValue;
        private          Func<string, string>                      _normalizer      = s => s?.Trim();
        private          char[]                                    _delimiters      = new[] { ',' };

        /// <summary>Creates a new, empty tags input.</summary>
        public TagsInput() : this(Array.Empty<string>()) { }

        /// <summary>Creates a tags input pre-populated with the given initial tags.</summary>
        public TagsInput(params string[] initialTags)
        {
            _input      = (HTMLInputElement)document.createElement("input");
            _input.type = "text";
            _input.classList.add("tss-tagsinput-entry");

            _container = Div(_("tss-tagsinput tss-default-component-margin"));
            _container.appendChild(_input);

            _observable = new SettableObservable<IReadOnlyList<string>>(_tags.AsReadOnly());

            _input.addEventListener("keydown", OnKeyDown);
            _input.addEventListener("blur",    (_) => CommitCurrent());

            _container.addEventListener("click", (e) =>
            {
                if (e.target == _container) _input.focus();
            });

            if (initialTags != null)
            {
                foreach (var t in initialTags) TryAdd(t, raiseEvents: false);
            }
            RefreshObservable();
        }

        /// <summary>Gets the current list of tags in insertion order.</summary>
        public IReadOnlyList<string> Tags => _tags.AsReadOnly();

        /// <summary>Gets or sets the placeholder shown in the inline entry field.</summary>
        public string Placeholder
        {
            get => _input.placeholder;
            set => _input.placeholder = value;
        }

        /// <summary>Gets or sets whether the same tag value can appear more than once. Defaults to <c>false</c>.</summary>
        public bool AllowDuplicates { get => _allowDuplicates; set => _allowDuplicates = value; }

        /// <summary>Gets or sets the maximum number of tags accepted. Defaults to no limit.</summary>
        public int MaxTags { get => _maxTags; set => _maxTags = value; }

        /// <summary>Gets or sets the outer container's CSS margin.</summary>
        public string Margin { get => _container.style.margin; set => _container.style.margin = value; }

        /// <summary>Gets or sets the outer container's CSS padding.</summary>
        public string Padding { get => _container.style.padding; set => _container.style.padding = value; }

        /// <summary>Sets the placeholder text shown in the inline entry field.</summary>
        public TagsInput SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;
            return this;
        }

        /// <summary>Allows the same tag value to be added more than once.</summary>
        public TagsInput AllowingDuplicates()
        {
            _allowDuplicates = true;
            return this;
        }

        /// <summary>Caps the number of tags that can be added.</summary>
        public TagsInput WithMaxTags(int max)
        {
            _maxTags = max;
            return this;
        }

        /// <summary>
        /// Sets the characters (in addition to <c>Enter</c>) that finalize the current entry into a tag.
        /// Defaults to <c>{','}</c>; pass an empty array to require <c>Enter</c> exclusively.
        /// </summary>
        public TagsInput WithDelimiters(params char[] delimiters)
        {
            _delimiters = delimiters ?? Array.Empty<char>();
            return this;
        }

        /// <summary>
        /// Sets a normalizer applied to every value before it becomes a tag (default: <see cref="string.Trim()"/>).
        /// Return <c>null</c> or empty to reject the value silently.
        /// </summary>
        public TagsInput WithNormalizer(Func<string, string> normalizer)
        {
            _normalizer = normalizer ?? (s => s);
            return this;
        }

        /// <summary>Registers a callback fired whenever a new tag is added.</summary>
        public TagsInput OnTagAdded(Action<string> handler)
        {
            _onTagAdded += handler;
            return this;
        }

        /// <summary>Registers a callback fired whenever a tag is removed.</summary>
        public TagsInput OnTagRemoved(Action<string> handler)
        {
            _onTagRemoved += handler;
            return this;
        }

        /// <summary>Registers a callback fired whenever the tag list changes for any reason.</summary>
        public TagsInput OnChange(Action handler)
        {
            _onChange += handler;
            return this;
        }

        /// <summary>
        /// Returns an observable that emits the latest tag list whenever it changes. Useful for binding
        /// into <c>UI.Defer(...)</c> chains and other reactive composition primitives.
        /// </summary>
        public IObservable<IReadOnlyList<string>> AsObservable() => _observable;

        /// <summary>Programmatically adds a tag (subject to duplicate/max checks and the normalizer).</summary>
        /// <returns><c>true</c> if the tag was added; <c>false</c> if rejected.</returns>
        public bool Add(string tag) => TryAdd(tag, raiseEvents: true);

        /// <summary>
        /// Programmatically replaces the entire tag list as part of a two-way binding.
        /// Existing tags are cleared first. Per-tag normalize / duplicate / max rules still apply.
        /// </summary>
        public void SetBoundValues(IReadOnlyList<string> values)
        {
            // Snapshot the caller's list before we touch _tags. The binding round-trip can hand
            // back the very ReadOnlyCollection we published from RefreshObservable, and that
            // collection is a live view over _tags — so clearing _tags would also empty `values`
            // mid-iteration. The snapshot is also the source of truth for the equality check below.
            var snapshot = values is object ? values.ToArray() : new string[0];

            // Reference-equality on lists doesn't terminate the bind loop the way it does for
            // scalars: each RefreshObservable allocates a new ReadOnlyCollection, so source &
            // component keep echoing forever even when their contents are equal. Compare by
            // sequence and no-op when the tags already match.
            if (_tags.Count == snapshot.Length)
            {
                var equal = true;

                for (var i = 0; i < snapshot.Length; i++)
                {
                    if (_tags[i] != snapshot[i])
                    {
                        equal = false;
                        break;
                    }
                }
                if (equal) return;
            }

            for (var i = _tags.Count - 1; i >= 0; i--) RemoveAt(i, raiseEvents: false);
            foreach (var v in snapshot) TryAdd(v, raiseEvents: false);
            RefreshObservable();
        }

        /// <summary>Removes the first occurrence of the given tag, if present.</summary>
        public bool Remove(string tag)
        {
            var idx = _tags.IndexOf(tag);
            if (idx < 0) return false;
            RemoveAt(idx);
            return true;
        }

        /// <summary>Clears every tag from the input.</summary>
        public TagsInput Clear()
        {
            if (_tags.Count == 0) return this;
            for (var i = _tags.Count - 1; i >= 0; i--) RemoveAt(i, raiseEvents: false);
            _onChange?.Invoke();
            RefreshObservable();
            return this;
        }

        /// <summary>Renders the input's container element.</summary>
        public HTMLElement Render() => _container;

        private bool TryAdd(string raw, bool raiseEvents)
        {
            var normalized = _normalizer?.Invoke(raw) ?? raw;
            if (string.IsNullOrEmpty(normalized)) return false;
            if (!_allowDuplicates && _tags.Contains(normalized)) return false;
            if (_tags.Count >= _maxTags) return false;

            _tags.Add(normalized);
            var chip = BuildChip(normalized);
            _container.insertBefore(chip, _input);

            if (raiseEvents)
            {
                _onTagAdded?.Invoke(normalized);
                _onChange?.Invoke();
                RefreshObservable();
            }
            return true;
        }

        private void RemoveAt(int index, bool raiseEvents = true)
        {
            if (index < 0 || index >= _tags.Count) return;
            var value = _tags[index];
            _tags.RemoveAt(index);

            // The chips appear before the entry input in document order, in the same order as _tags.
            var chip = _container.children[(uint)index] as HTMLElement;

            if (chip is object && chip.classList.contains("tss-tagsinput-chip"))
            {
                _container.removeChild(chip);
            }

            if (raiseEvents)
            {
                _onTagRemoved?.Invoke(value);
                _onChange?.Invoke();
                RefreshObservable();
            }
        }

        private HTMLElement BuildChip(string value)
        {
            var label  = Span(_("tss-tagsinput-chip-label",  text: value));
            var remove = Span(_("tss-tagsinput-chip-remove", text: "×"));
            var chip   = Div(_("tss-tagsinput-chip"), label, remove);

            remove.addEventListener("click", (e) =>
            {
                e.stopPropagation();
                Remove(value);
            });
            return chip;
        }

        private void OnKeyDown(Event evt)
        {
            var e = (KeyboardEvent)evt;

            if (e.key == "Enter")
            {
                if (CommitCurrent()) e.preventDefault();
                return;
            }

            if (e.key == "Tab" && !string.IsNullOrWhiteSpace(_input.value))
            {
                // Tab commits the current text and keeps focus in the entry field. Tab is only allowed
                // to move focus elsewhere when the field is empty or whitespace-only (which would not
                // produce a tag anyway). Both Tab and Shift+Tab follow this rule for symmetry.
                CommitCurrent();
                e.preventDefault();
                return;
            }

            if (e.key == "Backspace" && string.IsNullOrEmpty(_input.value) && _tags.Count > 0)
            {
                RemoveAt(_tags.Count - 1);
                return;
            }

            if (_delimiters != null && _delimiters.Length > 0 && e.key.Length == 1 && _delimiters.Contains(e.key[0]))
            {
                if (CommitCurrent()) e.preventDefault();
            }
        }

        private bool CommitCurrent()
        {
            var value = _input.value;
            if (string.IsNullOrEmpty(value)) return false;
            var added = TryAdd(value, raiseEvents: true);
            _input.value = string.Empty;
            return added;
        }

        private void RefreshObservable()
        {
            _observable.Value = _tags.AsReadOnly();
        }
    }
}