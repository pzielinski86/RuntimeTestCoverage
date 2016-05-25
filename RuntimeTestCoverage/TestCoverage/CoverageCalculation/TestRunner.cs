using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestCoverage.Compilation;
using TestCoverage.Extensions;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public class TestRunner : ITestRunner
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
            MethodDeclarationSyntax method,
            ISemanticModel semanticModel,
              string[] rewrittenAssemblies)
        {
            var testClass = method.GetParentClass();
            var rewrittenTestClass =
                rewrittenDocument.SyntaxTree
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>().First(x => x.Identifier.ToString() == testClass.Identifier.ToString());

            var fixtureDetails = _testsExtractor.GetTestFixtureDetails(rewrittenTestClass, semanticModel);
            var allReferences = _solutionExplorer.GetAllProjectReferences(project.Name);

            var testCases = fixtureDetails.Cases.Where(x => x.MethodName == method.Identifier.ToString()).ToList();

            if (testCases.Count == 0)
                return null;

            var compiledTestInfo = new CompiledTestFixtureInfo
            {
                AllReferences = allReferences.Union(rewrittenAssemblies).ToArray(),
                TestDocumentPath = rewrittenDocument.DocumentPath,
                SemanticModel = semanticModel
            };

            var coverage = RunTestCases(testCases, compiledTestInfo, project.Name);

            return coverage;
        }

        public LineCoverage[] RunAllTestsInDocument(RewrittenDocument rewrittenDocument,
            ISemanticModel semanticModel,
            Project project,
            string[] rewrittenAssemblies)
        {
            var allReferences = _solutionExplorer.GetAllProjectReferences(project.Name);

            var compiledTestInfo = new CompiledTestFixtureInfo
            {
                AllReferences = allReferences.Union(rewrittenAssemblies).ToArray(),
                TestDocumentPath = rewrittenDocument.DocumentPath,
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

        private LineCoverage[] RunTestCases(List<TestCase> testCases,
            CompiledTestFixtureInfo compiledTestFixtureInfo,
            string testProjectName)
        {

            var coverage = new List<LineCoverage>();

            foreach (var testCase in testCases)
            {
                var scriptParameters = new TestExecutionScriptParameters
                {
                    TestFixtureTypeFullName = testCase.TestFixture.FullQualifiedName,
                    SetupMethodName = testCase.TestFixture.SetupMethodName,
                    TestName = testCase.MethodName,
                    TestParameters = testCase.Arguments,
                    IsAsync = testCase.IsAsync
                };

                var testResult =
                    _testExecutorScriptEngine.RunTest(compiledTestFixtureInfo.AllReferences,
                        scriptParameters);

                var partialCoverage = testResult.GetCoverage(
                                 testCase.SyntaxNode,
                                 testProjectName,
                                 compiledTestFixtureInfo.TestDocumentPath);

                coverage.AddRange(partialCoverage);
            }

            return coverage.OrderBy(x => x.TestPath).ToArray();
        }
    }
}