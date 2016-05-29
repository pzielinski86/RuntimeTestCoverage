using System;

namespace TestCoverageVsPlugin
{
    public abstract class CoverageTaskArgsBase : EventArgs
    {
        public string DocPath { get; }

        public CoverageTaskArgsBase(string docPath)
        {
            DocPath = docPath;
        }
    }
}