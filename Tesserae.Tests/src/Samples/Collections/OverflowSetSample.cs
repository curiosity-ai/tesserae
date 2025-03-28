﻿using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 10, Icon = UIcons.MenuDots)]
    public class OverflowSetSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public OverflowSetSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(OverflowSetSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Breadcrumbs should be used as a navigational aid in your app or site. They indicate the current page’s location within a hierarchy and help the user understand where they are in relation to the rest of that hierarchy. They also afford one-click access to higher levels of that hierarchy."),
                    TextBlock("Breadcrumbs are typically placed, in horizontal form, under the masthead or navigation of an experience, above the primary content area.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    HStack().Children(
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Do"),
                            SampleDo("Place Breadcrumbs at the top of a page, above a list of items, or above the main content of a page.")
                        ),
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Don't"),
                            SampleDont("Don't use Breadcrumbs as a primary way to navigate an app or site.")))))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Label("Selected: ").SetContent(TextBlock().Var(out var msg)),
                    TextBlock("All Visible").Medium(),
                    OverflowSet().PaddingTop(16.px()).PB(16).Items(
                        Button("Folder 1").Link().OnClick((s, e) => msg.Text("Folder 1")),
                        Button("Folder 2").Link().OnClick((s, e) => msg.Text("Folder 2")).Disabled(),
                        Button("Folder 3").Link().OnClick((s, e) => msg.Text("Folder 3")),
                        Button("Folder 4").Link().OnClick((s, e) => msg.Text("Folder 4")),
                        Button("Folder 5").Link().OnClick((s, e) => msg.Text("Folder 5")),
                        Button("Folder 6").Link().OnClick((s, e) => msg.Text("Folder 6"))),
                    TextBlock("All Visible, Small").Medium(),
                    OverflowSet().Small().PaddingTop(16.px()).PB(16).Items(
                        Button("Folder 1").Link().OnClick((s, e) => msg.Text("Folder 1")),
                        Button("Folder 2").Link().OnClick((s, e) => msg.Text("Folder 2")).Disabled(),
                        Button("Folder 3").Link().OnClick((s, e) => msg.Text("Folder 3")),
                        Button("Folder 4").Link().OnClick((s, e) => msg.Text("Folder 4")),
                        Button("Folder 5").Link().OnClick((s, e) => msg.Text("Folder 5")),
                        Button("Folder 6").Link().OnClick((s, e) => msg.Text("Folder 6"))),
                    TextBlock("Collapse 200px").Medium(),
                    OverflowSet().PaddingTop(16.px()).PB(16).MaxWidth(200.px()).Items(
                        Button("Folder 1").Link().SetIcon(UIcons.Acorn).OnClick((s,   e) => msg.Text("Folder 1")),
                        Button("Folder 2").Link().SetIcon(UIcons.Corn).OnClick((s,    e) => msg.Text("Folder 2")),
                        Button("Folder 3").Link().SetIcon(UIcons.Bacon).OnClick((s,   e) => msg.Text("Folder 3")),
                        Button("Folder 4").Link().SetIcon(UIcons.Taco).OnClick((s,    e) => msg.Text("Folder 4")),
                        Button("Folder 5").Link().SetIcon(UIcons.Pie).OnClick((s,     e) => msg.Text("Folder 5")),
                        Button("Folder 6").Link().SetIcon(UIcons.Popcorn).OnClick((s, e) => msg.Text("Folder 6")))
                   ,
                    TextBlock("Collapse 200px, Small").Medium(),
                    OverflowSet().PaddingTop(16.px()).PB(16).Small().MaxWidth(200.px()).Items(
                        Button("Folder 1").Link().SetIcon(UIcons.Acorn).OnClick((s,   e) => msg.Text("Folder 1")),
                        Button("Folder 2").Link().SetIcon(UIcons.Corn).OnClick((s,    e) => msg.Text("Folder 2")),
                        Button("Folder 3").Link().SetIcon(UIcons.Bacon).OnClick((s,   e) => msg.Text("Folder 3")),
                        Button("Folder 4").Link().SetIcon(UIcons.Taco).OnClick((s,    e) => msg.Text("Folder 4")),
                        Button("Folder 5").Link().SetIcon(UIcons.Pie).OnClick((s,     e) => msg.Text("Folder 5")),
                        Button("Folder 6").Link().SetIcon(UIcons.Popcorn).OnClick((s, e) => msg.Text("Folder 6")))
                   ,
                    TextBlock("Collapse 300px").Medium(),
                    OverflowSet().PaddingTop(16.px()).PB(16).MaxWidth(300.px()).Items(
                        Button("Folder 1").Link().OnClick((s, e) => msg.Text("Folder 1")),
                        Button("Folder 2").Link().OnClick((s, e) => msg.Text("Folder 2")),
                        Button("Folder 3").Link().OnClick((s, e) => msg.Text("Folder 3")),
                        Button("Folder 4").Link().OnClick((s, e) => msg.Text("Folder 4")),
                        Button("Folder 5").Link().OnClick((s, e) => msg.Text("Folder 5")),
                        Button("Folder 6").Link().OnClick((s, e) => msg.Text("Folder 6"))),
                    TextBlock("Collapse 300px, from second, custom chevron").Medium(),
                    OverflowSet().PaddingTop(16.px()).PB(16).MaxWidth(300.px()).SetOverflowIndex(1).Items(
                        Button("Folder 1").Link().OnClick((s, e) => msg.Text("Folder 1")),
                        Button("Folder 2").Link().OnClick((s, e) => msg.Text("Folder 2")),
                        Button("Folder 3").Link().OnClick((s, e) => msg.Text("Folder 3")),
                        Button("Folder 4").Link().OnClick((s, e) => msg.Text("Folder 4")),
                        Button("Folder 5").Link().OnClick((s, e) => msg.Text("Folder 5")),
                        Button("Folder 6").Link().OnClick((s, e) => msg.Text("Folder 6")))
                ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}