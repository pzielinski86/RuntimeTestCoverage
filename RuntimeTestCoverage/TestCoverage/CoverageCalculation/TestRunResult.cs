namespace TestCoverage.CoverageCalculation
{
    public class TestRunResult
    {
        public string[] SetAuditVars { get; }
        public bool AssertionFailed { get;}
        public string ErrorMessage { get; }

        public TestRunResult(string[] setAuditVars, bool assertionFailed, string errorMessage)
        {
            SetAuditVars = setAuditVars;
            AssertionFailed = assertionFailed;
            ErrorMessage = errorMessage;
        }
    }
}