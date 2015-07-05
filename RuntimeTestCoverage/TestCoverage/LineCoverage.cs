using System;

namespace TestCoverage
{
    [Serializable]
    public class LineCoverage
    {
        public int Span { get; set; }
        public string Path { get; set; }
        public string TestPath { get; set; }

    }
}