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
            _coverageStore = new FileCoverageStore(solutionPath);
            _solutionExplorer = new SolutionExplorer(solutionPath);
            var settingsStore = new XmlCoverageSettingsStore(solutionPath);
            _testExplorer = new TestExplorer(_solutionExplorer, new NUnitTestExtractor(), settingsStore);
            _solutionExplorer.Open();

        }

        public CoverageResult CalculateForAllDocuments()
        {
            var rewritter = new SolutionRewriter(_auditVariablesRewriter, new ContentWriter());
            string[] unIgnoredProjects = _coverageSettingsStore.GetIgnoredTestProjects();
            Project[] projects =
                _solutionExplorer.Solution.Projects.Where(x => unIgnoredProjects.Contains(x.Name))
                    .ToArray();

            RewriteResult rewrittenResult = rewritter.RewriteAllClasses(projects);

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer, new RoslynCompiler(), _coverageStore, new NUnitTestExtractor(), new AppDomainTestExecutorScriptEngine());
            var coverage = lineCoverageCalc.CalculateForAllTests(rewrittenResult);

            _coverageStore.WriteAll(coverage);

            return new CoverageResult(coverage);
        }

        public CoverageResult CalculateForDocument(string projectName, string documentPath, string documentContent)
        {
            var rewritter = new SolutionRewriter(_auditVariablesRewriter, new ContentWriter());
            RewrittenDocument rewrittenDocument = rewritter.RewriteDocument(projectName, documentPath, documentContent);

            var lineCoverageCalc = new LineCoverageCalc(_solutionExplorer, new RoslynCompiler(), _coverageStore, new NUnitTestExtractor(), new AppDomainTestExecutorScriptEngine());
            Project project = _solutionExplorer.Solution.Projects.Single(p => p.Name == projectName);
            var coverage = lineCoverageCalc.CalculateForDocument(rewrittenDocument, project);

            _coverageStore.Append(documentPath, coverage);

            return new CoverageResult(coverage);
        }

        public void Dispose()
        {
        }
    }
}