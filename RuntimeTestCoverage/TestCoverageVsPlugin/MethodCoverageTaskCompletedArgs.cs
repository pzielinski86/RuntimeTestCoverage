using System;

namespace TestCoverageVsPlugin
{
    public class MethodCoverageTaskCompletedArgs : EventArgs
    {
        public string DocPath { get; }

        public MethodCoverageTaskCompletedArgs(string docPath)
        {
            DocPath = docPath;
        }
    }
}