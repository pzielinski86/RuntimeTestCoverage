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
        private ITestExplorer _testExplorer;

        public void Init(string solutionPath)
        {
            _auditVariablesRewriter = new AuditVariablesRewriter(new AuditVariablesWalker());
            _coverageStore = new XmlCoverageStore(solutionPath);
            _solutionExplorer = new SolutionExplorer(solutionPath);
            var settingsStore = new XmlCoverageSettingsStore(solutionPath);
            _testExplorer = new TestExplorer(_solutionExplorer, new NUnitTestExtractor(), settingsStore);
            _solutionExplorer.Open();

        }

        public CoverageResult CalculateForAllDocuments()
        {
            var rewritter = new SolutionRewriter(_auditVariablesRewriter, new ContentWriter());

            //TODO: Change a method to async and don't use .Result
            var projects = _testExplorer.GetUnignoredTestProjectsWithCoveredProjectsAsync().Result;
            RewriteResult rewrittenResult = rewritter.RewriteAllClasses(projects);

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer, new RoslynCompiler(), _coverageStore, 
                new TestRunner(new NUnitTestExtractor(), new AppDomainTestExecutorScriptEngine(), _solutionExplorer));
            var coverage = lineCoverageCalc.CalculateForAllTests(rewrittenResult);

            _coverageStore.WriteAll(coverage);

            return new CoverageResult(coverage);
        }

        public CoverageResult CalculateForDocument(string projectName, string documentPath, string documentContent)
        {
            var projects = _testExplorer.GetUnignoredTestProjectsWithCoveredProjectsAsync().Result;
            var project = projects.FirstOrDefault(x => x.Name == projectName);

            if (project == null)
                return new CoverageResult(new LineCoverage[0]);

            var rewritter = new SolutionRewriter(_auditVariablesRewriter, new ContentWriter());
            RewrittenDocument rewrittenDocument = rewritter.RewriteDocument(project, documentPath, documentContent);

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer, new RoslynCompiler(), _coverageStore,
                new TestRunner(new NUnitTestExtractor(), new AppDomainTestExecutorScriptEngine(), _solutionExplorer));

            var coverage = lineCoverageCalc.CalculateForDocument(project, rewrittenDocument);

            _coverageStore.Append(documentPath, coverage);

            return new CoverageResult(coverage);
        }

        public void Dispose()
        {
        }
    }
}