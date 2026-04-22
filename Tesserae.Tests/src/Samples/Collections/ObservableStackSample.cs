using System;
using System.Collections.Generic;
using System.Linq;
using H5;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 0, Icon = UIcons.RulerVertical)]
    public class ObservableStackSample : IComponent, ISample
    {
        private readonly IComponent _content;
        private static int _elementIndex = 4;
        private static ObservableList<IComponentWithID> _stackElementsList;

        public class StackElement : IComponentWithID
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Color { get; }
            public string Identifier => Id;
            public string ContentHash => HashingHelper.Fnv1aHash(Id + DisplayName).ToString();

            public StackElement(string id, string displayName)
            {
                Id = id;
                DisplayName = displayName;
                Color = '#' + Math.Floor(Math.Random() * 16777215).ToString(16).PadLeft(6, '0');
            }

            public HTMLElement Render()
            {
                return Card(VStack().Children(
                    Label("Name").Inline().AutoWidth().SetContent(TextBlock(DisplayName)),
                    Label("ID").Inline().AutoWidth().SetContent(TextBlock(Id)),
                    Label("Hash").Inline().AutoWidth().SetContent(TextBlock(ContentHash).Background(Color).Padding(4.px()))
                ).Class("flash-bg")).Render();
            }
        }

        public ObservableStackSample()
        {
            _stackElementsList = new ObservableList<IComponentWithID>();
            _stackElementsList.ReplaceAll(Enumerable.Range(0, 4).Select(i => new StackElement(i.ToString(), $"Item {i}")).ToArray());

            var obsStack = new ObservableStack(_stackElementsList, debounce: true);

            _content = SectionStack()
               .Title(SampleHeader(nameof(ObservableStackSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("ObservableStack is a specialized container that synchronizes its DOM with an observable list using an efficient reconciliation process."),
                    TextBlock("Instead of re-rendering the entire list when a change occurs, it identifies which elements were added, removed, or moved by comparing their unique Identifiers and ContentHashes. This makes it ideal for high-performance lists where preserving scroll position or component state is important.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use ObservableStack when your list data changes frequently or when you want smooth transitions for moved items. Ensure each item implements 'IComponentWithID' correctly, providing a stable 'Identifier' and a 'ContentHash' that reflects any changes in the item's data. Avoid frequent full-list replacements if only a few items have changed. Leverage the reconciliation behavior to keep the DOM footprint minimal and performance high.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Interactive Reconciliation Demo"),
                    TextBlock("Modify the list below and watch how the 'Display Elements' on the right update efficiently."),
                    SplitView().Height(500.px()).WS()
                       .Left(VStack().Children(
                            HStack().Children(
                                Button("Randomize Order").OnClick(() => _stackElementsList.ReplaceAll(_stackElementsList.Value.OrderBy(_ => Math.Random()).ToList())),
                                Button("Add New Item").Primary().OnClick(() => AddItem())
                            ).MB(16),
                            Label("Edit items in-place:").SemiBold(),
                            DeferSync(_stackElementsList, elements =>
                            {
                                var list = VStack();
                                for (int i = 0; i < elements.Count; i++)
                                {
                                    var idx = i;
                                    var item = elements[i] as StackElement;
                                    list.Add(HStack().AlignItemsCenter().Children(
                                        Button().SetIcon(UIcons.ArrowUp).OnClick(() => Move(idx, idx - 1)),
                                        Button().SetIcon(UIcons.ArrowDown).OnClick(() => Move(idx, idx + 1)),
                                        TextBox(item.DisplayName).OnBlur((tb, _) => { item.DisplayName = tb.Text; Update(idx, item); }).Background(item.Color).WS()
                                    ).MB(4));
                                }
                                return list.ScrollY();
                            }).Grow()
                       ).P(8))
                       .Right(VStack().Children(
                            Label("Rendered Stack:").SemiBold(),
                            obsStack.H(450.px()).WS()
                       ).P(8))
                ));
        }

        private void Move(int oldIdx, int newIdx)
        {
            if (newIdx < 0 || newIdx >= _stackElementsList.Count) return;
            var list = _stackElementsList.Value.ToList();
            var item = list[oldIdx];
            list.RemoveAt(oldIdx);
            list.Insert(newIdx, item);
            _stackElementsList.ReplaceAll(list);
        }

        private void Update(int idx, StackElement item)
        {
            var list = _stackElementsList.Value.ToList();
            list[idx] = item;
            _stackElementsList.ReplaceAll(list);
        }

        private void AddItem()
        {
            _elementIndex++;
            var list = _stackElementsList.Value.ToList();
            list.Add(new StackElement(_elementIndex.ToString(), $"Item {_elementIndex}"));
            _stackElementsList.ReplaceAll(list);
        }

        public HTMLElement Render() => _content.Render();
    }

    public static class HashingHelper
    {
        public static int Fnv1aHash(string value)
        {
            var valArray = value.ToCharArray();
            var hash = 0x811c9dc5;
            for (var i = 0; i < valArray.Length; i++) { hash ^= (uint)valArray[i]; hash *= 0x01000193; }
            return (int)hash;
        }
    }
}
