using System;

namespace TestCoverageSandbox
{
    [Serializable]
    public class TestCoverageCompilationException : Exception
    {
        public TestCoverageCompilationException(string[] errors) :base("Cannot compile test coverage exception.")
        {
            Errors = errors;
        }

        public string[] Errors { get; private set; }
    }
}