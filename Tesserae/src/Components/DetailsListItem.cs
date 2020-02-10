using System.Diagnostics.Contracts;

namespace Tesserae.Components
{
    public class DetailsListItem
    {
        public DetailsListItem(string className, string iconName, string fileName, string modifiedBy, string fileSize)
        {
            ClassName = className;
            IconName   = iconName;
            FileName   = fileName;
            ModifiedBy = modifiedBy;
            FileSize   = fileSize;
        }

        public string ClassName  { get; }

        public string IconName   { get; }

        public string FileName   { get; }

        public string ModifiedBy { get; }

        public string FileSize   { get; }
    }
}
