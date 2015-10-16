namespace TestCoverage.CoverageCalculation
{
    public class TestRunResult
    {
        public string[] SetAuditVars { get; }
        public string ErrorMessage { get; }

        public TestRunResult(string[] setAuditVars,  string errorMessage)
        {
            SetAuditVars = setAuditVars;
            ErrorMessage = errorMessage;
        }
    }
}