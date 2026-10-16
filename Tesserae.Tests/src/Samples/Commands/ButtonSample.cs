using System;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Commands, Order = 10, Icon = UIcons.Cursor, Description = "The control that triggers an action")]
    public class ButtonSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ButtonSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ButtonSample), UIcons.Cursor, "A control that triggers an action")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Buttons are best used to enable a user to commit a change or complete steps in a task. They are typically found inside forms, dialogs, panels or pages. An example of their usage is confirming the deletion of a file in a confirmation dialog."),
                    TextBlock("When considering their place in a layout, contemplate the order in which a user will flow through the UI. As an example, in a form, the individual will need to read and interact with the form fields before submiting the form. Therefore, as a general rule, the button should be placed at the bottom of the UI container (a dialog, panel, or page) which holds the related UI elements."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Buttons should clearly communicate what will happen when the user clicks them. Use concise, specific, self-explanatory labels, usually a single word. Default buttons should always perform safe operations. For example, a default button should never delete. Use only a single line of text in the label of the button. Expose only one or two buttons to the user at a time. Show only one primary button that inherits theme color at rest state. \"Submit\", \"OK\", and \"Apply\" buttons should always be styled as primary buttons."),
                    TextBlock("Avoid using generic labels like \"Ok\", especially in the case of an error. Do not place the default focus on a button that destroys data. Do not use a button to navigate to another place, use a link instead."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic Buttons"),
                    HStack().Children(
                        Button().SetText("Standard").Tooltip("This is a standard button").OnClick(() => alert("Clicked!")),
                        Button().SetText("Primary").Tooltip("This is a primary button").Primary().OnClick(() => alert("Clicked!")),
                        Button("Open the repository", href: "https://github.com/curiosity-ai/tesserae").Tooltip("An href makes it an anchor - middle-clickable, and it shows where it goes"),
                        Button("Link with a handler", href: "https://github.com/curiosity-ai/tesserae").OnClick(() => Toast().Information("The handler ran instead of following the link")).Tooltip("A plain click runs the handler; Ctrl/Cmd-click opens the href in a new tab and Shift-click in a new window")
                    ),
                    SampleSubTitle("Icons and States"),
                    HStack().Children(
                        Button().SetText("Confirm").SetIcon(UIcons.Check).Success().OnClick(() => alert("Clicked!")),
                        Button().SetText("Delete").SetIcon(UIcons.Trash).Danger().OnClick(() => alert("Clicked!")),
                        Button().SetText("Disabled").SetIcon(UIcons.Lock).Disabled()
                    ),
                    SampleSubTitle("Loading States"),
                    TextBlock("OnClickSpinWhile spins the button in place while the action runs: its content fades, a dot appears on a ring where it was, orbits and stretches into the arc, and when the action ends it collapses back to the dot as the content returns. The button keeps its box and its tone, and its tooltip is off while it works. A spinner text sits beside the ring and is ellipsized if it does not fit: the button never changes size, so nothing around it shifts.").MB(8),
                    HStack().AlignItemsCenter().Children(
                        Button().SetText("Click to Spin").OnClickSpinWhile(async () => await Task.Delay(2500)),
                        Button().SetText("With Loading Text").OnClickSpinWhile(async () => await Task.Delay(2500), "Processing..."),
                        Button().SetText("Sync").OnClickSpinWhile(async () => await Task.Delay(2500), "Synchronising every workspace..."),
                        Button().SetText("Save").SetIcon(UIcons.Disk).Primary().OnClickSpinWhile(async () => await Task.Delay(2500)),
                        Button().SetText("Error Simulation").OnClickSpinWhile(async () => { await Task.Delay(1000); throw new Exception("Action failed"); }, onError: (b, e) => b.SetText("Try again: " + e.Message).Danger())
                    ),
                    SampleSubTitle("Icon-Only Loading States"),
                    TextBlock("The same spinner on an icon-only button, as a row of download actions in a header uses it.").MB(8),
                    HStack().AlignItemsCenter().Children(
                        Button().SetIcon(UIcons.Download).Tooltip("Download").OnClickSpinWhile(async () => await Task.Delay(2500)),
                        Button().SetIcon(UIcons.FileCode).Tooltip("Download as Markdown").OnClickSpinWhile(async () => await Task.Delay(2500)),
                        Button().SetIcon(UIcons.FilePdf).Tooltip("Download as PDF").OnClickSpinWhile(async () => await Task.Delay(2500)),
                        Button().SetIcon(UIcons.FilePdf).Tooltip("Download as PDF (quick)").OnClickSpinWhile(async () => await Task.Delay(400)),
                        Button().SetIcon(UIcons.FilePdf).Tooltip("Download as PDF (primary)").Primary().OnClickSpinWhile(async () => await Task.Delay(2500)),
                        Button().SetIcon(UIcons.FilePdf).Tooltip("Download as PDF (no border)").NoBorder().OnClickSpinWhile(async () => await Task.Delay(2500))
                    ),
                    SampleSubTitle("Loading States With an Outcome"),
                    TextBlock("When the action returns a ButtonSpinnerOutcome the spinner ends on it: the arc closes into a full circle, the track drops away and a check (Success) or an x (Error) draws in, is held for a moment, then fades as the content returns. An action that throws ends on the x. A ghost button draws the mark in green or red; a filled button keeps its text colour for the mark and turns its fill green or red instead. ToSpinner() and UndoSpinner(outcome) do the same by hand.").MB(8),
                    HStack().AlignItemsCenter().Children(
                        Button().SetIcon(UIcons.FilePdf).Tooltip("Download as PDF (succeeds)").OnClickSpinWhile(() => SucceedAfter(2500)),
                        Button().SetIcon(UIcons.FilePdf).Tooltip("Download as PDF (fails)").OnClickSpinWhile(() => FailAfter(2500), onError: (b, e) => Toast().Error("Download failed", e.Message)),
                        Button().SetIcon(UIcons.CloudUpload).Tooltip("Upload (succeeds quickly)").OnClickSpinWhile(() => SucceedAfter(500)),
                        Button().SetIcon(UIcons.FilePdf).Tooltip("Download as PDF (primary, succeeds)").Primary().OnClickSpinWhile(() => SucceedAfter(2500)),
                        Button().SetIcon(UIcons.FilePdf).Tooltip("Download as PDF (primary, fails)").Primary().OnClickSpinWhile(() => FailAfter(2500), onError: (b, e) => Toast().Error("Download failed", e.Message))
                    ),
                    HStack().AlignItemsCenter().MT(8).Children(
                        Button().SetText("Publish").OnClickSpinWhile(() => SucceedAfter(2500)),
                        Button().SetText("Publish").Primary().OnClickSpinWhile(() => SucceedAfter(2500), "Publishing..."),
                        Button().SetText("Validate").OnClickSpinWhile(async () => { await Task.Delay(2500); return ButtonSpinnerOutcome.Error; }),
                        Button().SetText("Delete").SetIcon(UIcons.Trash).Danger().OnClickSpinWhile(() => FailAfter(2500), onError: (b, e) => Toast().Error("Delete failed", e.Message)),
                        Button().SetText("By hand").Var(out var byHand).OnClick(() =>
                        {
                            byHand.ToSpinner();
                            window.setTimeout(_ => byHand.UndoSpinner(ButtonSpinnerOutcome.Success), 2500);
                        })
                    ),
                    SampleSubTitle("Variations"),
                    HStack().Children(
                        Button().SetText("No Padding").NoPadding().Primary(),
                        Button().SetText("No Border").NoBorder()
                    ),
                    SampleSubTitle("Split Buttons"),
                    TextBlock("ButtonAndIcon splits one button into two click targets: the main half runs the most common action, the arrow half a complementary one (often a menu of alternatives). The tone and the disabled state apply to both halves.").MB(8),
                    HStack().Gap(8.px()).MB(8).Children(
                        ButtonAndIcon("Standard Action", (m, i, ev) => Toast().Information("Clicked action icon!"), mainIcon: UIcons.Play).OnClick((b, _) => Toast().Information("Clicked display!")),
                        ButtonAndIcon("Primary with Calendar Icon", (m, i, ev) => Toast().Success("Calendar icon clicked"), mainIcon: UIcons.Clock, secondaryIcon: UIcons.Calendar).Primary().OnClick((b, _) => Toast().Success("Main area clicked")),
                        ButtonAndIcon("Danger Action", (m, i, ev) => Toast().Error("Warning icon clicked"), mainIcon: UIcons.TriangleWarning).Danger().OnClick((b, _) => Toast().Error("Danger area clicked")),
                        ButtonAndIcon("Disabled", (m, i, ev) => Toast().Error("Should not fire"), mainIcon: UIcons.Lock).Disabled()
                    ),
                    HStack().Gap(8.px()).Children(
                        ButtonAndIcon("Move Item", (m, i, ev) => Toast().Information("Configure move..."), secondaryIcon: UIcons.Settings)
                           .ReplaceContent(VStack().Children(
                                HStack().AlignItemsCenter().Children(Icon(UIcons.Arrows), TextBlock("Move Item").SemiBold().PL(8)),
                                TextBlock("Use this to reorganize your workspace").Tiny().PT(4)))
                           .OnClick((b, _) => Toast().Information("Moving...")),
                        ButtonAndIcon("Action with Custom Tooltip", (m, i, ev) => Toast().Information("Arrow clicked"), mainIcon: UIcons.CommentInfo)
                           .Tooltip("This tooltip is applied to the entire component"),
                        ButtonAndIcon("Show Options", (m, i, ev) =>
                        {
                            Action hideMenu = null;
                            var menu = VStack().Children(
                                Button("Option 1").OnClick(() => { Toast().Information("Option 1"); hideMenu?.Invoke(); }),
                                Button("Option 2").OnClick(() => { Toast().Information("Option 2"); hideMenu?.Invoke(); }),
                                Button("Option 3").OnClick(() => { Toast().Information("Option 3"); hideMenu?.Invoke(); })
                            );
                            Tippy.ShowFor(i, menu, out hideMenu, TooltipAnimation.None, TooltipPlacement.BottomEnd, 0, 0, 350, true, null);
                        }, mainIcon: UIcons.List).Primary().OnClick((b, _) => Toast().Information("Default option"))
                    ),
                    SampleSubTitle("Buttons That Read As Links"),
                    TextBlock("Link() drops the border and the background so the button reads as text rather than as a box. A tone still applies, as the label's colour rather than as a filled surface - which is how a destructive action that should not be the thing the eye lands on is written. Only a button built with an href underlines on hover: one that merely runs a handler is a button, and an underline would promise an address it doesn't have.").MB(8),
                    HStack().Children(
                        Button("Open the repository", href: "https://github.com/curiosity-ai/tesserae").Link(),
                        Button("Forget this device").Link().OnClick(() => Toast().Information("A neutral link")),
                        Button("Make private").Link().Danger().OnClick(() => Toast().Warning("A danger link - destructive, but quieter than a filled red button")),
                        Button("Make private").Link().Danger().LinkOnHover().OnClick(() => Toast().Warning("The same, underlined on hover")),
                        Button("Make private").Link().Danger().Disabled()
                    ),
                    SampleSubTitle("Themed Backgrounds"),
                    HStack().Children(
                        Button().SetText("Blue").Background(Theme.Colors.Blue500).OnClick(() => alert("Clicked!")),
                        Button().SetText("Lime").Background(Theme.Colors.Lime500).OnClick(() => alert("Clicked!")),
                        Button().SetText("Magenta").Background(Theme.Colors.Magenta500).OnClick(() => alert("Clicked!")),
                        Button().SetText("Yellow").Background(Theme.Colors.Yellow500).OnClick(() => alert("Clicked!"))
                    ),
                    SampleSubTitle("AI Actions"),
                    TextBlock("AI() styles a button as an action that asks a model for something: a filled purple-to-blue gradient, and the Sparkles glyph unless the button already carries an icon. It is the AI equivalent of Primary(), so keep one per surface - the second and third AI action beside it want AISubtle(), which is the same hue as a tint with the accent's own text.").MB(8),
                    HStack().Children(
                        Button("Summarise").AI().OnClick(() => Toast().Information("The filled form: one per surface")),
                        Button("Rewrite").AISubtle().OnClick(() => Toast().Information("The quiet form")),
                        Button("Explain").AISubtle(withSparklesIcon: false).SetIcon(UIcons.CommentQuestion).OnClick(() => Toast().Information("A glyph of its own keeps its place")),
                        Button().AI().Tooltip("Icon only"),
                        Button("Generating").AI().Disabled()
                    ),
                    SampleSubTitle("Rounded Buttons"),
                    HStack().Children(
                        Button().SetText("Small").Rounded(BorderRadius.Small).Primary(),
                        Button().SetText("Medium").Rounded(BorderRadius.Medium).Primary(),
                        Button().SetText("Full").Rounded(BorderRadius.Full).Primary()
                    )
                )).SetTitle("Usage")))
               .SeeAlso(typeof(SaveButtonSample), typeof(CommandBarSample), typeof(ToggleSample), typeof(DropdownSample), typeof(AIVariantsSample));
        }

        private static async Task<ButtonSpinnerOutcome> SucceedAfter(int ms)
        {
            await Task.Delay(ms);
            return ButtonSpinnerOutcome.Success;
        }

        private static async Task<ButtonSpinnerOutcome> FailAfter(int ms)
        {
            await Task.Delay(ms);
            throw new Exception("Rendering failed");
        }

        public HTMLElement Render() => _content.Render();
    }
}
