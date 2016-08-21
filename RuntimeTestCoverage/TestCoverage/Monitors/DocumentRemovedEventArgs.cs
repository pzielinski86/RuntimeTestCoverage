using System;

namespace TestCoverage.Monitors
{
    public class DocumentRemovedEventArgs:EventArgs
    {
        public string DocumentPath { get; private set; }

        public DocumentRemovedEventArgs(string documentPath)
        {
            DocumentPath = documentPath;
        }
    }
}