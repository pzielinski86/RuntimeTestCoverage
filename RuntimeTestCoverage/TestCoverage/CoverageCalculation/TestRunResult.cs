using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public class TestRunResult : ITestRunResult
    {
        public AuditVariablePlaceholder[] SetAuditVars { get; }
        public bool AssertionFailed { get; }
        public string ErrorMessage { get; }

        public TestRunResult(AuditVariablePlaceholder[] setAuditVars, bool assertionFailed, string errorMessage)
        {
            SetAuditVars = setAuditVars;
            AssertionFailed = assertionFailed;
            ErrorMessage = errorMessage;
        }

        public virtual LineCoverage[] GetCoverage( 
            SyntaxNode testMethod, 
            string testProjectName, 
            string testDocumentPath)
        {
            List<LineCoverage> coverage = new List<LineCoverage>();
            string testDocName = Path.GetFileNameWithoutExtension(testDocumentPath);

            foreach (var variable in SetAuditVars)
            {

                LineCoverage lineCoverage = LineCoverage.EvaluateAuditVariable(variable, testMethod, testProjectName, testDocName);

                if (AssertionFailed)
                {
                    if (lineCoverage.Path == lineCoverage.TestPath && variable != SetAuditVars.Last())
                        lineCoverage.IsSuccess = true;
                    else
                        lineCoverage.IsSuccess = false;
                }
                else
                    lineCoverage.IsSuccess = true;


                lineCoverage.DocumentPath = variable.DocumentPath;
                lineCoverage.TestDocumentPath = testDocumentPath;

                coverage.Add(lineCoverage);
            }

            return coverage.ToArray();
        }
    }
}