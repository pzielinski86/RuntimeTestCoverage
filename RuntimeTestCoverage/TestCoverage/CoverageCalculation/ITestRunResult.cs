using Microsoft.CodeAnalysis;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestRunResult
    {
        string TestName { get; }
        AuditVariablePlaceholder[] AuditVariables { get; }
        bool ThrownException { get; }
        string ErrorMessage { get; }

        LineCoverage[] GetCoverage(
            SyntaxNode testMethod, 
            string testProjectName, 
            string testDocumentPath);
    }
}