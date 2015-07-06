using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using TestCoverage.CoverageCalculation;
using TestCoverage.Rewrite;

namespace TestCoverage
{
    public class SolutionCoverageEngine : MarshalByRefObject
    {
        private SolutionExplorer _solutionExplorer;        

        public void Init(string solutionPath)
        {
            _solutionExplorer=new SolutionExplorer(solutionPath);
            _solutionExplorer.Open();
                        
        }

        public Dictionary<string,LineCoverage[]> CalculateForAllDocuments()
        {             
            var rewritter = new SolutionRewritter(_solutionExplorer);
            RewriteResult rewriteResult = rewritter.RewriteAllClasses(_solutionExplorer.SolutionPath);                                   

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer);
            return lineCoverageCalc.CalculateForAllTests(_solutionExplorer.SolutionPath, rewriteResult);

        }

        public Dictionary<string, LineCoverage[]> CalculateForTest(string projectName, string documentPath, string documentContent, string className, string methodName)
        {
            var rewritter = new SolutionRewritter(_solutionExplorer);
            RewrittenDocument rewrittenDocument = rewritter.RewriteDocument(documentPath, documentContent);

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer);

            Project project = _solutionExplorer.Solution.Projects.Single(p => p.Name == projectName);
            return lineCoverageCalc.CalculateForTest(rewrittenDocument, project,className, methodName);

        }
    }
}