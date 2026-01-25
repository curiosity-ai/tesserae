using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.CaretSquareDown)]
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

            _content = SectionStack()
               .Title(SampleHeader(nameof(DropdownSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("A Dropdown is a list in which the selected item is always visible, and the others are visible on demand by clicking a drop-down button."),
                    TextBlock("They are used to simplify the design and make a choice within the UI. When closed, only the selected item is visible. When users click the drop-down button, all the options become visible.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use a Dropdown when there are multiple choices that can be collapsed under one title, especially if the list of items is long or when space is constrained. Use shortened statements or single words as options. Dropdowns are preferred over radio buttons when the selected option is more important than the alternatives. For less than 7 options, consider using a ChoiceGroup if space allows.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
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
                    SampleSubTitle("Async Loading"),
                    VStack().Children(
                        Label("Load on open (5s delay)").SetContent(Dropdown().Items(GetItemsAsync)),
                        Label("Load immediately (5s delay)").SetContent(StartLoadingAsyncDataImmediately(Dropdown().Items(GetItemsAsync))),
                        Label("Empty State").SetContent(Dropdown("No items available").Items(new Dropdown.Item[0]))
                    ),
                    SampleSubTitle("Validation"),
                    VStack().Children(
                        Label("Required Dropdown").SetContent(Dropdown().Required().Items(
                            DropdownItem("Choose one...").Header(),
                            DropdownItem("Valid Choice")
                        )),
                        Label("Validation (Must select 'Option 1')").SetContent(validatedDropdown)
                    )
                ));
        }

        private static Dropdown StartLoadingAsyncDataImmediately(Dropdown dropdown)
        {
            dropdown.LoadItemsAsync().FireAndForget();
            return dropdown;
        }

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
