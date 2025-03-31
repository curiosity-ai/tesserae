using System;
using System.Collections.Generic;
using System.Linq;
using H5;
using H5.Core;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Tesserae.Tests;

namespace Tesserae.Tests.src.Samples.Collections
{
    [SampleDetails(Group = "Collections", Order = 0, Icon = UIcons.RulerVertical)]
    public class ObservableStackSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public class StackElements : IComponentWithID
        {
            public string Id          { get; set; }
            public string DisplayName { get; set; }
            public string Color       { get; set; }

            private static string GetRandomColor()
            {
                return '#' + Math.Floor(Math.Random() * 16777215).ToString(16).PadLeft(6, '0');
            }

            public StackElements(string id, string displayName)
            {
                Id          = id;
                DisplayName = displayName;
                Color       = GetRandomColor();
            }

            public HTMLElement Render()
            {
                var card = Card(VStack().Children(
                    Label("DisplayName").Inline().AutoWidth().SetContent(TextBlock(DisplayName)),
                    Label("Id").Inline().AutoWidth().SetContent(TextBlock(Id)),
                    Label("ContentHash").Inline().AutoWidth().SetContent(TextBlock(ContentHash)).Background(Color)
                ).Class("flash-bg"));

                return card.Render();
            }

            public string Identifier  => Id;
            public string ContentHash => HashingHelper.Fnv1aHash(Id + DisplayName).ToString();

        }

        private static int                              _viewBoxHeight = 500;
        public static  int                              ElementIndex;
        public static  ObservableList<IComponentWithID> StackElementsList;


        public ObservableStackSample()
        {
            var mainButton = Button("Some Text").TextLeft().MinWidth(200.px()).Ellipsis().IconOnHover();
            mainButton.Tooltip("Tooltip for the main Button").SetIcon(UIcons.AngleLeft, Theme.Primary.Background);

            StackElementsList = new ObservableList<IComponentWithID>();


            var obsStack = new ObservableStack(StackElementsList, debounce: true);

            StackElementsList.ReplaceAll(Enumerable.Range(0, 4).Select(i => new StackElements(displayName: i.ToString(), id: i.ToString())).ToArray());
            ElementIndex = 4;

            _content = SectionStack()
               .Title(SampleHeader(nameof(ObservableStackSample)))
               .Section(Stack().Children(
                        SampleTitle("Overview"),
                        TextBlock("The ObservableStack is a container display element that efficiently updates its contents using reconciliation rather than full re-renders. Unlike the Defer component, which refreshes entirely based on observables, the Stack selectively patches existing DOM elements to reflect changes in an observable list.\n"
                          + "Each element in the Stack is tracked using an Identifier and a ContentHash, which are stored in the DOM’s data field. This allows the component to compare updates without inspecting the actual rendered content. If an element’s position and hash remain unchanged, it is left untouched; otherwise, it is updated in place. This approach significantly improves performance, especially in scenarios where maintaining UI state—such as scroll position—is critical.\n"
                          + "The reconciliation process is optimized to update only necessary parts of the DOM, preventing unnecessary reflows and repaints. Additionally, a debounce mechanism helps control update frequency, though care should be taken to avoid race conditions.\n"
                          + "The Stack integrates seamlessly with the frontend library’s observable list feature, making it an ideal choice for dynamic interfaces that require efficient, incremental updates.").BreakSpaces()
                    )
                )
               .Section(Stack().Children(SampleTitle("Observable Stack"),
                        SplitView()
                           .Left(VStack().Children(
                                Button("Randomize").OnClick(() =>
                                {
                                    StackElementsList.ReplaceAll(StackElementsList.Value.ToList().OrderBy(_ => Math.Random()).ToList());
                                }),
                                Label("Edit Elements").SetContent(
                                    VStack().Children(DeferSync(StackElementsList, currentElements =>
                                        {
                                            var stack = VStack();

                                            for (int i = 0; i < currentElements.Count; i++)
                                            {
                                                var index = i;

                                                stack.Add(
                                                    HStack().AlignItemsCenter().WS().JustifyContent(ItemJustify.Evenly).Children(Button().SetIcon(UIcons.ArrowUp).OnClick(() =>
                                                        {
                                                            var newList = StackElementsList.Value.ToList();
                                                            MoveItem(newList, index, Math.Max(index - 1, 0));
                                                            StackElementsList.ReplaceAll(newList);
                                                        }),
                                                        Button().SetIcon(UIcons.ArrowDown).OnClick(() =>
                                                        {
                                                            var newList = StackElementsList.Value.ToList();
                                                            MoveItem(newList, index, Math.Min(index + 1, newList.Count - 1));
                                                            StackElementsList.ReplaceAll(newList);
                                                        }),
                                                        TextBox(currentElements[i].As<StackElements>().DisplayName).OnBlur((tb, e) =>
                                                        {
                                                            var updatedText = tb.Text;

                                                            var newList = StackElementsList.Value.ToList();

                                                            var reference = newList[index].As<StackElements>();
                                                            reference.DisplayName = updatedText;

                                                            newList[index] = reference;

                                                            StackElementsList.ReplaceAll(newList);
                                                        }).Background(currentElements[i].As<StackElements>().Color))
                                                );
                                            }
                                            return stack;
                                        }),
                                        Empty().H(1).Grow(),
                                        Button("Add").OnClick(() => Add())))))
                           .Right(Label("Display Elements")
                               .SetContent(
                                    obsStack.H(_viewBoxHeight)))
                    )
                );
        }

        static void MoveItem(List<IComponentWithID> list, int oldIndex, int newIndex)
        {
            var item = list[oldIndex];
            list.RemoveAt(oldIndex);

//            if (newIndex > oldIndex) { newIndex--; }

            list.Insert(newIndex, item);
        }

        public static void Add()
        {
            ElementIndex++;
            var newList = StackElementsList.Value.ToList();
            newList.Add(new StackElements(displayName: ElementIndex.ToString(), id: ElementIndex.ToString()));
            StackElementsList.ReplaceAll(newList);
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
    public static class HashingHelper
    {
        public static int Fnv1aHash(string value)
        {
            var valArray = value.ToCharArray();

            var hash = 0x811c9dc5; // FNV offset basis

            for (var i = 0; i < valArray.Length; i++)
            {
                hash ^= valArray[i];
                hash += (hash << 1) + (hash << 4) + (hash << 7) + (hash << 8) + (hash << 24);
            }
            return Script.Write<int>("{0} >>> 0", hash); // Convert to unsigned 32-bit integer
        }
    }
}