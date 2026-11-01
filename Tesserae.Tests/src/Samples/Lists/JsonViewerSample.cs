using System.Text;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Lists, Order = 75, Icon = UIcons.BracketsCurly, Description = "A JSON document as a collapsible, coloured tree")]
    public class JsonViewerSample : IComponent, ISample
    {
        private const string ORDER = @"{
  ""id"": ""ord_4821"",
  ""status"": ""shipped"",
  ""total"": 1289.5,
  ""paid"": true,
  ""coupon"": null,
  ""customer"": { ""name"": ""Marie Lang"", ""email"": ""marie@example.com"", ""tags"": [""returning"", ""b2b""] },
  ""lines"": [
    { ""sku"": ""BRK-220"", ""title"": ""Brake caliper, front left"", ""quantity"": 2, ""price"": 349.0 },
    { ""sku"": ""SNS-014"", ""title"": ""Wheel speed sensor"", ""quantity"": 1, ""price"": 591.5 }
  ],
  ""shipping"": { ""carrier"": ""DHL"", ""tracking"": ""JJD0099887766"", ""address"": { ""city"": ""Munich"", ""country"": ""DE"" } },
  ""notes"": ""Deliver to the loading dock at the back of the building. Ring twice; if nobody answers, leave the parcel with the gatehouse, which is staffed around the clock and has signed for deliveries to this address before. Do not leave it at the front door, which faces a busy street and is not covered by the cameras.""
}";

        private readonly IComponent _content;

        public JsonViewerSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(JsonViewerSample), UIcons.BracketsCurly, "A JSON document as a collapsible, syntax-coloured tree")
               .FlatSection(VStack().WS().Children(Overview()))
               .FlatSection(VStack().WS().Children(Large()))
               .FlatSection(VStack().WS().Children(Partial()))
               .FlatSection(VStack().WS().Children(Invalid()))
               .SeeAlso(typeof(TreeSample), typeof(PropertyGridSample), typeof(DetailsGridSample));
        }

        private static Card FeatureCard(string title, string subTitle, string description, params IComponent[] content)
        {
            var stack = VStack().WS().Children(SampleSubTitle(subTitle), TextBlock(description).MB(8));

            foreach (var c in content)
            {
                stack.Add(c);
            }

            return Card(stack).SetTitle(title);
        }

        private IComponent Overview()
        {
            var viewer = JsonViewer(ORDER).RootLabel("order.json").ExpandToDepth(2).MaxStringLength(80);

            return FeatureCard("Overview", "Keys, strings, numbers, booleans and null, each in its own colour",
                "JsonViewer(json) draws a document as a tree. A collapsed object or array says how big it is; an open one shows its braces with a guide between them. Click a row, or use the arrow keys, to open and close it. Hovering a row shows a button that copies its value, and a key's tooltip is its path from the root. A long string is cut at MaxStringLength and shown whole when clicked.",
                HStack().PB(8).Children(
                    Button("Expand all").SetIcon(UIcons.Expand).OnClick(() => viewer.ExpandAll()),
                    Button("Collapse all").SetIcon(UIcons.Compress).OnClick(() => viewer.CollapseAll())),
                viewer.WS());
        }

        private IComponent Large()
        {
            var json = new StringBuilder("[");

            for (var i = 0; i < 2500; i++)
            {
                if (i > 0) json.Append(',');
                json.Append("{\"id\":").Append(i).Append(",\"name\":\"Sensor ").Append(i).Append("\",\"online\":").Append(i % 7 == 0 ? "false" : "true").Append(",\"reading\":").Append((i * 37 % 1000) / 10.0).Append('}');
            }

            json.Append(']');

            return FeatureCard("Large documents", "Children are built when opened, a batch at a time",
                "A container builds its rows the first time it is opened and shows its first MaxChildren entries (100 by default), with a row that shows the next batch. This array has 2,500 entries; nothing below the first hundred exists in the page until it is asked for.",
                JsonViewer(json.ToString()).RootLabel("sensors").MaxChildren(100).WS().MaxHeight(360.px()).Style(s => s.overflowY = "auto"));
        }

        private IComponent Partial()
        {
            return FeatureCard("Part of a document", "ShowMore(false): say what was left out, offer nothing",
                "For a viewer that shows part of something whose whole is available elsewhere - a preview with a download beside it - ShowMore(false) turns the last row into a count of what was left out. Copyable(false) drops the copy buttons.",
                JsonViewer("{\"items\": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20]}").ExpandToDepth(2).MaxChildren(5).ShowMore(false).Copyable(false).WS());
        }

        private IComponent Invalid()
        {
            return FeatureCard("Invalid JSON", "The parser's error, not a broken tree",
                "Text that does not parse is shown as the parser's message, so a caller can hand over whatever it received. Value(object) takes a value that is already parsed.",
                JsonViewer("{\"name\": \"unterminated").WS(),
                JsonViewer().Value(Transpose.Script.Write<object>("({ parsed: 'already', count: 3 })")).WS().MT(8));
        }

        public HTMLElement Render() => _content.Render();
    }
}
