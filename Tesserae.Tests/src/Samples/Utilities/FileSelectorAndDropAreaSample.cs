using Tesserae.Tests;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 20, Icon = UIcons.Folder)]
    public class FileSelectorAndDropAreaSample : IComponent, ISample
    {
        private readonly IComponent _content;
        public FileSelectorAndDropAreaSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(FileSelectorAndDropAreaSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("FileSelector and FileDropArea provide two different ways for users to upload files. FileSelector uses a standard button that opens the system file dialog, while FileDropArea provides a larger target area for users to drag and drop files directly into the application.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use FileSelector for simple, single-file selections in forms. Use FileDropArea when users are likely to be uploading multiple files or when a more prominent upload target is desired. Always specify the allowed file types using the 'Accepts' property. Provide immediate feedback after files are selected or dropped, such as displaying the file names or sizes.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("File Selector"),
                    Label("Selected file size: ").Inline().SetContent(TextBlock("").Var(out var size)),
                    FileSelector().OnFileSelected((fs,                                                                            e) => size.Text = fs.SelectedFile.size.ToString() + " bytes"),
                    FileSelector().SetPlaceholder("You must select a zip file").Required().SetAccepts(".zip").OnFileSelected((fs, e) => size.Text = fs.SelectedFile.size.ToString() + " bytes"),
                    FileSelector().SetPlaceholder("Please select any image").SetAccepts("image/*").OnFileSelected((fs,            e) => size.Text = fs.SelectedFile.size.ToString() + " bytes"),
                    SampleSubTitle("File Drop Area"),
                    Label("Dropped Files: ").SetContent(Stack().Var(out var droppedFiles)),
                    FileDropArea().OnFilesDropped((s, e) =>
                    {
                        foreach (var file in e)
                        {
                            droppedFiles.Add(TextBlock(file.name).Small());
                        }
                    }).Multiple()
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}