using System;

namespace TestCoverageVsPlugin
{
    public class MethodCoverageTaskArgs : EventArgs
    {
        public string DocPath { get; }

        public MethodCoverageTaskArgs(string docPath)
        {
            DocPath = docPath;
        }
    }
}