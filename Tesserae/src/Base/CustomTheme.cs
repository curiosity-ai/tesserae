using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// The standard implementation of <see cref="ICustomTheme"/>: a root class on the body plus one or
    /// more stylesheets fetched on first activation.
    ///
    /// <para>
    /// A theme package derives from this and passes the URL its combined stylesheet lands at in the
    /// application's output, which is the <c>output</c> folder plus the <c>name</c> of the resource group
    /// in the package's <c>tps.json</c>:
    /// </para>
    /// <code>
    /// public sealed class MyTheme : CustomTheme
    /// {
    ///     public MyTheme() : base("My Theme", "tss-theme-mine", "assets/css/tss-theme-mine.css") { }
    /// }
    /// </code>
    /// </summary>
    public abstract class CustomTheme : ICustomTheme
    {
        private readonly string[] _stylesheets;
        private Task              _loading;

        /// <param name="name">A human readable name.</param>
        /// <param name="rootClassName">The class added to <c>document.body</c> while active.</param>
        /// <param name="stylesheets">The stylesheet URL(s) to load on first activation, relative to the document base.</param>
        protected CustomTheme(string name, string rootClassName, params string[] stylesheets)
        {
            if (string.IsNullOrWhiteSpace(rootClassName)) throw new ArgumentException("A custom theme needs a root class name.", nameof(rootClassName));

            Name          = name ?? rootClassName;
            RootClassName = rootClassName;
            _stylesheets  = stylesheets ?? new string[0];
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string RootClassName { get; }

        /// <summary>The stylesheet URLs this theme loads on first activation.</summary>
        public IReadOnlyList<string> Stylesheets => _stylesheets;

        /// <summary>True once the stylesheets have been loaded.</summary>
        public bool IsLoaded { get; private set; }

        /// <inheritdoc />
        public bool IsActive => document.body.classList.contains(RootClassName);

        /// <inheritdoc />
        public async Task Activate()
        {
            await LoadAsync();
            document.body.classList.add(RootClassName);
            OnActivated();
        }

        /// <inheritdoc />
        public void Deactivate()
        {
            if (!IsActive) return;
            document.body.classList.remove(RootClassName);
            OnDeactivated();
        }

        /// <summary>
        /// Loads the stylesheets without activating the theme, e.g. to warm the cache before a theme
        /// picker is opened. Safe to call more than once; a failed load is retried on the next call.
        /// </summary>
        public async Task LoadAsync()
        {
            if (_loading is null)
            {
                _loading = _stylesheets.Length > 0 ? Require.LoadStyleAsync(_stylesheets) : Task.CompletedTask;
            }

            try
            {
                await _loading;
                IsLoaded = true;
            }
            catch
            {
                _loading = null;
                throw;
            }
        }

        /// <summary>Called after the root class was added. Override to do theme specific set up.</summary>
        protected virtual void OnActivated() { }

        /// <summary>Called after the root class was removed. Override to undo <see cref="OnActivated"/>.</summary>
        protected virtual void OnDeactivated() { }
    }
}
