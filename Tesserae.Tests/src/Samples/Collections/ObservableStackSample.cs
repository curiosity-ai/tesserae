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
        public static  ObservableList<IComponentWithID> StackElementsList1;


        public ObservableStackSample()
        {
            var mainButton = Button("Some Text").TextLeft().MinWidth(200.px()).Ellipsis().IconOnHover();
            mainButton.Tooltip("Tooltip for the main Button").SetIcon(UIcons.AngleLeft, Theme.Primary.Background);

            StackElementsList1 = new ObservableList<IComponentWithID>();

            StackElementsList1.Observe(c => console.log(string.Join(", ", c.Select(e => e.Identifier))));

            var obsStack = new ObservableStack(StackElementsList1, debounce: true);

            StackElementsList1.ReplaceAll(UpdateIsLast(Enumerable.Range(0, 4).Select(i => new StackElements(displayName: i.ToString(), id: i.ToString())).ToArray()));
            ElementIndex = 4;

            _content = SectionStack()
               .Title(SampleHeader(nameof(ObservableStackSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("A stack that observes a list of elements with an Identifier and a ContentHash. "
                      + "Opposed to a Defer, which rerenders the whole element. "
                      + "This Stack uses reconciliation to edit the DOM nodes to match the items in the observable liest. "
                      + "The data filed on the DOM elements is used to attach an identifer and a hash to the element to detect changes. ")))
               .Section(Stack().Children(SampleTitle("Observable Stack"),
                        SplitView()
                           .Left(VStack().Children(
                                HStack().Children(
                                    Button("Check").OnClick(() =>
                                    {
                                        if (obsStack.RunCheckIfReconcileWorked(StackElementsList1.Value))
                                        {
                                            Toast().Success("Check success");
                                        }
                                        else
                                        {
                                            Toast().Warning("Check failed");
                                        }
                                    }),
                                    Button("Randomize").OnClick(() =>
                                    {
                                        StackElementsList1.ReplaceAll(UpdateIsLast(StackElementsList1.Value.ToList().OrderBy(_ => Math.Random()).ToList()));
                                    })),
                                Label("Edit Elements").SetContent(
                                    VStack().Children(DeferSync(StackElementsList1, currentElements =>
                                        {
                                            var stack = VStack();

                                            for (int i = 0; i < currentElements.Count; i++)
                                            {
                                                var index = i;

                                                stack.Add(
                                                    HStack().AlignItemsCenter().WS().JustifyContent(ItemJustify.Evenly).Children(Button().SetIcon(UIcons.ArrowUp).OnClick(() =>
                                                        {
                                                            console.log($"{index} move up");
                                                            var newList = StackElementsList1.Value.ToList();
                                                            MoveItem(newList, index, Math.Max(index - 1, 0));
                                                            StackElementsList1.ReplaceAll(UpdateIsLast(newList));
                                                        }),
                                                        Button().SetIcon(UIcons.ArrowDown).OnClick(() =>
                                                        {
                                                            console.log($"{index} move down");

                                                            var newList = StackElementsList1.Value.ToList();
                                                            MoveItem(newList, index, Math.Min(index + 1, newList.Count - 1));
                                                            StackElementsList1.ReplaceAll(UpdateIsLast(newList));
                                                        }),
                                                        TextBox(currentElements[i].As<StackElements>().DisplayName).OnBlur((tb, e) =>
                                                        {
                                                            console.log($"{index} rename");

                                                            var updatedText = tb.Text;

                                                            var newList = StackElementsList1.Value.ToList();

                                                            var reference = newList[index].As<StackElements>();
                                                            reference.DisplayName = updatedText;

                                                            newList[index] = reference;

                                                            StackElementsList1.ReplaceAll(UpdateIsLast(newList));


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

        private static List<IComponentWithID> UpdateIsLast(IReadOnlyList<IComponentWithID> stackElementsList)
        {
            var elements = stackElementsList.ToList();
//            bool anyChanged = false;
//
//            for (int i = 0; i < elements.Count; i++)
//            {
//                var element       = elements[i];
//                var isLastElement = (i == elements.Count - 1);
//
//                if (element is StackElements stackElement && stackElement.IsLast != isLastElement)
//                {
//                    stackElement.IsLast = isLastElement;
//                    anyChanged          = true;
//                }
//            }

            return elements;
        }

        public static void Add()
        {
            console.log($"add");

            ElementIndex++;
            var newList = StackElementsList1.Value.ToList();
            newList.Add(new StackElements(displayName: ElementIndex.ToString(), id: ElementIndex.ToString()));
            StackElementsList1.ReplaceAll(UpdateIsLast(newList));
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