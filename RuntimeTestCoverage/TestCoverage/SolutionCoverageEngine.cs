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
        private ISolutionExplorer _solutionExplorer;
        private IAuditVariablesRewriter _auditVariablesRewriter;

        public void Init(string solutionPath)
        {
            _auditVariablesRewriter=new AuditVariablesRewriter(new AuditVariablesWalker());
            _solutionExplorer =new SolutionExplorer(solutionPath);
            _solutionExplorer.Open();
                        
        }

        public Dictionary<string,LineCoverage[]> CalculateForAllDocuments()
        {             
            var rewritter = new SolutionRewriter(_solutionExplorer, _auditVariablesRewriter,  new ContentWriter());
            RewriteResult rewriteResult = rewritter.RewriteAllClasses();

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer);
            return lineCoverageCalc.CalculateForAllTests(_solutionExplorer.SolutionPath, rewriteResult);

        }

        public Dictionary<string, LineCoverage[]> CalculateForDocument(string projectName, string documentPath, string documentContent)
        {
            var rewritter = new SolutionRewriter(_solutionExplorer, _auditVariablesRewriter, new ContentWriter());
            RewrittenDocument rewrittenDocument = rewritter.RewriteDocument(projectName,documentPath, documentContent);

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer);
            Project project = _solutionExplorer.Solution.Projects.Single(p => p.Name == projectName);
            return lineCoverageCalc.CalculateForDocument(rewrittenDocument, project);

        }

        public Dictionary<string, LineCoverage[]> CalculateForTest(string projectName, string documentPath, string documentContent, string className, string methodName)
        {
            var rewritter = new SolutionRewriter(_solutionExplorer, _auditVariablesRewriter, new ContentWriter());
            RewrittenDocument rewrittenDocument = rewritter.RewriteDocument(projectName,documentPath, documentContent);

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer);

            Project project = _solutionExplorer.Solution.Projects.Single(p => p.Name == projectName);
            return lineCoverageCalc.CalculateForTest(rewrittenDocument, project,className, methodName);

        }
    }
}