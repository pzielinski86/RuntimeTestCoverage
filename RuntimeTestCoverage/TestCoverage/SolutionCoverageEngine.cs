using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Rewrite;
using TestCoverage.Storage;

namespace TestCoverage
{
    public class SolutionCoverageEngine : MarshalByRefObject, ISolutionCoverageEngine
    {
        private ICoverageStore _coverageStore;
        private ISolutionExplorer _solutionExplorer;
        private IAuditVariablesRewriter _auditVariablesRewriter;


        public void Init(string solutionPath)
        {
            _auditVariablesRewriter=new AuditVariablesRewriter(new AuditVariablesWalker());
            _coverageStore = new FileCoverageStore(solutionPath);
            _solutionExplorer = new SolutionExplorer(solutionPath);
            _solutionExplorer.Open();

        }

        public CoverageResult CalculateForAllDocuments()
        {
            var rewritter = new SolutionRewriter(_solutionExplorer, _auditVariablesRewriter, new ContentWriter());
            RewriteResult rewriteResult = rewritter.RewriteAllClasses();

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer, new RoslynCompiler(),_coverageStore, new NUnitTestExtractor(), new AppDomainTestExecutorScriptEngine());
            var coverage = lineCoverageCalc.CalculateForAllTests(rewriteResult);

            _coverageStore.Append(coverage);

            return new CoverageResult(coverage);
        }

        public CoverageResult CalculateForDocument(string projectName, string documentPath, string documentContent)
        {
            var rewritter = new SolutionRewriter(_solutionExplorer, _auditVariablesRewriter, new ContentWriter());
            RewrittenDocument rewrittenDocument = rewritter.RewriteDocument(projectName, documentPath, documentContent);

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer, new RoslynCompiler(), _coverageStore,new NUnitTestExtractor(), new AppDomainTestExecutorScriptEngine());
            Project project = _solutionExplorer.Solution.Projects.Single(p => p.Name == projectName);
            var coverage = lineCoverageCalc.CalculateForDocument(rewrittenDocument, project);

            _coverageStore.Append(coverage);

            return new CoverageResult(coverage);
        }
        public void Dispose()
        {
            
        }
    }
}