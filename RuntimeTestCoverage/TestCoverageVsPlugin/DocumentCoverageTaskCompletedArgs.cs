using System;

namespace TestCoverageVsPlugin
{
    public class DocumentCoverageTaskCompletedArgs : EventArgs
    {
        public string DocPath { get; }

        public DocumentCoverageTaskCompletedArgs(string docPath)
        {
            DocPath = docPath;
        }
    }
}