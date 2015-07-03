using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.SqlServer.Server;

namespace TestCoverage
{
    public class LineCoverageEngine : MarshalByRefObject
    {
        public LineCoverage[] CalculateForAllDocuments(string solutionPath)
        {
            var rewritter = new SolutionRewritter();
            RewriteResult rewriteResult = rewritter.RewriteAllClasses(solutionPath);

            var solutionExplorer = new SolutionExplorer(solutionPath);
            solutionExplorer.Open();

            var lineCoverageCalc = new LineCoverageCalc(solutionExplorer);
            return lineCoverageCalc.CalculateForAllTests(solutionPath, rewriteResult);

        }

        public LineCoverage[] CalculateForTest(string solutionPath,string projectName, string documentPath, string documentContent, string className, string methodName)
        {
            var solutionExplorer = new SolutionExplorer(solutionPath);
            solutionExplorer.Open();

            var rewritter = new SolutionRewritter();
            RewrittenDocument rewrittenDocument = rewritter.RewriteDocument(documentPath, documentContent);

            var lineCoverageCalc = new LineCoverageCalc(solutionExplorer);

            Project project = solutionExplorer.Solution.Projects.Single(p => p.Name == projectName);
            return lineCoverageCalc.CalculateForTest(rewrittenDocument, project,className, methodName);

        }
    }
}