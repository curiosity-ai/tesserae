using System;
using System.Linq;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Inputs, Order = 80, Icon = UIcons.CaretSquareDown, Description = "Single and multi-select dropdowns")]
    public sealed class DropdownSample : IComponent, ISample
    {
        private readonly IComponent _content;
        public DropdownSample()
        {
            var validatedDropdown = Dropdown().Items(
                            DropdownItem("Option 1"),
                            DropdownItem("Option 2")
                        );
            validatedDropdown.Attach(dd => dd.IsInvalid = dd.SelectedItems.Length != 1 || dd.SelectedItems[0].Text != "Option 1");

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(DropdownSample), UIcons.CaretDown, "A control to select an option from a dropdown")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("A Dropdown is a list in which the selected item is always visible, and the others are visible on demand by clicking a drop-down button."),
                    TextBlock("They are used to simplify the design and make a choice within the UI. When closed, only the selected item is visible. When users click the drop-down button, all the options become visible."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use a Dropdown when there are multiple choices that can be collapsed under one title, especially if the list of items is long or when space is constrained. Use shortened statements or single words as options. Dropdowns are preferred over radio buttons when the selected option is more important than the alternatives. For less than 7 options, consider using a ChoiceGroup if space allows."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic Dropdown"),
                    VStack().Children(
                        Label("Standard").SetContent(Dropdown().Items(
                            DropdownItem("Option 1").Selected(),
                            DropdownItem("Option 2"),
                            DropdownItem("Option 3")
                        )),
                        Label("With Headers and Dividers").SetContent(Dropdown().Items(
                            DropdownItem("Group 1").Header(),
                            DropdownItem("Item 1.1"),
                            DropdownItem("Item 1.2"),
                            DropdownItem().Divider(),
                            DropdownItem("Group 2").Header(),
                            DropdownItem("Item 2.1"),
                            DropdownItem("Item 2.2").Selected()
                        ))
                    ),
                    SampleSubTitle("Selection Modes"),
                    VStack().Children(
                        Label("Searchable").SetContent(Dropdown().Searchable().Items(
                            DropdownItem("Apple"),
                            DropdownItem("Banana").Selected(),
                            DropdownItem("Orange").Selected(),
                            DropdownItem("Grape")
                        )),
                        Label("Multi-select").SetContent(Dropdown().Multi().Items(
                            DropdownItem("Apple"),
                            DropdownItem("Banana").Selected(),
                            DropdownItem("Orange").Selected(),
                            DropdownItem("Grape")
                        )),
                        Label("Custom Arrow Icon").SetContent(Dropdown().SetArrowIcon(UIcons.AnglesUpDown).Items(
                            DropdownItem("Low"),
                            DropdownItem("Medium").Selected(),
                            DropdownItem("High")
                        ))
                    ),
                    SampleSubTitle("Mixed Renderings"),
                    VStack().Children(
                        TextBlock("An item's content is any IComponent, and the box shows a clone of it - so whatever an option renders in the list, the box has to render inline, comma-separated, clipped to one 32px row. These are deliberately unreasonable options: they are here to keep the box honest about avatars, badges, charts, emoji, two-line blocks and text that is far too long."),
                        Label("People (avatar + two lines, multi)").SetContent(PeopleDropdown()),
                        Label("Status badges (multi)").SetContent(Dropdown().Multi().Items(
                            BadgeItem("Critical", BadgeTone.Danger,  UIcons.Exclamation).Selected(),
                            BadgeItem("Degraded", BadgeTone.Warning, UIcons.TriangleWarning).Selected(),
                            BadgeItem("Healthy",  BadgeTone.Success, UIcons.CheckCircle),
                            BadgeItem("Unknown",  BadgeTone.Neutral, UIcons.Interrogation)
                        )),
                        Label("Colour swatches (multi)").SetContent(Dropdown().Multi().Items(
                            SwatchItem("Crimson", "#d13438").Selected(),
                            SwatchItem("Marigold", "#eaa300").Selected(),
                            SwatchItem("Seafoam", "#00b7c3").Selected(),
                            SwatchItem("Orchid", "#8764b8")
                        )),
                        Label("Charts inside options (multi)").SetContent(Dropdown().Multi().Items(
                            MetricItem("Revenue",  new double[] { 3, 5, 4, 8, 6, 11, 9, 14 }, "#107c10").Selected(),
                            MetricItem("Sessions", new double[] { 14, 9, 11, 6, 8, 4, 5, 3 }, "#d13438").Selected(),
                            MetricItem("Latency",  new double[] { 6, 6, 7, 6, 8, 7, 6, 7 }, "#8764b8")
                        )),
                        TextBlock("Content taller than the row is the one case the box cannot rescue: a comma next to a two-line block has nowhere good to sit. Give tall content a short form through the second DropdownItem argument, the way the People dropdown above does - this one deliberately does not, to show the difference."),
                        Label("Two-line blocks, no short form (multi)").SetContent(Dropdown().Multi().Items(
                            TwoLineItem("Frankfurt", "eu-central-1").Selected(),
                            TwoLineItem("Oregon", "us-west-2").Selected(),
                            TwoLineItem("Singapore", "ap-southeast-1")
                        )),
                        Label("Emoji and interactive content (multi)").SetContent(Dropdown().Multi().Items(
                            DropdownItem(HStack().AlignItemsCenter().Gap(8.px()).Children(Icon(Emoji.Pizza), TextBlock("Pizza")), HStack().AlignItemsCenter().Gap(4.px()).Children(Icon(Emoji.Pizza, TextSize.Small), TextBlock("Pizza").Small())).Selected(),
                            DropdownItem(HStack().AlignItemsCenter().Gap(8.px()).Children(Icon(Emoji.Cheese), TextBlock("Cheese")), HStack().AlignItemsCenter().Gap(4.px()).Children(Icon(Emoji.Cheese, TextSize.Small), TextBlock("Cheese").Small())).Selected(),
                            DropdownItem(HStack().AlignItemsCenter().Gap(8.px()).Children(TextBlock("Rated"), Rating(5).SetValue(4).ReadOnly()), TextBlock("Rated 4/5").Small()),
                            DropdownItem(HStack().AlignItemsCenter().Gap(8.px()).Children(TextBlock("Shortcut"), KeyboardShortcut("Ctrl", "K")), TextBlock("Ctrl+K").Small())
                        )),
                        Label("Text that is far too long (multi)").SetContent(Dropdown().Multi().Items(
                            DropdownItem("A perfectly reasonable option").Selected(),
                            DropdownItem("An option whose label someone pasted an entire sentence into, which the box has one clipped row to deal with").Selected(),
                            DropdownItem("Another one, just as long, so that the clipping has to happen part way through a comma-separated list").Selected()
                        )),
                        Label("Everything at once (multi)").SetContent(Dropdown().Multi().Items(
                            BadgeItem("Critical", BadgeTone.Danger, UIcons.Exclamation).Selected(),
                            SwatchItem("Seafoam", "#00b7c3").Selected(),
                            PersonRow("Ana Pereira", "ana@example.com").Selected(),
                            MetricItem("Revenue", new double[] { 3, 5, 4, 8, 6, 11, 9, 14 }, "#107c10").Selected(),
                            DropdownItem(HStack().AlignItemsCenter().Gap(8.px()).Children(Icon(Emoji.Pizza), TextBlock("Pizza")), HStack().AlignItemsCenter().Gap(4.px()).Children(Icon(Emoji.Pizza, TextSize.Small), TextBlock("Pizza").Small())).Selected()
                        )),
                        TextBlock("WithCustomSelectionRender takes over the box completely: the selected items are handed to you and whatever you return is what the box shows - no clones, and no commas to get right."),
                        Label("Custom selection render (count)").SetContent(
                            Dropdown().Multi()
                               .WithCustomSelectionRender(items => Badge($"{items.Length} region{(items.Length == 1 ? "" : "s")} selected").Primary().Pill())
                               .Items(
                                    TwoLineItem("Frankfurt", "eu-central-1").Selected(),
                                    TwoLineItem("Oregon", "us-west-2").Selected(),
                                    TwoLineItem("Singapore", "ap-southeast-1")
                                )),
                        Label("Custom selection render (avatar pile)").SetContent(
                            Dropdown().Multi()
                               .WithCustomSelectionRender(items => HStack().AlignItemsCenter().Gap(4.px()).Children(
                                    items.Take(4).Select(i => (IComponent)Avatar(initials: i.GetDataAs<string>()).Size(AvatarSize.XSmall)).ToArray()))
                               .Items(
                                    PersonRow("Ana Pereira", "ana@example.com").Selected(),
                                    PersonRow("Bo Lindqvist", "bo@example.com").Selected(),
                                    PersonRow("Chen Wei", "chen@example.com").Selected(),
                                    PersonRow("Dara O'Neill", "dara@example.com")
                                ))
                    ),
                    SampleSubTitle("Async Loading"),
                    VStack().Children(
                        Label("Load on open (5s delay)").SetContent(Dropdown().Items(GetItemsAsync)),
                        Label("Load immediately (5s delay)").SetContent(StartLoadingAsyncDataImmediately(Dropdown().Items(GetItemsAsync))),
                        Label("Deferred item content").SetContent(Dropdown().Items(
                            DeferredDropdownItem("Item 1", 500),
                            DeferredDropdownItem("Item 2", 800),
                            DeferredDropdownItem("Item 3", 1100)
                        )),
                        Label("Empty State").SetContent(Dropdown("No items available").Items(new Dropdown.Item[0]))
                    ),
                    SampleSubTitle("Deferred Mixed Content"),
                    VStack().Children(
                        TextBlock("The same rich options as above, only now each one's content arrives through a Defer. An option is drawn twice - as a row in the list and, when selected, in the closed box - and because each is built from the factory the item was given, each is a live component: it mounts, so its Defer loads, wherever it is. Two of these are selected up front and fill themselves in without the list ever being opened."),
                        Label("Deferred rich options (multi, two pre-selected)").SetContent(Dropdown().Multi().Items(
                            DeferredItem(300,  () => BadgeRow("Critical", BadgeTone.Danger, UIcons.Exclamation),  selected: true),
                            DeferredItem(700,  () => SwatchRow("Seafoam", "#00b7c3", 16),                         selected: true),
                            DeferredItem(1100, () => PersonBlock("Ana Pereira", "ana@example.com")),
                            DeferredItem(1500, () => MetricRow("Revenue", new double[] { 3, 5, 4, 8, 6, 11, 9, 14 }, "#107c10", 90, 24, labelWidth: 70))
                        )),
                        Label("Deferred rich options (single)").SetContent(Dropdown().Items(
                            DeferredItem(400,  () => PersonBlock("Bo Lindqvist", "bo@example.com")),
                            DeferredItem(900,  () => BadgeRow("Healthy", BadgeTone.Success, UIcons.CheckCircle)),
                            DeferredItem(1300, () => SwatchRow("Orchid", "#8764b8", 16))
                        )),
                        TextBlock("It does not matter how deep the Defer is either: the box builds the whole subtree, so a Defer buried inside the content mounts and loads there just as a top-level one does. The option below wraps a deferred fragment in a stack, and is selected up front."),
                        Label("Deferred nested inside the content (multi, pre-selected)").SetContent(Dropdown().Multi().Items(
                            NestedDeferredItem(600,  "Frankfurt",  "eu-central-1",  selected: true),
                            NestedDeferredItem(1000, "Oregon",     "us-west-2")
                        )),
                        TextBlock("The price of two live components is that the factory runs twice, so whatever it does happens twice - two Defers, and two of whatever they call. Where that work is expensive, do it once outside the factory and let both instances await the same Task. The option below fetches once and is rendered twice from the result; the label shows how many times the 'fetch' actually ran."),
                        Label("Work shared between the two instances (multi, pre-selected)").SetContent(Dropdown().Multi().Items(
                            SharedWorkItem("Shared fetch", selected: true)
                        )),
                        TextBlock("A short form is a separate factory, for when the box wants a shorter version of the option rather than the same one. Here both halves are deferred, on different delays."),
                        Label("Deferred row and deferred short form (multi)").SetContent(Dropdown().Multi().Items(
                            DeferredPairItem(500,  1200, "Frankfurt",  "eu-central-1",   selected: true),
                            DeferredPairItem(800,  1600, "Oregon",     "us-west-2",      selected: true),
                            DeferredPairItem(1100, 2000, "Singapore",  "ap-southeast-1")
                        ))
                    ),
                    SampleSubTitle("Lazy Search (SearchAsync)"),
                    VStack().Children(
                        TextBlock("With thousands of options there is no loading them all up front. Seed the dropdown with the first page, and let SearchAsync fetch the rest as the User types - the items it returns are added to the ones already there, so the seed and the current selection survive every lookup. This example holds 5,000 people and seeds only the first 20."),
                        Label("Search 5,000 people").SetContent(
                            Dropdown()
                               .Items(FirstPageOfPeople())
                               .SearchAsync(SearchPeopleAsync, placeholder: "Type a name..."))
                    ),
                    SampleSubTitle("Validation"),
                    VStack().Children(
                        Label("Required Dropdown").SetContent(Dropdown().Required().Items(
                            DropdownItem("Choose one...").Header(),
                            DropdownItem("Valid Choice")
                        )),
                        Label("Validation (Must select 'Option 1')").SetContent(validatedDropdown)
                    ),
                    SampleSubTitle("Rounded Dropdowns"),
                    VStack().Children(
                        Label("Small").SetContent(Dropdown().Rounded(BorderRadius.Small).Items(DropdownItem("Option 1"), DropdownItem("Option 2"))),
                        Label("Medium").SetContent(Dropdown().Rounded(BorderRadius.Medium).Items(DropdownItem("Option 1"), DropdownItem("Option 2"))),
                        Label("Full").SetContent(Dropdown().Rounded(BorderRadius.Full).Items(DropdownItem("Option 1"), DropdownItem("Option 2")))
                    )
                )).SetTitle("Usage")))
               .SeeAlso(typeof(PickerSample), typeof(ChoiceGroupSample), typeof(TagsInputSample), typeof(SearchableListSample), typeof(MenuSample));
        }

        // --- Mixed renderings ------------------------------------------------------------------
        // The content each option draws, defined once as an IComponent, so the plain options above and
        // the deferred ones below are demonstrably showing the same thing through different paths.

        private static IComponent PersonBlock(string name, string email) =>
            HStack().AlignItemsCenter().Gap(8.px()).Children(
                Avatar(initials: Initials(name)).Size(AvatarSize.Small).Presence(AvatarPresence.Online),
                VStack().Children(
                    TextBlock(name).Small(),
                    TextBlock(email).Tiny().Secondary()));

        private static IComponent PersonChip(string name) =>
            HStack().AlignItemsCenter().Gap(4.px()).Children(
                Avatar(initials: Initials(name)).Size(AvatarSize.XSmall),
                TextBlock(name.Split(' ')[0]).Small());

        private static IComponent BadgeRow(string text, BadgeTone tone, UIcons icon) =>
            Badge(text).Tone(tone).Pill().SetIcon(Tesserae.Icon.Transform(icon, UIconsWeight.Regular));

        // A colour chip drawn with a bare div, to prove the box does not care what the content is.
        private static IComponent SwatchRow(string name, string color, int size) =>
            HStack().AlignItemsCenter().Gap(8.px()).Children(
                Raw(Div(Att(styles: s =>
                {
                    s.width        = $"{size}px";
                    s.height       = $"{size}px";
                    s.borderRadius = "4px";
                    s.background   = color;
                    s.flexShrink   = "0";
                }))),
                TextBlock(name).Small());

        private static IComponent MetricRow(string name, double[] series, string color, double width, double height, int labelWidth = 0) =>
            HStack().AlignItemsCenter().Gap(8.px()).Children(
                labelWidth > 0 ? TextBlock(name).Small().W(labelWidth) : TextBlock(name).Small(),
                Sparkline(series, width: width, height: height, color: color));

        private static IComponent TwoLineBlock(string title, string subtitle) =>
            VStack().Children(
                TextBlock(title).Small(),
                TextBlock(subtitle).Tiny().Secondary());

        private static string Initials(string name) => string.Join("", name.Split(' ').Select(part => part.Substring(0, 1)));

        // --- Options built from that content --------------------------------------------------
        // The second argument to DropdownItem is what the *box* shows once the option is selected,
        // which is the escape hatch for content too tall or too wide to sit on one clipped row.

        private static Dropdown PeopleDropdown() => Dropdown().Multi().Items(
            PersonRow("Ana Pereira",  "ana@example.com").Selected(),
            PersonRow("Bo Lindqvist", "bo@example.com").Selected(),
            PersonRow("Chen Wei",     "chen@example.com"),
            PersonRow("Dara O'Neill", "dara@example.com"));

        // A two-line row with an avatar in the list, collapsing to avatar + first name in the box.
        private static Dropdown.Item PersonRow(string name, string email) =>
            DropdownItem(PersonBlock(name, email), PersonChip(name)).SetKey(email).SetData(Initials(name));

        // A badge is already a self-contained pill, so the box shows the same thing the list does.
        private static Dropdown.Item BadgeItem(string text, BadgeTone tone, UIcons icon) =>
            DropdownItem(BadgeRow(text, tone, icon), Badge(text).Tone(tone).Pill()).SetKey(text);

        private static Dropdown.Item SwatchItem(string name, string color) =>
            DropdownItem(SwatchRow(name, color, 16), SwatchRow(name, color, 10)).SetKey(name);

        // A chart inside an option - the tallest, widest thing here, and the box gets a smaller one.
        private static Dropdown.Item MetricItem(string name, double[] series, string color) =>
            DropdownItem(MetricRow(name, series, color, 90, 24, labelWidth: 70), MetricRow(name, series, color, 36, 12)).SetKey(name);

        // Deliberately gives no short form, so the box has to clip the two-line block itself.
        private static Dropdown.Item TwoLineItem(string title, string subtitle) =>
            DropdownItem(() => TwoLineBlock(title, subtitle)).SetKey(subtitle);

        // --- The same content, arriving through a Defer ----------------------------------------
        // No short form, so the box shows a copy of the row - which is exactly the case that has to
        // catch up once the Defer resolves, and which is broken from the start when the option is
        // selected before its content exists.
        private static int _deferredKey;

        private static Dropdown.Item DeferredItem(int delayMs, Func<IComponent> content, bool selected = false) =>
            DropdownItem(() => Defer(async () =>
                {
                    await Task.Delay(delayMs);
                    return content();
                }, loadMessage: Skeleton().Animated().W(120).H(16)))
               .SetKey($"deferred-{_deferredKey++}")
               .SelectedIf(selected);

        // Both halves deferred, on different delays: the row is copied into the box, but the short form
        // is a live component, so its own Defer resolves in the box itself.
        private static Dropdown.Item DeferredPairItem(int rowDelayMs, int chipDelayMs, string title, string subtitle, bool selected = false) =>
            DropdownItem(
                    () => Defer(async () =>
                    {
                        await Task.Delay(rowDelayMs);
                        return TwoLineBlock(title, subtitle);
                    }, loadMessage: Skeleton().Animated().W(120).H(16)),
                    () => Defer(async () =>
                    {
                        await Task.Delay(chipDelayMs);
                        return TextBlock(title).Small();
                    }, loadMessage: Skeleton().Animated().W(60).H(12)))
               .SetKey(subtitle)
               .SelectedIf(selected);

        // A Defer that is not the item's content but sits inside it, to show that the box mounts the whole
        // subtree it builds rather than just its root.
        private static Dropdown.Item NestedDeferredItem(int delayMs, string title, string subtitle, bool selected = false) =>
            DropdownItem(() => HStack().AlignItemsCenter().Gap(8.px()).Children(
                    Icon(UIcons.Globe),
                    Defer(async () =>
                    {
                        await Task.Delay(delayMs);
                        return TwoLineBlock(title, subtitle);
                    }, loadMessage: Skeleton().Animated().W(100).H(16))))
               .SetKey(subtitle)
               .SelectedIf(selected);

        // The factory runs once for the row and once for the box, so anything expensive in it runs twice
        // unless the work itself is shared. One Task, awaited by both, is all that takes.
        private static int _sharedFetchCount;

        private static Dropdown.Item SharedWorkItem(string name, bool selected = false)
        {
            var fetchOnce = FetchOnceAsync(name);   // started once, here, not inside the factory

            return DropdownItem(() => Defer(async () =>
                {
                    var value = await fetchOnce;
                    return TextBlock(value).Small();
                }, loadMessage: Skeleton().Animated().W(120).H(16)))
               .SetKey(name)
               .SelectedIf(selected);
        }

        private static async Task<string> FetchOnceAsync(string name)
        {
            await Task.Delay(600);
            _sharedFetchCount++;
            return $"{name} (fetched {_sharedFetchCount}x)";
        }

        private static Dropdown StartLoadingAsyncDataImmediately(Dropdown dropdown)
        {
            dropdown.LoadItemsAsync().FireAndForget();
            return dropdown;
        }

        private Dropdown.Item DeferredDropdownItem(string text, int delayMs)
        {
            return DropdownItem(() => Defer(async () =>
            {
                await Task.Delay(delayMs);
                return Label($"Loaded {text}");
            }, loadMessage: Skeleton().Animated().W(120).H(16)));
        }

        // Stands in for the server: 5,000 options that would be senseless to render up front.
        private static readonly string[] _people = Enumerable.Range(1, 5000).Select(i => $"Person {i:0000}").ToArray();

        private static Dropdown.Item[] FirstPageOfPeople() => _people.Take(20).Select(PersonItem).ToArray();

        // Whatever this returns is ADDED to the items already in the dropdown, and anything whose key
        // is already listed is dropped - so returning a page that overlaps the seed is harmless.
        private static async Task<Dropdown.Item[]> SearchPeopleAsync(string searchTerm)
        {
            await Task.Delay(400); // The round trip a real lookup would pay for

            if (string.IsNullOrWhiteSpace(searchTerm)) return FirstPageOfPeople();

            return _people.Where(p => p.ToLower().Contains(searchTerm.ToLower()))
                          .Take(50)
                          .Select(PersonItem)
                          .ToArray();
        }

        // The key is what tells two options apart when their text is not unique, and what lets a
        // lookup return options the dropdown already knows about without duplicating them.
        private static Dropdown.Item PersonItem(string person) => DropdownItem(person).SetKey(person);

        private async Task<Dropdown.Item[]> GetItemsAsync()
        {
            await Task.Delay(5000);
            return new[]
            {
                DropdownItem("Header 1").Header(),
                DropdownItem("Async Item 1"),
                DropdownItem("Async Item 2"),
                DropdownItem().Divider(),
                DropdownItem("Header 2").Header(),
                DropdownItem("Async Item 3")
            };
        }

        public HTMLElement Render() => _content.Render();
    }
}
