using Microsoft.CodeAnalysis;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestRunResult
    {
        AuditVariablePlaceholder[] SetAuditVars { get; }
        bool AssertionFailed { get; }
        string ErrorMessage { get; }

        LineCoverage[] GetCoverage(
            SyntaxNode testMethod, 
            string testProjectName, 
            string testDocumentPath);
    }
}