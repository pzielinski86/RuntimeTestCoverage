using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public class TestRunResult
    {
        public string[] SetAuditVars { get; }
        public bool AssertionFailed { get; }
        public string ErrorMessage { get; }

        public TestRunResult(string[] setAuditVars, bool assertionFailed, string errorMessage)
        {
            SetAuditVars = setAuditVars;
            AssertionFailed = assertionFailed;
            ErrorMessage = errorMessage;
        }

        public LineCoverage[] GetCoverage(AuditVariablesMap auditVariablesMap, 
            SyntaxNode testMethod, 
            string testProjectName, 
            string testDocumentPath)
        {
            List<LineCoverage> coverage = new List<LineCoverage>();
            string testDocName = Path.GetFileNameWithoutExtension(testDocumentPath);

            foreach (string varName in SetAuditVars)
            {
                string docPath = auditVariablesMap.Map[varName].DocumentPath;

                LineCoverage lineCoverage = LineCoverage.EvaluateAuditVariable(auditVariablesMap, varName, testMethod, testProjectName, testDocName);

                if (AssertionFailed)
                {
                    if (lineCoverage.Path == lineCoverage.TestPath && varName != SetAuditVars.Last())
                        lineCoverage.IsSuccess = true;
                    else
                        lineCoverage.IsSuccess = false;
                }
                else
                    lineCoverage.IsSuccess = true;


                lineCoverage.DocumentPath = docPath;
                lineCoverage.TestDocumentPath = testDocumentPath;

                coverage.Add(lineCoverage);
            }

            return coverage.ToArray();
        }
    }
}