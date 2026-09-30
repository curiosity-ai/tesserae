using System.Threading.Tasks;

namespace Tesserae
{
    /// <summary>
    /// A theme shipped as its own package, which restyles every Tesserae component while it is active.
    ///
    /// <para>
    /// A custom theme works by one mechanism and no other: it owns a root class name
    /// (<see cref="RootClassName"/>), which <see cref="Activate"/> adds to <c>document.body</c> and
    /// <see cref="Deactivate"/> removes again. Every rule in the theme's stylesheet is scoped under that
    /// class, so the theme's CSS can stay loaded after it is deactivated and simply stops matching.
    /// Because the class is on the body, it also reaches layers (modals, panels, tooltips, menus), which
    /// are mounted on the body rather than inside the component that opened them.
    /// </para>
    ///
    /// <para>
    /// The stylesheet itself is loaded the first time the theme is activated, not when the page loads:
    /// a theme package declares its combined stylesheet in its <c>tps.json</c> with <c>"load": false</c>,
    /// so the file is copied into the application's output but not linked from <c>index.html</c>.
    /// <see cref="CustomTheme"/> implements all of this; derive from it rather than implementing the
    /// interface by hand.
    /// </para>
    ///
    /// <para>
    /// Switch themes through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>, which keeps at most
    /// one custom theme active and raises <see cref="UI.Theme.OnThemeChanged"/>.
    /// </para>
    /// </summary>
    public interface ICustomTheme
    {
        /// <summary>A human readable name, e.g. for a theme picker.</summary>
        string Name { get; }

        /// <summary>
        /// The class added to <c>document.body</c> while the theme is active. Every rule in the theme's
        /// stylesheet must be scoped under it. Use a name no other theme will use, e.g.
        /// <c>tss-theme-&lt;name&gt;</c>.
        /// </summary>
        string RootClassName { get; }

        /// <summary>True while <see cref="RootClassName"/> is on the body.</summary>
        bool IsActive { get; }

        /// <summary>
        /// Activates the theme: loads its stylesheet(s) the first time it is called, then adds
        /// <see cref="RootClassName"/> to the body. Completes once the stylesheet is applied, so the page
        /// never shows the class without the rules it switches on.
        /// </summary>
        Task Activate();

        /// <summary>Removes <see cref="RootClassName"/> from the body. The stylesheet stays loaded.</summary>
        void Deactivate();
    }
}
