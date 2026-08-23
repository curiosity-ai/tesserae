using System;
using System.Linq;

namespace Tesserae.Tests
{
    internal class Sample
    {
        public string           Type             { get; }
        public string           Name             { get; }
        public string           Group            { get; }
        public int              Order            { get; }
        public UIcons           Icon             { get; }

        /// <summary>What the component is for, in one short line, from <see cref="SampleDetailsAttribute.Description"/>.</summary>
        public string           Description      { get; }

        public Func<System.Threading.Tasks.Task<IComponent>> ContentGenerator { get; }

        public Sample(string type, string name, string group, int order, UIcons icon, string description, Func<System.Threading.Tasks.Task<IComponent>> contentGenerator)
        {
            Type             = type;
            Name             = name;
            Group            = group;
            Order            = order;
            Icon             = icon;
            Description      = description;
            ContentGenerator = contentGenerator;
        }

        public static string FormatSampleName(Type sampleType)
        {
            return FormatSampleName(sampleType.Name);
        }
        public static string FormatSampleName(string sampleType)
        {
            return string.Join("", sampleType.Replace("Sample", "").Select(c => char.IsUpper(c) ? " " + c : "" + c)).Trim().Replace("U Icons", "UIcons").Replace("A I ", "AI ").Replace("H T M L", "HTML").Replace(" And ", " and ");
        }
    }
}