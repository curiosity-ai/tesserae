using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Inputs, Order = 180, Icon = UIcons.Password, Description = "A one-time code entered into segmented cells")]
    public class CodeInputSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public CodeInputSample()
        {
            var result = TextBlock("Type or paste a code. The right one is 2468.").Secondary();
            var code   = CodeInput(4);

            code.OnCompleted(value =>
            {
                if (value == "2468")
                {
                    result.Text = "Verified.";
                    result.Success();
                }
                else
                {
                    result.Text = "That code is not right. Try 2468.";
                    result.Danger();
                    code.Invalid();
                }
            });

            var bound = CodeInput(6);
            var echo  = TextBlock("").Secondary();
            bound.AsObservable().Observe(v => echo.Text = string.IsNullOrEmpty(v) ? "Value: (empty)" : "Value: " + v);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(CodeInputSample), UIcons.Password, "A one-time code, one cell per character")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("CodeInput draws a verification code as a row of cells. Each cell fills with a small pop as it is typed, the next cell carries the caret, and OnCompleted fires when the last is filled. Invalid() shakes the row and outlines it for a refused code; typing again clears it."),
                        TextBlock("Underneath there is one ordinary <input> laid over the cells, so pasting a whole code, the browser's and the phone's one-time-code autofill, Backspace and screen readers all behave as in a plain field. Numeric() (the default) accepts digits only; Numeric(false) accepts letters too.").PT(8))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Verify a code"),
                        code,
                        result.PT(8),
                        SampleSubTitle("Six characters, letters allowed, bound"),
                        bound.Numeric(false),
                        echo.PT(8),
                        SampleSubTitle("Masked"),
                        CodeInput(4).Masked(),
                        SampleSubTitle("Disabled"),
                        CodeInput(4).Disabled())).SetTitle("Usage")))
               .SeeAlso(typeof(TextBoxSample), typeof(ValidatorSample), typeof(MicroInteractionsSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
