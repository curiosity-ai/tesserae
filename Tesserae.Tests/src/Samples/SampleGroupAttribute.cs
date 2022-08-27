using System;

namespace Tesserae.Tests
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class SampleDetailsAttribute : Attribute
    {
        public string Group { get; set; }
        public int Order { get; set; }
        public LineAwesome Icon { get; set; }
    }
}