using Tesserae.Components;

namespace Tesserae.Tests.Samples
{
    public class PickerSampleItem : IPickerItem
    {
        public PickerSampleItem(string name)
        {
            Name = name;
        }

        public string Name     { get; }

        public bool IsSelected { get; set; }
    }
}
