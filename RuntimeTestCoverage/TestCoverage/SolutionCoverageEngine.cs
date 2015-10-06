using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Rewrite;

namespace TestCoverage
{
    public class SolutionCoverageEngine : MarshalByRefObject, ISolutionCoverageEngine
    {
        private ISolutionExplorer _solutionExplorer;
        private IAuditVariablesRewriter _auditVariablesRewriter;

        public void Init(string solutionPath)
        {
            _auditVariablesRewriter = new AuditVariablesRewriter(new AuditVariablesWalker());
            _solutionExplorer = new SolutionExplorer(solutionPath);
            _solutionExplorer.Open();

        }

        public CoverageResult CalculateForAllDocuments()
        {
            var rewritter = new SolutionRewriter(_solutionExplorer, _auditVariablesRewriter, new ContentWriter());
            RewriteResult rewriteResult = rewritter.RewriteAllClasses();

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer, new RoslynCompiler(), new NUnitTestExtractor(), new AppDomainTestExecutorScriptEngine());
            var coverage = lineCoverageCalc.CalculateForAllTests(rewriteResult);

            return new CoverageResult(coverage);
        }

        public CoverageResult CalculateForDocument(string projectName, string documentPath, string documentContent)
        {
            var rewritter = new SolutionRewriter(_solutionExplorer, _auditVariablesRewriter, new ContentWriter());
            RewrittenDocument rewrittenDocument = rewritter.RewriteDocument(projectName, documentPath, documentContent);

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer, new RoslynCompiler(), new NUnitTestExtractor(), new AppDomainTestExecutorScriptEngine());
            Project project = _solutionExplorer.Solution.Projects.Single(p => p.Name == projectName);
            var coverage = lineCoverageCalc.CalculateForDocument(rewrittenDocument, project);

            return new CoverageResult(coverage);
        }
        public void Dispose()
        {
            
        }
    }
}