using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading.Tasks;
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
        private bool _isDisposed;

        public void Init(string solutionPath)
        {
            _solutionExplorer = new SolutionExplorer(solutionPath);
            _auditVariablesRewriter = new AuditVariablesRewriter(new AuditVariablesWalker());
            _coverageStore = new SqlCompactCoverageStore();
            var settingsStore = new XmlCoverageSettingsStore();
            _testExplorer = new TestExplorer(_solutionExplorer, new NUnitTestExtractor(), _coverageStore, settingsStore);

        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public async Task<CoverageResult> CalculateForAllDocumentsAsync()
        {
            var rewritter = new SolutionRewriter(_auditVariablesRewriter);

            //TODO: Change a method to async and don't use .Result
            var projects = await _testExplorer.GetUnignoredTestProjectsWithCoveredProjectsAsync().ConfigureAwait(false);
            RewriteResult rewrittenResult = rewritter.RewriteAllClasses(projects);

            LineCoverage[] coverage = null;

            using (var appDomainTestExecutorScriptEngine = new AppDomainTestExecutorScriptEngine())
            {
                var lineCoverageCalc = new LineCoverageCalc(_testExplorer, new RoslynCompiler(), new TestRunner(new NUnitTestExtractor(), appDomainTestExecutorScriptEngine, _solutionExplorer));

                coverage = lineCoverageCalc.CalculateForAllTests(rewrittenResult);
            }

            _coverageStore.WriteAll(coverage);

            return new CoverageResult(coverage);
        }

        public CoverageResult CalculateForMethod(string projectName, MethodDeclarationSyntax method)
        {
            var projects = _testExplorer.GetUnignoredTestProjectsWithCoveredProjectsAsync().Result;
            var project = projects.FirstOrDefault(x => x.Name == projectName);

            if (project == null)
                return new CoverageResult(new LineCoverage[0]);

            var rewritter = new SolutionRewriter(_auditVariablesRewriter);
            RewrittenDocument rewrittenDocument = rewritter.RewriteDocumentWithAssemblyInfo(project, projects, method.SyntaxTree.FilePath, method.SyntaxTree.ToString());

            LineCoverage[] coverage = null;

            using (var appDomainTestExecutorScriptEngine = new AppDomainTestExecutorScriptEngine())
            {
                var lineCoverageCalc = new LineCoverageCalc(_testExplorer, new RoslynCompiler(),
                    new TestRunner(new NUnitTestExtractor(), appDomainTestExecutorScriptEngine, _solutionExplorer));

                coverage = lineCoverageCalc.CalculateForMethod(project, rewrittenDocument, method);
            }

            _coverageStore.Append(coverage);

            return new CoverageResult(coverage);
        }

        public CoverageResult CalculateForDocument(string projectName, string documentPath, string documentContent)
        {
            var projects = _testExplorer.GetUnignoredTestProjectsWithCoveredProjectsAsync().Result;
            var project = projects.FirstOrDefault(x => x.Name == projectName);

            if (project == null)
                return new CoverageResult(new LineCoverage[0]);

            var rewritter = new SolutionRewriter(_auditVariablesRewriter);
            RewrittenDocument rewrittenDocument = rewritter.RewriteDocumentWithAssemblyInfo(project, projects, documentPath, documentContent);

            LineCoverage[] coverage = null;
            using (var appDomainTestExecutorScriptEngine = new AppDomainTestExecutorScriptEngine())
            {
                var lineCoverageCalc = new LineCoverageCalc(_testExplorer, new RoslynCompiler(),
                    new TestRunner(new NUnitTestExtractor(), appDomainTestExecutorScriptEngine, _solutionExplorer));

                coverage = lineCoverageCalc.CalculateForDocument(project, rewrittenDocument);

            }
            _coverageStore.AppendByDocumentPath(documentPath, coverage);

            return new CoverageResult(coverage);
        }

        public bool IsDisposed => _isDisposed;

        public void Dispose()
        {
            _isDisposed = true;
        }
    }
}