using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 20, Icon = UIcons.BreadSlice)]
    public class ToastSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ToastSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(ToastSample)))
               .Section(Stack().WidthStretch().Children(
                    SampleTitle("Overview"),
                    TextBlock("Toasts are used for short-lived notifications to users.")))
               .Section(Stack().WidthStretch().Children(
                    SampleTitle("Best Practices"),
                    SplitView().SplitInMiddle().Left(
                            Stack().WidthStretch().Children(
                                SampleSubTitle("Do"),
                                SampleDo("Write short and recognizable messages"),
                                SampleDo("Keep toasts long enough to be read, but not long enough to bother")))
                       .Right(Stack().WidthStretch().Children(
                            SampleSubTitle("Don't"),
                            SampleDont("Overload users with toasts.")))))
               .Section(
                    Stack().WidthStretch().Children(
                        SampleTitle("Usage"),
                        SampleSubTitle("Toasts top-right (default)"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().Information("Info!")),
                            Button().SetText("Success").OnClick(() => Toast().Success("Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().Warning("Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().Error("Error!"))),
                        SampleSubTitle("Toasts top left"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().TopLeft().Information("Info!")),
                            Button().SetText("Success").OnClick(() => Toast().TopLeft().Success("Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().TopLeft().Warning("Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().TopLeft().Error("Error!"))),
                        SampleSubTitle("Toasts bottom right"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().BottomRight().Information("Info!")),
                            Button().SetText("Success").OnClick(() => Toast().BottomRight().Success("Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().BottomRight().Warning("Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().BottomRight().Error("Error!"))),
                        SampleSubTitle("Toasts bottom left"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().BottomLeft().Information("Info!")),
                            Button().SetText("Success").OnClick(() => Toast().BottomLeft().Success("Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().BottomLeft().Warning("Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().BottomLeft().Error("Error!"))),
                        SampleSubTitle("Toasts top center with title"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().TopCenter().Information("This is a title", "Info!")),
                            Button().SetText("Success").OnClick(() => Toast().TopCenter().Success("This is a title", "Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().TopCenter().Warning("This is a title", "Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().TopCenter().Error("This is a title", "Error!"))),
                        SampleSubTitle("Toasts top full with title"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().TopFull().Information("This is a title", "Info!")),
                            Button().SetText("Success").OnClick(() => Toast().TopFull().Success("This is a title", "Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().TopFull().Warning("This is a title", "Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().TopFull().Error("This is a title", "Error!"))),
                        SampleSubTitle("Toasts bottom center with title"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().BottomCenter().Information("This is a title", "Info!")),
                            Button().SetText("Success").OnClick(() => Toast().BottomCenter().Success("This is a title", "Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().BottomCenter().Warning("This is a title", "Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().BottomCenter().Error("This is a title", "Error!"))),
                        SampleSubTitle("Toasts bottom full with title"),
                        HStack().Children(
                            Button().SetText("Info").OnClick(() => Toast().BottomFull().Information("This is a title", "Info!")),
                            Button().SetText("Success").OnClick(() => Toast().BottomFull().Success("This is a title", "Success!")),
                            Button().SetText("Warning").OnClick(() => Toast().BottomFull().Warning("This is a title", "Warning!")),
                            Button().SetText("Error").OnClick(() => Toast().BottomFull().Error("This is a title", "Error!"))),
                        SampleSubTitle("Toast as banner"),
                        HStack().Children(
                            Button().SetText("Info on top").OnClick(() => Toast().TopFull().Banner().Information("This is a banner", "Info!")),
                            Button().SetText("Success on top").OnClick(() => Toast().TopFull().Banner().Success("This is a banner", "Success!")),
                            Button().SetText("Warning on bottom").OnClick(() => Toast().BottomFull().Banner().Warning("This is a banner", "Warning!")),
                            Button().SetText("Error on bottom").OnClick(() => Toast().BottomFull().Banner().Error("This is a banner", "Error!")))
                    ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}