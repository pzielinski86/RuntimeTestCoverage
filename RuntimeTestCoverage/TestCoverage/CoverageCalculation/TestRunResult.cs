namespace TestCoverage.CoverageCalculation
{
    public class TestRunResult
    {
        public string[] SetAuditVars { get; private set; }
        public bool AssertionFailed { get; private set; }

        public TestRunResult(string[] setAuditVars,bool assertionFailed)
        {
            SetAuditVars = setAuditVars;
            AssertionFailed = assertionFailed;
        }
    }
}