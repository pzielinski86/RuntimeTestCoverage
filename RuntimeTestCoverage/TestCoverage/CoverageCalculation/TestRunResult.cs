using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    [Serializable]
    public class TestRunResult : ITestRunResult
    {
        public string TestName { get; private set; }
        public AuditVariablePlaceholder[] AuditVariables { get; }
        public bool ThrownException => ErrorMessage != null;
        public string ErrorMessage { get; }

        public TestRunResult(string testName, AuditVariablePlaceholder[] auditVariables, string errorMessage)
        {
            TestName = testName;
            AuditVariables = auditVariables;
            ErrorMessage = errorMessage;
        }

        public virtual LineCoverage[] GetCoverage(
            SyntaxNode testMethod,
            string testProjectName,
            string testDocumentPath)
        {
            List<LineCoverage> coverage = new List<LineCoverage>();
            string testDocName = Path.GetFileNameWithoutExtension(testDocumentPath);

            var variablesSortedByExecutionOrder = AuditVariables.OrderBy(x => x.ExecutionCounter);

            var lastAuditVariableInTest = variablesSortedByExecutionOrder.Last(x => x.DocumentPath == testDocumentPath);
            var lastAuditVariableInSut =
                variablesSortedByExecutionOrder.LastOrDefault(x => x.DocumentPath != testDocumentPath);

            foreach (var variable in AuditVariables)
            {
                LineCoverage lineCoverage = LineCoverage.EvaluateAuditVariable(variable, testMethod, testProjectName, testDocName);
                lineCoverage.DocumentPath = variable.DocumentPath;
                lineCoverage.TestDocumentPath = testDocumentPath;
                lineCoverage.IsSuccess = true;

                if (ThrownException)
                {
                    if (lineCoverage.IsItInTestMethod)
                    {
                        if (variable == lastAuditVariableInTest)
                        {
                            lineCoverage.IsSuccess = false;
                            lineCoverage.ErrorMessage = ErrorMessage;
                        }
                    }
                    else if (variable == lastAuditVariableInSut)
                    {
                        lineCoverage.IsSuccess = false;
                        lineCoverage.ErrorMessage = ErrorMessage;
                    }
                }

                coverage.Add(lineCoverage);
            }

            return coverage.ToArray();
        }
    }
}