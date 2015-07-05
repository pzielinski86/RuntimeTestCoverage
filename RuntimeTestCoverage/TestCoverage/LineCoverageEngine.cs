using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.SqlServer.Server;

namespace TestCoverage
{
    public class LineCoverageEngine : MarshalByRefObject
    {
        private SolutionExplorer _solutionExplorer;        

        public void Init(string solutionPath)
        {
            _solutionExplorer=new SolutionExplorer(solutionPath);
            _solutionExplorer.Open();
        }

        public LineCoverage[] CalculateForAllDocuments()
        {             
            var rewritter = new SolutionRewritter(_solutionExplorer);
            RewriteResult rewriteResult = rewritter.RewriteAllClasses(_solutionExplorer.SolutionPath);                                   

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer);
            return lineCoverageCalc.CalculateForAllTests(_solutionExplorer.SolutionPath, rewriteResult);

        }

        public LineCoverage[] CalculateForTest(string projectName, string documentPath, string documentContent, string className, string methodName)
        {
            var rewritter = new SolutionRewritter(_solutionExplorer);
            RewrittenDocument rewrittenDocument = rewritter.RewriteDocument(documentPath, documentContent);

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer);

            Project project = _solutionExplorer.Solution.Projects.Single(p => p.Name == projectName);
            return lineCoverageCalc.CalculateForTest(rewrittenDocument, project,className, methodName);

        }
    }
}