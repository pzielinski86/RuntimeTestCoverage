using System;

namespace TestCoverage.CoverageCalculation
{
    [Serializable]
    public class TestExecutionScriptParameters
    {        
        public string SetUpMethodName;
        public string TearDownMethodName;
        public string TestName;
        public object[] TestParameters;
        public bool IsAsync { get; set; }
    }
}