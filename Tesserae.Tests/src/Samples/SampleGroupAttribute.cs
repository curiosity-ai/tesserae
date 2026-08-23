using System;

namespace Tesserae.Tests
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class SampleDetailsAttribute : Attribute
    {
        public string Group { get; set; }
        public int    Order { get; set; }
        public UIcons Icon  { get; set; }

        /// <summary>
        /// One line saying what the component is for. The landing page shows it under the sample's
        /// name, on a single ellipsized line, so it has to stay short — about 50 characters — and
        /// read as a description of the component rather than of the sample page.
        /// </summary>
        public string Description { get; set; }
    }
}
