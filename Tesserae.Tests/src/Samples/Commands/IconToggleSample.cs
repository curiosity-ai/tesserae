using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Commands, Order = 30, Icon = UIcons.LayoutFluid)]
    public class IconToggleSample : IComponent, ISample
    {
        public enum ViewMode { List, Grid, Table }

        public enum ComposerMode { Chat, Search }

        public enum Align { Left, Center, Justify }

        public enum Device { Desktop, Tablet, Phone }

        public enum AnswerStyle { Fast, Balanced, Thorough }

        private readonly IComponent _content;

        public IconToggleSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(IconToggleSample), UIcons.LayoutFluid, "A segmented control of icon buttons where exactly one is selected")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("An IconToggle is a segmented control: a small track holding two or more icon buttons, of which exactly one is selected at a time. It's the compact alternative to a ChoiceGroup for switching a mode or a view."),
                        TextBlock("Each item carries an icon, a tooltip and a data payload of type T, and the current payload is exposed as an observable, so the selection can drive content or be two-way bound."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Reach for an IconToggle when the options are few (two to four), mutually exclusive, and each has an icon that reads on its own — view switchers, alignment, a chat/search mode selector. Always give every item a tooltip: an icon alone is rarely self-explanatory, and the tooltip is the only label an icon-only item has."),
                        VStack().Children(
                            SampleDo("Keep it to a handful of items that all fit on one row"),
                            SampleDo("Add a text label when the icon alone is ambiguous"),
                            SampleDo("Select a sensible default — the first item is selected on render"),
                            SampleDont("Use it for actions; those are Buttons, not a selection"),
                            SampleDont("Use it for options that aren't mutually exclusive — use CheckBoxes")))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Basic"),
                        TextBlock("Icon-only items, the first one selected on render.").Secondary(),
                        BasicExample(),

                        SampleSubTitle("With labels"),
                        TextBlock("Passing a text as the last argument of IconToggleItem puts a label next to the icon.").Secondary(),
                        LabelledExample(),

                        SampleSubTitle("Sizes"),
                        TextBlock("The default fits inline next to other controls; Compact() is for dense toolbars and Large() for a page-level switch.").Secondary(),
                        SizesExample(),

                        SampleSubTitle("Rounded"),
                        TextBlock("Rounded(BorderRadius) reshapes the track, and the items follow along.").Secondary(),
                        RoundedExample(),

                        SampleSubTitle("Vertical"),
                        TextBlock("Vertical() stacks the items into a rail, every item as wide as the widest.").Secondary(),
                        VerticalExample(),

                        SampleSubTitle("Full width"),
                        TextBlock("FullWidth() stretches the track to its container, with every item taking an equal share.").Secondary(),
                        FullWidthExample(),

                        SampleSubTitle("Disabled"),
                        TextBlock("An item can be disabled on its own, or the whole control at once.").Secondary(),
                        DisabledExample(),

                        SampleSubTitle("Driving content"),
                        TextBlock("The selection is an observable, so a DeferSync can rebuild a panel whenever it changes.").Secondary(),
                        DrivingContentExample(),

                        SampleSubTitle("Event handling"),
                        TextBlock("OnChange fires on every change but not for the initial selection.").Secondary(),
                        EventExample(),

                        SampleSubTitle("Two-way binding"),
                        TextBlock("IconToggle is bindable, so it stays in sync with an observable changed from anywhere else.").Secondary(),
                        BindingExample(),

                        SampleSubTitle("In a toolbar"),
                        TextBlock("A Compact() toggle lines up with the other buttons of a toolbar row.").Secondary(),
                        ToolbarExample()
                    )).SetTitle("Usage")))
               .FlatSection(Stack().Children(AIExample()))
               .SeeAlso(typeof(ToggleSample), typeof(ChoiceGroupSample), typeof(SegmentedPivotSample), typeof(ButtonSample), typeof(OmniBoxSample), typeof(BindingSample), typeof(AIVariantsSample));
        }

        private static IComponent BasicExample()
        {
            var toggle = IconToggle(
                IconToggleItem(UIcons.List,  "List view",  ViewMode.List),
                IconToggleItem(UIcons.Apps,  "Grid view",  ViewMode.Grid),
                IconToggleItem(UIcons.Table, "Table view", ViewMode.Table));

            return HStack().AlignItemsCenter().Children(
                toggle,
                DeferSync(toggle.AsObservable(), mode => TextBlock($"{mode} view").Secondary().ML(12)));
        }

        private static IComponent LabelledExample()
        {
            return HStack().AlignItemsCenter().Children(
                IconToggle(
                    IconToggleItem(UIcons.Comment, "Ask the assistant", ComposerMode.Chat,   "Chat"),
                    IconToggleItem(UIcons.Search,  "Search everything", ComposerMode.Search, "Search")),
                IconToggle(
                    IconToggleItem(UIcons.AlignLeft,    "Align left",    Align.Left,    "Left"),
                    IconToggleItem(UIcons.AlignCenter,  "Center",        Align.Center,  "Center"),
                    IconToggleItem(UIcons.AlignJustify, "Justify",       Align.Justify, "Justify")).ML(16));
        }

        private static IComponent SizesExample()
        {
            return VStack().Children(
                LabelledRow("Compact()", IconToggle(
                    IconToggleItem(UIcons.List, "List view", ViewMode.List),
                    IconToggleItem(UIcons.Apps, "Grid view", ViewMode.Grid)).Compact()),
                LabelledRow("Default", IconToggle(
                    IconToggleItem(UIcons.List, "List view", ViewMode.List),
                    IconToggleItem(UIcons.Apps, "Grid view", ViewMode.Grid))),
                LabelledRow("Large()", IconToggle(
                    IconToggleItem(UIcons.List, "List view", ViewMode.List),
                    IconToggleItem(UIcons.Apps, "Grid view", ViewMode.Grid)).Large()));
        }

        private static IComponent RoundedExample()
        {
            return VStack().Children(
                LabelledRow("BorderRadius.Small",  ModeToggle().Rounded(BorderRadius.Small)),
                LabelledRow("BorderRadius.Medium", ModeToggle().Rounded(BorderRadius.Medium)),
                LabelledRow("BorderRadius.Full",   ModeToggle().Rounded(BorderRadius.Full)));
        }

        private static IComponent VerticalExample()
        {
            return HStack().Children(
                IconToggle(
                    IconToggleItem(UIcons.List,  "List view",  ViewMode.List),
                    IconToggleItem(UIcons.Apps,  "Grid view",  ViewMode.Grid),
                    IconToggleItem(UIcons.Table, "Table view", ViewMode.Table)).Vertical(),
                IconToggle(
                    IconToggleItem(UIcons.DesktopWallpaper, "Desktop", Device.Desktop, "Desktop"),
                    IconToggleItem(UIcons.TabletAndroid,    "Tablet",  Device.Tablet,  "Tablet"),
                    IconToggleItem(UIcons.MobileNotch,      "Phone",   Device.Phone,   "Phone")).Vertical().ML(24));
        }

        private static IComponent FullWidthExample()
        {
            return VStack().WS().MaxWidth(420.px()).Children(
                IconToggle(
                    IconToggleItem(UIcons.Comment, "Ask the assistant", ComposerMode.Chat,   "Chat"),
                    IconToggleItem(UIcons.Search,  "Search everything", ComposerMode.Search, "Search")).FullWidth());
        }

        private static IComponent DisabledExample()
        {
            var partly = IconToggle(
                IconToggleItem(UIcons.List,   "List view",                    ViewMode.List),
                IconToggleItem(UIcons.Apps,   "Grid view",                    ViewMode.Grid),
                IconToggleItem(UIcons.Marker, "Map view (needs a location)",  ViewMode.Table).Disabled());

            var whole = ModeToggle().Disabled();

            return VStack().Children(
                LabelledRow("One item disabled", partly),
                LabelledRow("Whole control disabled", whole));
        }

        private static IComponent DrivingContentExample()
        {
            var toggle = IconToggle(
                IconToggleItem(UIcons.List,  "List view",  ViewMode.List),
                IconToggleItem(UIcons.Apps,  "Grid view",  ViewMode.Grid),
                IconToggleItem(UIcons.Table, "Table view", ViewMode.Table));

            return VStack().WS().Children(
                toggle,
                DeferSync(toggle.AsObservable(), mode => Preview(mode)).MT(8));
        }

        private static IComponent Preview(ViewMode mode)
        {
            var items = new[] { "Quarterly report", "Design review notes", "Roadmap draft" };

            if (mode == ViewMode.List)
            {
                var list = VStack().WS();

                foreach (var item in items)
                {
                    list.Add(HStack().AlignItemsCenter().Children(Icon(UIcons.Document).PR(8), TextBlock(item)).PT(4).PB(4));
                }

                return Card(list).WS();
            }

            if (mode == ViewMode.Grid)
            {
                var grid = HStack().WS();

                foreach (var item in items)
                {
                    grid.Add(Card(VStack().Children(Icon(UIcons.Document), TextBlock(item).PT(8))).W(140).MR(8));
                }

                return grid;
            }

            var table = VStack().WS();
            table.Add(HStack().WS().Children(TextBlock("Name").SemiBold().W(220), TextBlock("Kind").SemiBold()).PT(4).PB(4));

            foreach (var item in items)
            {
                table.Add(HStack().WS().Children(TextBlock(item).W(220), TextBlock("Document").Secondary()).PT(4).PB(4));
            }

            return Card(table).WS();
        }

        private static IComponent EventExample()
        {
            return IconToggle(
                    IconToggleItem(UIcons.Sun,  "Light appearance", "light"),
                    IconToggleItem(UIcons.Moon, "Dark appearance",  "dark"))
               .OnChange((sender, value) => Toast().Information($"Appearance is now {value}"));
        }

        private static IComponent BindingExample()
        {
            var view = new SettableObservable<ViewMode>(ViewMode.Grid);

            return HStack().AlignItemsCenter().Children(
                IconToggle(
                    IconToggleItem(UIcons.List,  "List view",  ViewMode.List),
                    IconToggleItem(UIcons.Apps,  "Grid view",  ViewMode.Grid),
                    IconToggleItem(UIcons.Table, "Table view", ViewMode.Table)).Bind(view),
                Button("List").Small().OnClick(() => view.Value  = ViewMode.List).ML(16),
                Button("Grid").Small().OnClick(() => view.Value  = ViewMode.Grid),
                Button("Table").Small().OnClick(() => view.Value = ViewMode.Table),
                DeferSync(view, v => TextBlock($"view = {v}").SemiBold().ML(8)));
        }

        private static IComponent AIExample()
        {
            return Card(VStack().WS().Children(
                TextBlock("AI() marks the control as an AI one - picking how a model should work, or which of its answers to look at. The track takes the purple-to-blue tint, the unselected items the accent, and the selected pill is filled with the gradient: the selected segment is the one filled action on the control, which is what an AI Button says too."),
                SampleSubTitle("Which model, and how it answers"),
                LabelledRow("Answer style", IconToggle(
                    IconToggleItem(UIcons.Bolt,     "Fast - a short answer",       AnswerStyle.Fast),
                    IconToggleItem(UIcons.Scale,    "Balanced",                    AnswerStyle.Balanced),
                    IconToggleItem(UIcons.Brain,    "Thorough - reads every source", AnswerStyle.Thorough)).AI()),
                LabelledRow("With labels", IconToggle(
                    IconToggleItem(UIcons.Comment, "Ask the assistant", ComposerMode.Chat).SetText("Ask"),
                    IconToggleItem(UIcons.Search,  "Search everything", ComposerMode.Search).SetText("Search")).AI()),
                LabelledRow("Compact, in a toolbar", IconToggle(
                    IconToggleItem(UIcons.Bolt,  "Fast",     AnswerStyle.Fast),
                    IconToggleItem(UIcons.Scale, "Balanced", AnswerStyle.Balanced),
                    IconToggleItem(UIcons.Brain, "Thorough", AnswerStyle.Thorough)).AI().Compact()),
                LabelledRow("Large", IconToggle(
                    IconToggleItem(UIcons.Bolt,  "Fast",     AnswerStyle.Fast),
                    IconToggleItem(UIcons.Scale, "Balanced", AnswerStyle.Balanced),
                    IconToggleItem(UIcons.Brain, "Thorough", AnswerStyle.Thorough)).AI().Large()),
                LabelledRow("Full width", IconToggle(
                    IconToggleItem(UIcons.Bolt,  "Fast",     AnswerStyle.Fast).SetText("Fast"),
                    IconToggleItem(UIcons.Scale, "Balanced", AnswerStyle.Balanced).SetText("Balanced"),
                    IconToggleItem(UIcons.Brain, "Thorough", AnswerStyle.Thorough).SetText("Thorough")).AI().FullWidth())
            )).SetTitle("The AI variant", UIcons.Sparkles, Theme.Colors.Purple600).WS();
        }

        private static IComponent ToolbarExample()
        {
            var toggle = IconToggle(
                IconToggleItem(UIcons.List,  "List view",  ViewMode.List),
                IconToggleItem(UIcons.Apps,  "Grid view",  ViewMode.Grid),
                IconToggleItem(UIcons.Table, "Table view", ViewMode.Table)).Compact();

            return Card(HStack().WS().AlignItemsCenter().Children(
                TextBlock("Documents").SemiBold(),
                HStack().Grow(),
                SearchBox("Filter").W(180).MR(8),
                toggle,
                Button().SetIcon(UIcons.Refresh).Tooltip("Refresh").NoBorder().NoBackground().ML(8))).WS();
        }

        private static IconToggle<ComposerMode> ModeToggle() => IconToggle(
            IconToggleItem(UIcons.Comment, "Ask the assistant", ComposerMode.Chat),
            IconToggleItem(UIcons.Search,  "Search everything", ComposerMode.Search));

        private static IComponent LabelledRow(string label, IComponent toggle) =>
            HStack().AlignItemsCenter().Children(TextBlock(label).Secondary().W(180), toggle).PB(8);

        public HTMLElement Render() => _content.Render();
    }
}
