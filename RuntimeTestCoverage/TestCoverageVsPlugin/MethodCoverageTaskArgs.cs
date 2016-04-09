using System;

namespace TestCoverageVsPlugin
{
    public class MethodCoverageTaskArgs : EventArgs
    {
        public string DocPath { get; }
        public string MethodName { get; }

        public MethodCoverageTaskArgs(string docPath,string methodName)
        {
            DocPath = docPath;
            MethodName = methodName;
        }
    }
}