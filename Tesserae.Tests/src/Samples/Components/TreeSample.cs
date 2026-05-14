using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 100, Icon = UIcons.FolderTree)]
    public class TreeSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TreeSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(TreeSample), UIcons.Sitemap, "A component that displays a hierarchical list")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("A tree displays hierarchical data. Nodes can be expanded or collapsed to reveal nested data."),
                    TextBlock("Supports synchronous and asynchronous loading of child nodes."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic Synchronous Tree"),
                    new Tree().Items(
                        new Tree.Item("samples/ConsoleApp...", UIcons.Folder.ToString()).Expanded().Items(
                            new Tree.Item("ConsoleApp1.csproj", UIcons.File.ToString()).Selected(),
                            new Tree.Item("Program.cs", UIcons.File.ToString())
                        ),
                        new Tree.Item("src", UIcons.Folder.ToString()).Expanded().Items(
                            new Tree.Item("MarkdownRende...", UIcons.Folder.ToString()).Expanded().Items(
                                new Tree.Item("MarkdownConve...", UIcons.File.ToString()),
                                new Tree.Item("Slides", UIcons.Folder.ToString()).Expanded().Items(
                                    new Tree.Item("Blocks", UIcons.Folder.ToString()).Expanded().Items(
                                        new Tree.Item("HeadingRe...", UIcons.File.ToString()),
                                        new Tree.Item("HeadingRe...", UIcons.File.ToString())
                                    ),
                                    new Tree.Item("SlideDocume...", UIcons.File.ToString())
                                )
                            ),
                            new Tree.Item("MarkdownRende...", UIcons.Folder.ToString()).Expanded().Items(
                                new Tree.Item("MarkdownRende...", UIcons.File.ToString()),
                                new Tree.Item("Program.cs", UIcons.File.ToString())
                            ),
                            new Tree.Item("MarkdownRenderer...", UIcons.File.ToString())
                        )
                    ),
                    SampleSubTitle("Asynchronous Tree"),
                    new Tree().Items(
                        new Tree.Item("Lazy Loaded Folder", UIcons.Folder.ToString()).ItemsAsync(async () =>
                        {
                            await Task.Delay(1000);
                            return new[]
                            {
                                new Tree.Item("Async Child 1", UIcons.File.ToString()),
                                new Tree.Item("Async Child 2", UIcons.File.ToString())
                            };
                        })
                    ),
                    SampleSubTitle("Selectable Tree"),
                    new Tree().SelectionEnabled().Items(
                        new Tree.Item("Root 1", UIcons.Folder.ToString()).Expanded().Items(
                            new Tree.Item("Child A", UIcons.File.ToString()),
                            new Tree.Item("Child B", UIcons.File.ToString())
                        ),
                        new Tree.Item("Root 2", UIcons.Folder.ToString()).Expanded().Items(
                            new Tree.Item("Child C", UIcons.File.ToString()).Selected(),
                            new Tree.Item("Child D", UIcons.File.ToString())
                        )
                    ),
                    SampleSubTitle("Tree with Commands and Context Menu"),
                    new Tree().Items(
                        new Tree.Item("src", UIcons.Folder.ToString(),
                            new TreeCommand(UIcons.Plus).Tooltip("Add file").OnClick(() => window.alert("Add file clicked")),
                            new TreeCommand(UIcons.Refresh).Tooltip("Refresh").OnClick(() => window.alert("Refresh clicked"))
                        ).Expanded().Items(
                            new Tree.Item("Program.cs", UIcons.File.ToString(),
                                new TreeCommand(UIcons.Pencil).Tooltip("Rename").OnClick(() => window.alert("Rename Program.cs")),
                                new TreeCommand(UIcons.Trash).Tooltip("Delete").OnClick(() => window.alert("Delete Program.cs"))
                            ),
                            new Tree.Item("README.md", UIcons.File.ToString(),
                                new TreeCommand(UIcons.MenuDots).Tooltip("Context menu").HookToParentContextMenu().OnClick(() => window.alert("README.md context action (right-click or button)"))
                            ),
                            new Tree.Item("notes.txt", UIcons.File.ToString()).OnContextMenu((s, e) =>
                            {
                                e.preventDefault();
                                window.alert("Right-clicked notes.txt");
                            }),
                            new Tree.Item("config.json", UIcons.File.ToString(),
                                new TreeCommand(UIcons.MenuBurger).Tooltip("More actions").OnClickMenu(() => new[]
                                {
                                    new TreeCommand(UIcons.Pencil).SetText("Rename").OnClick(() => window.alert("Rename config.json")),
                                    new TreeCommand(UIcons.Copy).SetText("Duplicate").OnClick(() => window.alert("Duplicate config.json")),
                                    new TreeCommand(UIcons.Trash).SetText("Delete").Danger().OnClick(() => window.alert("Delete config.json"))
                                })
                            )
                        )
                    )
               )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}