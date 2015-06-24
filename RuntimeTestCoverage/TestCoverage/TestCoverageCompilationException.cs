using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TestCoverage
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