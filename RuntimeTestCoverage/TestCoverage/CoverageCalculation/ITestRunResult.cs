using Microsoft.CodeAnalysis;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestRunResult
    {
        string[] SetAuditVars { get; }
        bool AssertionFailed { get; }
        string ErrorMessage { get; }

        LineCoverage[] GetCoverage(AuditVariablesMap auditVariablesMap, 
            SyntaxNode testMethod, 
            string testProjectName, 
            string testDocumentPath);
    }
}