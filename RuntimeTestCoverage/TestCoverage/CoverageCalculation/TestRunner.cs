using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Compilation;
using TestCoverage.Extensions;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    internal class TestRunner : ITestRunner
    {
        private readonly ITestsExtractor _testsExtractor;
        private readonly ITestExecutorScriptEngine _testExecutorScriptEngine;
        private readonly ISolutionExplorer _solutionExplorer;

        public TestRunner(ITestsExtractor testsExtractor,
            ITestExecutorScriptEngine testExecutorScriptEngine,
            ISolutionExplorer solutionExplorer)
        {
            _testsExtractor = testsExtractor;
            _testExecutorScriptEngine = testExecutorScriptEngine;
            _solutionExplorer = solutionExplorer;
        }

        public LineCoverage[] RunTest(Project project,
            RewrittenDocument rewrittenDocument,
            string methodName,
            ISemanticModel semanticModel,
              _Assembly[] allAssemblies)
        {
            var testClass = _testsExtractor.GetTestClasses(rewrittenDocument.SyntaxTree.GetRoot()).Single();
            var fixtureDetails = _testsExtractor.GetTestFixtureDetails(testClass, semanticModel);
            var allReferences = _solutionExplorer.GetAllProjectReferences(project.Name);

            var testCases = fixtureDetails.Cases.Where(x => x.MethodName == methodName).ToList();

            var compiledTestInfo = new CompiledTestFixtureInfo
            {
                TestProjectReferences = allReferences,
                TestDocumentPath = rewrittenDocument.DocumentPath,
                AllAssemblies = allAssemblies,
                SemanticModel = semanticModel
            };

            var coverage = RunTestCases(testCases, compiledTestInfo, project.Name);

            return coverage;
        }

        public LineCoverage[] RunAllTestsInDocument(RewrittenDocument rewrittenDocument,
            ISemanticModel semanticModel,
            Project project,
            _Assembly[] allAssemblies)
        {
            var allReferences = _solutionExplorer.GetAllProjectReferences(project.Name);

            var compiledTestInfo = new CompiledTestFixtureInfo
            {
                TestProjectReferences = allReferences,
                TestDocumentPath = rewrittenDocument.DocumentPath,
                AllAssemblies = allAssemblies,
                SemanticModel = semanticModel
            };

            var coverage = new List<LineCoverage>();

            var testClasses = _testsExtractor.GetTestClasses(rewrittenDocument.SyntaxTree.GetRoot());

            if (testClasses.Length == 0)
                return null;

            foreach (ClassDeclarationSyntax testClass in testClasses)
            {
                compiledTestInfo.TestClass = testClass;

                var partialCoverage = RunAllTestsInFixture(compiledTestInfo, project.Name);
                coverage.AddRange(partialCoverage);
            }

            return coverage.ToArray();
        }

        private LineCoverage[] RunAllTestsInFixture(CompiledTestFixtureInfo compiledTestFixtureInfo, string testProjectName)
        {
            TestFixtureDetails testFixtureDetails = _testsExtractor.GetTestFixtureDetails(compiledTestFixtureInfo.TestClass, compiledTestFixtureInfo.SemanticModel);

            var coverage = RunTestCases(testFixtureDetails.Cases, compiledTestFixtureInfo, testProjectName);

            return coverage;
        }

        private LineCoverage[] RunTestCases(List<TestCase> testCases, CompiledTestFixtureInfo compiledTestFixtureInfo, string testProjectName)
        {
            var coverage = new ConcurrentBag<LineCoverage>();

            Parallel.For(0, testCases.Count, new ParallelOptions { MaxDegreeOfParallelism = 1 }, i =>
            {
                ITestRunResult testResult =
                    _testExecutorScriptEngine.RunTest(compiledTestFixtureInfo.TestProjectReferences,
                        compiledTestFixtureInfo.AllAssemblies,
                        testCases[i]);

                var partialCoverage = testResult.GetCoverage(
                     testCases[i].SyntaxNode,
                    testProjectName,
                    compiledTestFixtureInfo.TestDocumentPath);

                coverage.AddRange(partialCoverage);
            });

            return coverage.ToArray();
        }
    }
}