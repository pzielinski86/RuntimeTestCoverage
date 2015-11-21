using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    [Serializable]
    public class TestRunResult : ITestRunResult
    {
        public AuditVariablePlaceholder[] AuditVariables { get; }
        public bool AssertionFailed { get; }
        public string ErrorMessage { get; }

        public TestRunResult(AuditVariablePlaceholder[] auditVariables, bool assertionFailed, string errorMessage)
        {
            AuditVariables = auditVariables;
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

            foreach (var variable in AuditVariables)
            {
                LineCoverage lineCoverage = LineCoverage.EvaluateAuditVariable(variable, testMethod, testProjectName, testDocName);

                if (AssertionFailed)
                {
                    if (lineCoverage.NodePath == lineCoverage.TestPath && variable != AuditVariables.Last())
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