using System;
using System.Linq;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Lists, Order = 70, Icon = UIcons.FolderTree, Description = "An expandable hierarchical list")]
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
                        new Tree.Item("samples/ConsoleApp...", UIcons.Folder).Expanded().Items(
                            new Tree.Item("ConsoleApp1.csproj", UIcons.File).Selected(),
                            new Tree.Item("Program.cs", UIcons.File)
                        ),
                        new Tree.Item("src", UIcons.Folder).Expanded().Items(
                            new Tree.Item("MarkdownRende...", UIcons.Folder).Expanded().Items(
                                new Tree.Item("MarkdownConve...", UIcons.File),
                                new Tree.Item("Slides", UIcons.Folder).Expanded().Items(
                                    new Tree.Item("Blocks", UIcons.Folder).Expanded().Items(
                                        new Tree.Item("HeadingRe...", UIcons.File),
                                        new Tree.Item("HeadingRe...", UIcons.File)
                                    ),
                                    new Tree.Item("SlideDocume...", UIcons.File)
                                )
                            ),
                            new Tree.Item("MarkdownRende...", UIcons.Folder).Expanded().Items(
                                new Tree.Item("MarkdownRende...", UIcons.File),
                                new Tree.Item("Program.cs", UIcons.File)
                            ),
                            new Tree.Item("MarkdownRenderer...", UIcons.File)
                        )
                    ),
                    SampleSubTitle("Compact Tree"),
                    new Tree().Compact().Items(
                        new Tree.Item("skills", UIcons.Folder).Expanded().Items(
                            new Tree.Item("docx", UIcons.Folder).Expanded().Items(
                                new Tree.Item("examples", UIcons.Folder),
                                new Tree.Item("ooxml", UIcons.Folder).Expanded().Items(
                                    new Tree.Item("comments.md", UIcons.File),
                                    new Tree.Item("hyperlinks_and_fields.md", UIcons.File),
                                    new Tree.Item("rels_and_content_types.md", UIcons.File),
                                    new Tree.Item("tracked_changes.md", UIcons.File)
                                )
                            ),
                            new Tree.Item("pptx", UIcons.Folder)
                        )
                    ),
                    SampleSubTitle("Asynchronous Tree"),
                    new Tree().Items(
                        new Tree.Item("Lazy Loaded Folder", UIcons.Folder).ItemsAsync(async () =>
                        {
                            await Task.Delay(1000);
                            return new[]
                            {
                                new Tree.Item("Async Child 1", UIcons.File),
                                new Tree.Item("Async Child 2", UIcons.File)
                            };
                        })
                    ),
                    SampleSubTitle("Selectable Tree"),
                    new Tree().SelectionEnabled().Items(
                        new Tree.Item("Root 1", UIcons.Folder).Expanded().Items(
                            new Tree.Item("Child A", UIcons.File),
                            new Tree.Item("Child B", UIcons.File)
                        ),
                        new Tree.Item("Root 2", UIcons.Folder).Expanded().Items(
                            new Tree.Item("Child C", UIcons.File).Selected(),
                            new Tree.Item("Child D", UIcons.File)
                        )
                    ),
                    SampleSubTitle("Multiple Selection, Cascading Into Folders"),
                    TextBlock("Click a checkbox to pick one row, ctrl (or cmd) click a row to do the same, and shift-click one to pick everything up to it. Selecting a folder selects everything inside it; a folder only part of which is picked is drawn half-selected. The read-only file cannot be picked at all.").Small().Secondary(),
                    MultipleSelectionTree(out var selectionLabel),
                    selectionLabel,
                    SampleSubTitle("Tree with Commands and Context Menu"),
                    new Tree().Items(
                        new Tree.Item("src", UIcons.Folder,
                            new TreeCommand(UIcons.Plus).Tooltip("Add file").OnClick(() => window.alert("Add file clicked")),
                            new TreeCommand(UIcons.Refresh).Tooltip("Refresh").OnClick(() => window.alert("Refresh clicked"))
                        ).Expanded().Items(
                            new Tree.Item("Program.cs", UIcons.File,
                                new TreeCommand(UIcons.Pencil).Tooltip("Rename").OnClick(() => window.alert("Rename Program.cs")),
                                new TreeCommand(UIcons.Trash).Tooltip("Delete").OnClick(() => window.alert("Delete Program.cs"))
                            ),
                            new Tree.Item("README.md", UIcons.File,
                                new TreeCommand(UIcons.MenuDots).Tooltip("Context menu").HookToParentContextMenu().OnClick(() => window.alert("README.md context action (right-click or button)"))
                            ),
                            new Tree.Item("notes.txt", UIcons.File).OnContextMenu((s, e) =>
                            {
                                e.preventDefault();
                                window.alert("Right-clicked notes.txt");
                            }),
                            new Tree.Item("config.json", UIcons.File,
                                new TreeCommand(UIcons.MenuBurger).Tooltip("More actions").OnClickMenu(() => new[]
                                {
                                    new TreeCommand(UIcons.Pencil).SetText("Rename").OnClick(() => window.alert("Rename config.json")),
                                    new TreeCommand(UIcons.Copy).SetText("Duplicate").OnClick(() => window.alert("Duplicate config.json")),
                                    new TreeCommand(UIcons.Trash).SetText("Delete").Danger().OnClick(() => window.alert("Delete config.json"))
                                })
                            )
                        )
                    )
               )).SetTitle("Usage")))
               .SeeAlso(typeof(DetailsListSample), typeof(AccordionSample), typeof(PlanSample), typeof(NodeViewSample), typeof(SearchableGroupedListSample));
        }

        private static IComponent MultipleSelectionTree(out TextBlock selectionLabel)
        {
            var label = TextBlock("Nothing selected").Small().Secondary();

            var tree = new Tree().Compact().Selectable(TreeSelectionMode.Multiple).CascadeSelection().Items(
                new Tree.Item("config", UIcons.Folder).Expanded().Items(
                    new Tree.Item("endpoints", UIcons.Folder).Expanded().Items(
                        new Tree.Item("search.cs",   UIcons.File),
                        new Tree.Item("upload.cs",   UIcons.File),
                        new Tree.Item("webhook.cs",  UIcons.File)
                    ),
                    new Tree.Item("indexes", UIcons.Folder).Expanded().Items(
                        new Tree.Item("people.json",    UIcons.File),
                        new Tree.Item("documents.json", UIcons.File)
                    ),
                    new Tree.Item("workspace.json", UIcons.File).Selectable(false)
                )
            );

            tree.OnSelectionChanged((_, items) =>
            {
                var files = items.Where(i => !i.HasChildren).Select(i => i.Text).ToArray();

                label.Text = files.Length == 0 ? "Nothing selected" : files.Length + " selected: " + string.Join(", ", files);
            });

            selectionLabel = label;

            return tree;
        }

        public HTMLElement Render() => _content.Render();
    }
}