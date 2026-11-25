using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// How an AI tool may run: on its own, only after the user approves each call, or not at all.
    /// </summary>
    public enum ToolApprovalMode
    {
        AlwaysAllow,
        NeedsApproval,
        Blocked,
    }

    /// <summary>
    /// Picks the <see cref="ToolApprovalMode"/> of one AI tool: a compact three-way <see cref="IconToggle{T}"/>
    /// whose selected item takes the colour of what it means - success for always allow, warning for needs
    /// approval, danger for blocked - so a list of tools reads at a glance.
    /// </summary>
    [Transpose.Name("tss.ToolApprovalToggle")]
    public sealed class ToolApprovalToggle : IComponent, IBindableComponent<ToolApprovalMode>
    {
        private static readonly ToolApprovalMode[] Modes = { ToolApprovalMode.AlwaysAllow, ToolApprovalMode.NeedsApproval, ToolApprovalMode.Blocked };

        private readonly IconToggle<ToolApprovalMode> _toggle;

        /// <summary>
        /// Initializes a new instance of this class, with <paramref name="selected"/> picked. The labels are the
        /// items' tooltips, for an application that translates its UI; null keeps <see cref="DefaultLabel"/>.
        /// </summary>
        public ToolApprovalToggle(
            ToolApprovalMode selected           = ToolApprovalMode.NeedsApproval,
            string           alwaysAllowLabel   = null,
            string           needsApprovalLabel = null,
            string           blockedLabel       = null)
        {
            var labels = new[] { alwaysAllowLabel, needsApprovalLabel, blockedLabel };

            var items = new IconToggle<ToolApprovalMode>.Item[Modes.Length];

            for (var i = 0; i < Modes.Length; i++)
            {
                items[i] = IconToggleItem(IconOf(Modes[i]), string.IsNullOrWhiteSpace(labels[i]) ? DefaultLabel(Modes[i]) : labels[i], Modes[i]);
            }

            _toggle = IconToggle(items).Compact();

            _toggle.Render().classList.add("tss-tool-approval-toggle");
            _toggle.AsObservable().Observe(MarkMode);
            _toggle.Select(selected);
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _toggle.Render();

        /// <summary>
        /// Gets the selected mode.
        /// </summary>
        public ToolApprovalMode Selected => _toggle.Selected;

        /// <summary>
        /// Selects a mode.
        /// </summary>
        public ToolApprovalToggle Select(ToolApprovalMode mode)
        {
            _toggle.Select(mode);

            return this;
        }

        /// <summary>
        /// Returns the selected mode as an observable.
        /// </summary>
        public IObservable<ToolApprovalMode> AsObservable() => _toggle.AsObservable();

        /// <summary>
        /// Selects a mode as part of a two-way binding.
        /// </summary>
        public void SetBoundValue(ToolApprovalMode value) => _toggle.SetBoundValue(value);

        /// <summary>
        /// Calls the handler whenever the user picks another mode, but not for the initial one.
        /// </summary>
        public ToolApprovalToggle OnChange(Action<ToolApprovalToggle, ToolApprovalMode> onChange)
        {
            _toggle.OnChange((_, mode) => onChange(this, mode));

            return this;
        }

        /// <summary>
        /// Configures whether the control is disabled, for a mode the user may see but not change.
        /// </summary>
        public ToolApprovalToggle Disabled(bool value = true)
        {
            _toggle.Disabled(value);

            return this;
        }

        /// <summary>
        /// The icon every surface shows for <paramref name="mode"/>, so a read-only view matches the toggle.
        /// </summary>
        public static UIcons IconOf(ToolApprovalMode mode)
        {
            switch (mode)
            {
                case ToolApprovalMode.AlwaysAllow:   return UIcons.CheckCircle;
                case ToolApprovalMode.NeedsApproval: return UIcons.HandPaper;
                default:                             return UIcons.Ban;
            }
        }

        /// <summary>
        /// The English name of <paramref name="mode"/>, the item's tooltip when the constructor is given no label.
        /// </summary>
        public static string DefaultLabel(ToolApprovalMode mode)
        {
            switch (mode)
            {
                case ToolApprovalMode.AlwaysAllow:   return "Always allow";
                case ToolApprovalMode.NeedsApproval: return "Needs approval";
                default:                             return "Blocked";
            }
        }

        private void MarkMode(ToolApprovalMode mode)
        {
            var root = _toggle.Render();

            root.UpdateClassIf(mode == ToolApprovalMode.AlwaysAllow,   "tss-tool-approval-allow");
            root.UpdateClassIf(mode == ToolApprovalMode.NeedsApproval, "tss-tool-approval-ask");
            root.UpdateClassIf(mode == ToolApprovalMode.Blocked,       "tss-tool-approval-block");
        }
    }
}
