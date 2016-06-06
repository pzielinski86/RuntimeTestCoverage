using System;

namespace TestCoverage.CoverageCalculation
{
    [Serializable]
    public class TestExecutionScriptParameters
    {
        public string TestFixtureTypeFullName;
        public string SetupMethodName;
        public string TestName;
        public object[] TestParameters;
        public bool IsAsync { get; set; }
    }
}