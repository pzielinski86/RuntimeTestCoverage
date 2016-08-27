using System;
using System.Collections.Generic;

namespace TestCoverage.CoverageCalculation
{
    [Serializable]
    public class TestFixtureExecutionScriptParameters
    {
        public string TestSetUpMethodName;
        public string TestTearDownMethodName;
        public string TestFixtureTypeFullName;
        public string TestFixtureSetUpMethodName { get; set; }
        public string TestFixtureTearDownMethodName { get; set; }
        public string TestFixtureAssemblyName { get; set; }

        public List<TestExecutionScriptParameters> TestCases;
    }
}