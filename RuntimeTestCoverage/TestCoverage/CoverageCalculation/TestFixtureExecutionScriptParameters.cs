using System;
using System.Collections.Generic;

namespace TestCoverage.CoverageCalculation
{
    [Serializable]
    public class TestFixtureExecutionScriptParameters
    {
        public string TestFixtureTypeFullName;
        public string TestFixtureSetUpMethodName { get; set; }
        public string TestFixtureTearDownMethodName { get; set; }
        public List<TestExecutionScriptParameters> TestCases;
    }
}