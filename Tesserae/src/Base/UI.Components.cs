﻿using System;
using System.Threading.Tasks;
using Tesserae;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.UI")]
    public static partial class UI
    {
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

        public static IComponent If(bool condition, IComponent ifTrue, IComponent ifFalse = null) => condition ? (ifTrue ?? Raw()) : (ifFalse ?? Raw());

        public static IComponent If(bool condition, Func<IComponent> ifTrue, IComponent ifFalse = null) => condition ? (ifTrue?.Invoke() ?? Raw()) : (ifFalse ?? Raw());

        public static IComponent If(bool condition, Func<IComponent> ifTrue, Func<IComponent> ifFalse) => condition ? (ifTrue?.Invoke() ?? Raw()) : (ifFalse?.Invoke() ?? Raw());

        /// <summary>
        /// Adds an ID to the element representing the component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Id<T>(this T component, string id) where T : IComponent
        {
            if (component is DeferedComponent deferedComponent)
            {
                deferedComponent.Container.id = id;
                return component;
            }

            var el = component.Render();
            el.id = id;
            return component;
        }

        /// <summary>
        /// Adds a CSS class to the element representing the component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Class<T>(this T component, string className) where T : IComponent
        {
            if (component is DeferedComponent deferedComponent)
            {
                deferedComponent.Container.classList.add(className);
                return component;
            }

            var el = component.Render();
            el.classList.add(className);
            return component;
        }

        /// <summary>
        /// Remove a CSS class to the element representing the component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T RemoveClass<T>(this T component, string className) where T : IComponent
        {
            if (component is DeferedComponent deferedComponent)
            {
                deferedComponent.Container.classList.remove(className);
                return component;
            }

            var el = component.Render();
            el.classList.remove(className);
            return component;
        }

        /// <summary>
        /// Creates a wrapper IComponent from an HTML element
        /// </summary>
        /// <param name="element">HTML element to be wrapped</param>
        /// <returns></returns>
        public static Raw Raw(HTMLElement element) => new Raw(element);

        public static Raw Raw() => new Raw();

        public static Raw Raw(IComponent component) => new Raw(component);

        public static IComponent Empty() => new Raw();

        public static Image Image(string source, string fallback = null) => new Image(source, fallback);

        public static Card Card(IComponent content) => new Card(content);

        public static BackgroundArea BackgroundArea(IComponent content) => new BackgroundArea(content);

        //Note: the Defer method with optional loadMessage caused a bridge compiler issue when resolving the method, so we provide here both with and without the loadMessage method


        public static IDefer Defer(Func<Task<IComponent>> asyncGenerator) => DeferedComponent.Create(asyncGenerator);

        public static IDefer DeferSync(Func<IComponent> syncGenerator) => DeferedComponent.Create(() => syncGenerator().AsTask());

        public static IDefer Defer<TComponent>(IObservable<TComponent> observableComponent) where TComponent : IComponent => DeferedComponent.Observe(observableComponent, c => c.AsTask());

        public static IDefer Defer<T1>(IObservable<T1> o1, Func<T1, Task<IComponent>> asyncGenerator) => DeferedComponent.Observe(o1, asyncGenerator);

        public static IDefer Defer<T1, T2>(IObservable<T1> o1, IObservable<T2> o2, Func<T1, T2, Task<IComponent>> asyncGenerator) => DeferedComponent.Observe(o1, o2, asyncGenerator);

        public static IDefer Defer<T1, T2, T3>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, Func<T1, T2, T3, Task<IComponent>> asyncGenerator) => DeferedComponent.Observe(o1, o2, o3, asyncGenerator);

        public static IDefer Defer<T1, T2, T3, T4>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, Func<T1, T2, T3, T4, Task<IComponent>> asyncGenerator) => DeferedComponent.Observe(o1, o2, o3, o4, asyncGenerator);

        public static IDefer Defer<T1, T2, T3, T4, T5>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, Func<T1, T2, T3, T4, T5, Task<IComponent>> asyncGenerator) => DeferedComponent.Observe(o1, o2, o3, o4, o5, asyncGenerator);

        public static IDefer Defer<T1, T2, T3, T4, T5, T6>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, Func<T1, T2, T3, T4, T5, T6, Task<IComponent>> asyncGenerator) => DeferedComponent.Observe(o1, o2, o3, o4, o5, o6, asyncGenerator);

        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, Func<T1, T2, T3, T4, T5, T6, T7, Task<IComponent>> asyncGenerator) => DeferedComponent.Observe(o1, o2, o3, o4, o5, o6, o7, asyncGenerator);

        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7, T8>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<IComponent>> asyncGenerator) => DeferedComponent.Observe(o1, o2, o3, o4, o5, o6, o7, o8, asyncGenerator);

        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, IObservable<T9> o9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task<IComponent>> asyncGenerator) => DeferedComponent.Observe(o1, o2, o3, o4, o5, o6, o7, o8, o9, asyncGenerator);

        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, IObservable<T9> o9, IObservable<T10> o10, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task<IComponent>> asyncGenerator) => DeferedComponent.Observe(o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, asyncGenerator);

        public static IDefer Defer(Func<Task<IComponent>> asyncGenerator, IComponent loadMessage) => DeferedComponent.Create(asyncGenerator, loadMessage);

        public static IDefer DeferSync(Func<IComponent> syncGenerator, IComponent loadMessage) => DeferedComponent.Create(() => syncGenerator().AsTask(), loadMessage);

        public static IDefer Defer<T1>(IObservable<T1> o1, Func<T1, Task<IComponent>> asyncGenerator, IComponent loadMessage) => DeferedComponent.Observe(o1, asyncGenerator, loadMessage);

        public static IDefer Defer<T1, T2>(IObservable<T1> o1, IObservable<T2> o2, Func<T1, T2, Task<IComponent>> asyncGenerator, IComponent loadMessage) => DeferedComponent.Observe(o1, o2, asyncGenerator, loadMessage);

        public static IDefer Defer<T1, T2, T3>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, Func<T1, T2, T3, Task<IComponent>> asyncGenerator, IComponent loadMessage) => DeferedComponent.Observe(o1, o2, o3, asyncGenerator, loadMessage);

        public static IDefer Defer<T1, T2, T3, T4>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, Func<T1, T2, T3, T4, Task<IComponent>> asyncGenerator, IComponent loadMessage) => DeferedComponent.Observe(o1, o2, o3, o4, asyncGenerator, loadMessage);

        public static IDefer Defer<T1, T2, T3, T4, T5>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, Func<T1, T2, T3, T4, T5, Task<IComponent>> asyncGenerator, IComponent loadMessage) => DeferedComponent.Observe(o1, o2, o3, o4, o5, asyncGenerator, loadMessage);

        public static IDefer Defer<T1, T2, T3, T4, T5, T6>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, Func<T1, T2, T3, T4, T5, T6, Task<IComponent>> asyncGenerator, IComponent loadMessage) => DeferedComponent.Observe(o1, o2, o3, o4, o5, o6, asyncGenerator, loadMessage);

        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, Func<T1, T2, T3, T4, T5, T6, T7, Task<IComponent>> asyncGenerator, IComponent loadMessage) => DeferedComponent.Observe(o1, o2, o3, o4, o5, o6, o7, asyncGenerator, loadMessage);

        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7, T8>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<IComponent>> asyncGenerator, IComponent loadMessage) => DeferedComponent.Observe(o1, o2, o3, o4, o5, o6, o7, o8, asyncGenerator, loadMessage);

        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, IObservable<T9> o9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task<IComponent>> asyncGenerator, IComponent loadMessage) => DeferedComponent.Observe(o1, o2, o3, o4, o5, o6, o7, o8, o9, asyncGenerator, loadMessage);

        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, IObservable<T9> o9, IObservable<T10> o10, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task<IComponent>> asyncGenerator, IComponent loadMessage) => DeferedComponent.Observe(o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, asyncGenerator, loadMessage);

        /// <summary>
        /// A Stack is a container-type component that abstracts the implementation of a flexbox in order to define the layout of its children components.
        /// </summary>
        public static Stack Stack(Stack.Orientation orientation = Tesserae.Stack.Orientation.Vertical) => new Stack(orientation);
        
        public static Masonry Masonry(int columns, int gutter = 10) => new Masonry(columns: columns, gutter: gutter);

        /// <summary>
        /// Horizontal stack, same as calling Stack().Horizontal()
        /// </summary>
        /// <returns></returns>
        public static Stack HStack() => new Stack(Tesserae.Stack.Orientation.Horizontal);


        /// <summary>
        /// Horizontal stack, same as calling Stack() or Stack().Vertical()
        /// </summary>
        /// <returns></returns>
        public static Stack VStack() => new Stack(Tesserae.Stack.Orientation.Vertical);

        public static Grid Grid(params UnitSize[] columns)                  => new Grid(columns);
        public static Grid Grid(UnitSize[]        columns, UnitSize[] rows) => new Grid(columns, rows);

        public static SectionStack SectionStack() => new SectionStack();

        public static Float Float(IComponent child, Float.Position position) => new Float(child, position);

        public static Button Button(string text = string.Empty) => new Button(text);

        public static CheckBox CheckBox(string text = string.Empty) => new CheckBox(text);

        public static Toggle Toggle(IComponent onText, IComponent offText) => new Toggle(onText: onText, offText: offText);

        public static Toggle Toggle(string onText, string offText) => new Toggle(onText: TextBlock(onText), offText: TextBlock(offText));

        public static Toggle Toggle(string text) => new Toggle(onText: TextBlock(text), offText: TextBlock(text).Secondary());

        public static Toggle Toggle() => new Toggle(null, null);

        public static ChoiceGroup.Choice Choice(string label = string.Empty) => new ChoiceGroup.Choice(label);

        public static ChoiceGroup ChoiceGroup(string label = string.Empty) => new ChoiceGroup(label);

        public static TextBlock TextBlock(string text = string.Empty, bool treatAsHTML = false, bool selectable = false, TextSize textSize = TextSize.Small, TextWeight textWeight = TextWeight.Regular) => new TextBlock(text, treatAsHTML, selectable, textSize, textWeight);

        public static FileSelector FileSelector() => new FileSelector();

        public static FileDropArea FileDropArea() => new FileDropArea();

        public static Validator Validator() => new Validator();

        public static Icon Icon(string icon, string color = null) => new Icon(icon).Foreground(color ?? "");

        public static Icon Icon(string icon, TextSize size, string color = null) => new Icon($"{icon} {size}").Foreground(color ?? "");

        public static Icon Icon(LineAwesome icon, LineAwesomeWeight weight = LineAwesomeWeight.Light, TextSize size = TextSize.Medium, string color = null) => new Icon($"{weight} {icon} {size}").Foreground(color ?? "");

        public static HorizontalSeparator HorizontalSeparator(string text) => new HorizontalSeparator(text);

        public static HorizontalSeparator HorizontalSeparator(IComponent component) => new HorizontalSeparator(component);

        public static Label Label(string text = string.Empty) => new Label(text);

        public static Label Label(IComponent component) => new Label(component);

        public static EditableLabel EditableLabel(string text = string.Empty) => new EditableLabel(text);

        public static EditableArea EditableArea(string text = string.Empty) => new EditableArea(text);

        public static Breadcrumb Breadcrumb() => new Breadcrumb();

        public static TextBreadcrumbs TextBreadcrumbs() => new TextBreadcrumbs();

        public static TextBreadcrumb TextBreadcrumb(string text = string.Empty) => new TextBreadcrumb(text);

        public static Button Crumb(string text = string.Empty) => new Button(text).NoBorder().NoBackground();

        public static OverflowSet OverflowSet() => new OverflowSet();

        public static TextBox TextBox(string text = string.Empty) => new TextBox(text);

        public static TextArea TextArea(string text = string.Empty) => new TextArea(text);

        public static ColorPicker ColorPicker(Color color = null) => new ColorPicker(color);

        public static DateTimePicker DateTimePicker(DateTimeOffset? dateTime = null) => new DateTimePicker(dateTime);
        public static DatePicker     DatePicker(DateTimeOffset?     dateTime = null) => new DatePicker(dateTime);

        public static SearchBox SearchBox(string placeholder = string.Empty) => new SearchBox(placeholder);

        public static Slider Slider(int val = 0, int min = 0, int max = 100, int step = 10) => new Slider(val, min, max, step);

        /// <summary>
        /// A Layer is a technical component that does not have specific Design guidance.
        ///
        /// Layers are used to render content outside of a DOM tree, at the end of the document. This allows content to escape traditional boundaries caused by "overflow: hidden" css rules and keeps it on the top without using z-index rules. This is useful for example in
        /// ContextualMenu and Tooltip scenarios, where the content should always overlay everything else.
        /// </summary>
        public static Layer Layer() => new Layer();

        public static LayerHost LayerHost() => new LayerHost();

        public static Nav Nav() => new Nav();

        public static Nav.NavLink NavLink(string text = null) => new Nav.NavLink(text);

        public static Nav.NavLink NavLink(IComponent content) => new Nav.NavLink(content);

        public static Panel Panel(string     title = null) => new Panel(title);
        public static Panel Panel(IComponent title)        => new Panel(title);

        public static Modal Modal(IComponent header = null) => new Modal(header);

        public static Modal Modal(string header) => new Modal(string.IsNullOrWhiteSpace(header) ? null : TextBlock(header).MediumPlus().SemiBold());

        public static TutorialModal TutorialModal()                              => new TutorialModal("",    "");
        public static TutorialModal TutorialModal(string title, string helpText) => new TutorialModal(title, helpText);

        public static ProgressModal ProgressModal() => new ProgressModal();

        public static Dialog Dialog(IComponent content = null, IComponent title = null, bool centerContent = true) => new Dialog(content, title, centerContent);

        public static Dialog Dialog(string text, bool centerContent = true) => new Dialog(title: string.IsNullOrWhiteSpace(text) ? null : TextBlock(text).Primary().SemiBold().MaxWidth(50.vw()), centerContent: centerContent);

        public static Pivot Pivot() => new Pivot();

        public static Sidebar Sidebar() => new Sidebar();

        public static Sidebar.Item SidebarItem(string text, string icon, string href = null) => new Sidebar.Item(text, icon, href);

        public static Sidebar.Item SidebarItem(string     text, IComponent icon, string href = null) => new Sidebar.Item(text, icon, href);
        public static Sidebar.Item SidebarItem(IComponent text, string     icon, string href = null) => new Sidebar.Item(text, icon, href);
        public static Sidebar.Item SidebarItem(IComponent text, string     href              = null) => new Sidebar.Item(text, href);
        public static Sidebar.Item SidebarItem(IComponent text, IComponent icon, string href = null) => new Sidebar.Item(text, icon, href);

        public static Navbar Navbar() => new Navbar();

        public static Toast Toast() => new Toast();

        public static ProgressIndicator ProgressIndicator() => new ProgressIndicator();

        public static Dropdown Dropdown()                            => new Dropdown();
        public static Dropdown Dropdown(string          noItemsText) => new Dropdown(noItemsSpan: string.IsNullOrWhiteSpace(noItemsText) ? null : Span(_(text: noItemsText)));
        public static Dropdown Dropdown(HTMLSpanElement noItemsSpan) => new Dropdown(noItemsSpan);

        public static Dropdown.Item DropdownItem() => new Dropdown.Item("");

        public static Dropdown.Item DropdownItem(string text, string selectedText = string.Empty) => new Dropdown.Item(text, selectedText);

        public static Dropdown.Item DropdownItem(IComponent content, IComponent selectedContent = null) => new Dropdown.Item(content, selectedContent);

        public static ContextMenu ContextMenu() => new ContextMenu();

        public static ContextMenu.Item ContextMenuItem(string text = string.Empty) => new ContextMenu.Item(text);

        public static ContextMenu.Item ContextMenuItem(IComponent component) => new ContextMenu.Item(component);

        public static Spinner Spinner(string text = string.Empty) => new Spinner(text);

        public static Link Link(string url, IComponent content, bool noUnderline = false) => new Link(url, content, noUnderline);

        public static Link Link(string url, string text) => new Link(url, TextBlock(text));

        public static Link Link(string url, string text, string icon, bool noUnderline = false) => new Link(url, Button(text).SetIcon(icon).NoBorder().NoBackground().Padding(0.px()), noUnderline);

        public static SplitView SplitView() => new SplitView();

        public static VirtualizedList VirtualizedList(int rowsPerPage = 4, int columnsPerRow = 4) => new VirtualizedList(rowsPerPage, columnsPerRow);

        public static SearchableList<T> SearchableList<T>(T[] components, params UnitSize[] columns) where T : ISearchableItem => new SearchableList<T>(components, columns);

        public static SearchableList<T> SearchableList<T>(ObservableList<T> components, params UnitSize[] columns) where T : ISearchableItem => new SearchableList<T>(components, columns);

        public static SearchableGroupedList<T> SearchableGroupedList<T>(T[] components, Func<string, IComponent> groupedItemHeaderGenerator, params UnitSize[] columns) where T : ISearchableGroupedItem => new SearchableGroupedList<T>(components, groupedItemHeaderGenerator, columns);

        public static SearchableGroupedList<T> SearchableGroupedList<T>(ObservableList<T> components, Func<string, IComponent> groupedItemHeaderGenerator, params UnitSize[] columns) where T : ISearchableGroupedItem => new SearchableGroupedList<T>(components, groupedItemHeaderGenerator, columns);

        public static ItemsList ItemsList(IComponent[] components, params UnitSize[] columns) => new ItemsList(components, columns);

        public static ItemsList ItemsList(ObservableList<IComponent> components, params UnitSize[] columns) => new ItemsList(components, columns);

        public static InfiniteScrollingList InfiniteScrollingList(Func<IComponent[]>       getNextItemPage, params UnitSize[]        columns)                                    => new InfiniteScrollingList(getNextItemPage, columns);
        public static InfiniteScrollingList InfiniteScrollingList(Func<Task<IComponent[]>> getNextItemPage, params UnitSize[]        columns)                                    => new InfiniteScrollingList(getNextItemPage, columns);
        public static InfiniteScrollingList InfiniteScrollingList(IComponent[]             initComponents,  Func<Task<IComponent[]>> getNextItemPage, params UnitSize[] columns) => new InfiniteScrollingList(initComponents,  getNextItemPage, columns);
        public static InfiniteScrollingList InfiniteScrollingList(IComponent[]             initComponents,  Func<IComponent[]>       getNextItemPage, params UnitSize[] columns) => new InfiniteScrollingList(initComponents,  getNextItemPage, columns);

        public static DetailsList<TDetailsListItem> DetailsList<TDetailsListItem>(params IDetailsListColumn[] columns) where TDetailsListItem : class, IDetailsListItem<TDetailsListItem> => new DetailsList<TDetailsListItem>(columns);

        public static DetailsListIconColumn IconColumn(Icon icon, UnitSize width, bool     enableColumnSorting = false, string sortingKey          = null,  Action onColumnClick = null)                              => new DetailsListIconColumn(icon, width, null,     enableColumnSorting, sortingKey, onColumnClick);
        public static DetailsListIconColumn IconColumn(Icon icon, UnitSize width, UnitSize maxWidth,                    bool   enableColumnSorting = false, string sortingKey    = null, Action onColumnClick = null) => new DetailsListIconColumn(icon, width, maxWidth, enableColumnSorting, sortingKey, onColumnClick);

        public static DetailsListColumn DetailsListColumn(string title, UnitSize width, bool     isRowHeader = false, bool enableColumnSorting = false, string sortingKey          = null,  Action onColumnClick = null)                              => new DetailsListColumn(title, width, null,     isRowHeader, enableColumnSorting, sortingKey, onColumnClick);
        public static DetailsListColumn DetailsListColumn(string title, UnitSize width, UnitSize maxWidth,            bool isRowHeader         = false, bool   enableColumnSorting = false, string sortingKey    = null, Action onColumnClick = null) => new DetailsListColumn(title, width, maxWidth, isRowHeader, enableColumnSorting, sortingKey, onColumnClick);

        public static Picker<TPickerItem> Picker<TPickerItem>(int maximumAllowedSelections = int.MaxValue, bool duplicateSelectionsAllowed = false, int suggestionsTolerance = 0, bool renderSelectionsInline = true, string suggestionsTitleText = null) where TPickerItem : class, IPickerItem => new Picker<TPickerItem>(maximumAllowedSelections, duplicateSelectionsAllowed, suggestionsTolerance, renderSelectionsInline, suggestionsTitleText);

        public static VisibilitySensor VisibilitySensor(Action<VisibilitySensor> onVisible, bool singleCall = true, IComponent message = null) => new VisibilitySensor(onVisible, singleCall, message);

        public static CombinedObservable<T1, T2> Combine<T1, T2>(IObservable<T1> o1, IObservable<T2> o2) => new CombinedObservable<T1, T2>(o1, o2);

        public static Timeline     Timeline()                   => new Timeline();
        public static Teaching     Teaching()                   => new Teaching();
        public static ToggleButton ToToggle(this Button button) => new ToggleButton(button);
    }
}