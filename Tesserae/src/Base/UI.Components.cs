using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tesserae.Components;
using static Retyped.dom;

namespace Tesserae
{
    public static partial class UI
    {
        public delegate bool BeforeSelectEventHandler<TSender>(TSender sender);

        /// <summary>
        /// Helper method to capture the current component inline on it's definition, as an out variable
        /// </summary>
        /// <typeparam name="T">Any component implementing <see cref="IComponent"/></typeparam>
        /// <param name="component"></param>
        /// <param name="var">Capture variable</param>
        /// <returns>itself</returns>
        public static T Var<T>(this T component, out T var) where T : IComponent
        {
            var = component;
            return component;
        }

        public static T Do<T>(this T component, Action<T> action) where T : IComponent
        {
            action(component);
            return component;
        }

        /// <summary>
        /// Adds an ID to the element representing the component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Id<T>(this T component, string id) where T : IComponent
        {
            if(component is Defer deferedComponent)
            {
                deferedComponent.InnerElement.id = id;
                return component;
            }
            var el = component.Render();
            el.id = id;
            return component;
        }

        /// <summary>
        /// Creates a wrapper IComponent from an HTML element
        /// </summary>
        /// <param name="element">HTML element to be wrapped</param>
        /// <returns></returns>
        public static Raw Raw(HTMLElement element) => new Raw(element);

        public static Raw Raw() => new Raw(null);
        public static Card Card(IComponent content) => new Card(content);

        public static Defer Defer(Func<Task<IComponent>> asyncGenerator) => new Defer(asyncGenerator);

        public static Defer DeferSync(Func<IComponent> syncGenerator) => new Defer(() => Task.FromResult<IComponent>(syncGenerator()));

        public static Stack Stack(Stack.Orientation orientation = Components.Stack.Orientation.Vertical) => new Stack(orientation);

        public static Grid Grid(params UnitSize[] columns) => new Grid(columns);

        public static SectionStack SectionStack() => new SectionStack();

        public static Button Button(string text = string.Empty) => new Button(text);

        public static CheckBox CheckBox(string text = string.Empty) => new CheckBox(text);

        public static Toggle Toggle(string text = string.Empty) => new Toggle(text);

        public static Toggle Toggle(string onText, string offText) => new Toggle(onText: onText, offText: offText);

        public static ChoiceGroup.Option Option(string label = string.Empty) => new ChoiceGroup.Option(label);

        public static ChoiceGroup ChoiceGroup(string label = "Pick one") => new ChoiceGroup(label);

        public static TextBlock TextBlock(string text = string.Empty) => new TextBlock(text);

        public static FileSelector FileSelector() => new FileSelector();

        public static FileDropArea FileDropArea() => new FileDropArea();

        public static Validator Validator() => new Validator();

        public static Icon Icon(string icon) => new Icon(icon);

        public static HorizontalSeparator HorizontalSeparator(string text) => new HorizontalSeparator(text);

        public static HorizontalSeparator HorizontalSeparator(IComponent component) => new HorizontalSeparator(component);

        public static Label Label(string text = string.Empty) => new Label(text);

        public static Label Label(IComponent component) => new Label(component);

        public static EditableLabel EditableLabel(string text = string.Empty) => new EditableLabel(text);

        public static EditableArea EditableArea(string text = string.Empty) => new EditableArea(text);

        public static Breadcrumb Breadcrumb() => new Breadcrumb();

        public static Button Crumb(string text = string.Empty) => new Button(text).NoBorder().NoBackground();

        public static OverflowSet OverflowSet() => new OverflowSet();

        public static TextBox TextBox(string text = string.Empty) => new TextBox(text);

        public static SearchBox SearchBox(string placeholder = string.Empty) => new SearchBox(placeholder);

        public static Slider Slider(int val = 0, int min = 0, int max = 100, int step = 10) => new Slider(val, min, max, step);

        public static Layer Layer() => new Layer();

        public static LayerHost LayerHost() => new LayerHost();

        public static Nav Nav() => new Nav();

        public static Nav.NavLink NavLink(string text = null, string icon = null) => new Nav.NavLink(text, icon);

        public static Nav.NavLink NavLink(IComponent content) => new Nav.NavLink(content);

        public static Panel Panel() => new Panel();

        public static Modal Modal(IComponent header = null) => new Modal(header);

        public static Modal Modal(string header) => new Modal(string.IsNullOrWhiteSpace(header) ? null : TextBlock(header).MediumPlus().SemiBold());

        public static ProgressModal ProgressModal() => new ProgressModal();

        public static Dialog Dialog(IComponent header = null) => new Dialog(header);

        public static Dialog Dialog(string header) => new Dialog(string.IsNullOrWhiteSpace(header) ? null : TextBlock(header).MediumPlus().SemiBold());

        public static Pivot Pivot() => new Pivot();

        public static Sidebar Sidebar() => new Sidebar();

        public static Sidebar.Item SidebarItem(string text, string icon) => new Sidebar.Item(text, icon);

        public static Sidebar.Item SidebarItem(string text, IComponent icon) => new Sidebar.Item(text, icon);

        public static Navbar Navbar() => new Navbar();

        public static Toast Toast() => new Toast();

        public static ProgressIndicator ProgressIndicator() => new ProgressIndicator();

        public static Dropdown Dropdown() => new Dropdown();

        public static Dropdown.Item DropdownItem(string text = string.Empty) => new Dropdown.Item(text);

        public static Dropdown.Item DropdownItem(IComponent component) => new Dropdown.Item(component);

        public static ContextMenu ContextMenu() => new ContextMenu();

        public static ContextMenu.Item ContextMenuItem(string text = string.Empty) => new ContextMenu.Item(text);

        public static ContextMenu.Item ContextMenuItem(IComponent component) => new ContextMenu.Item(component);

        public static Spinner Spinner(string text = string.Empty) => new Spinner(text);

        public static Link Link(string url, IComponent content) => new Link(url, content);

        public static Link Link(string url, string text) => new Link(url, TextBlock(text));

        public static Link Link(string url, string text, string icon) => new Link(url, Button(text).SetIcon(icon).NoBorder().NoBackground().Padding(0.px()));

        public static SplitView SplitView() => new SplitView();

        public static VirtualizedList VirtualizedList(IEnumerable<IComponent> components, int rowsPerPage = 4, int columnsPerRow = 4) => new VirtualizedList(components, rowsPerPage, columnsPerRow);

        public static SearchableList<T> SearchableList<T>(IEnumerable<T> components, params UnitSize[] columns) where T : ISearchableItem => new SearchableList<T>(components.ToArray(), columns);

        public static SearchableList<T> SearchableList<T>(ObservableList<T> components, params UnitSize[] columns) where T : ISearchableItem => new SearchableList<T>(components, columns);

        public static ItemsList ItemsList(IEnumerable<IComponent> components, params UnitSize[] columns)=> new ItemsList(components.ToArray(), columns);

        public static ItemsList ItemsList(ObservableList<IComponent> components, params UnitSize[] columns)  => new ItemsList(components, columns);

        public static DetailsList<TDetailsListItem> DetailsList<TDetailsListItem>(bool small = false) where TDetailsListItem : class, IDetailsListItem<TDetailsListItem> => new DetailsList<TDetailsListItem>(small);

        public static DetailsListIconColumn IconColumn(LineAwesome lineAwesomeIcon, UnitSize width, LineAwesomeSize lineAwesomeSize = LineAwesomeSize.Default, bool enableColumnSorting = false, string sortingKey = null, Action onColumnClick = null) => new DetailsListIconColumn(lineAwesomeIcon, width, lineAwesomeSize, enableColumnSorting, sortingKey, onColumnClick);

        public static DetailsListColumn DetailsListColumn(string title, UnitSize width, bool isRowHeader = false, bool enableColumnSorting = false, string sortingKey = null, Action onColumnClick = null) => new DetailsListColumn(title, width, isRowHeader, enableColumnSorting, sortingKey, onColumnClick);

        public static Picker<TPickerItem> Picker<TPickerItem>(int maximumAllowedSelections = 5, bool
        duplicateSelectionsAllowed = false, int suggestionsTolerance = 2,  bool renderSelectionsInline = true,
        string suggestionsTitleText = null) where TPickerItem : class,
        IPickerItem => new Picker<TPickerItem>(maximumAllowedSelections, duplicateSelectionsAllowed,
        suggestionsTolerance, renderSelectionsInline, suggestionsTitleText);

        public static VisibilitySensor VisibilitySensor(Action<VisibilitySensor> onVisible, bool singleCall = true, IComponent message = null) => new VisibilitySensor(onVisible, singleCall, message);
    }
}
