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

            fixtureDetails.Cases.RemoveAll(x => x.MethodName != method.Identifier.ToString());

            if (fixtureDetails.Cases.Count == 0)
                return null;

            var compiledTestInfo = new CompiledTestFixtureInfo
            {
                AllReferences = allReferences.Union(rewrittenAssemblies).ToArray(),
                TestDocumentPath = rewrittenDocument.DocumentPath,
                SemanticModel = semanticModel
            };

            var coverage = RunTestFixture(fixtureDetails, compiledTestInfo, project.Name);

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

            var coverage = RunTestFixture(testFixtureDetails, compiledTestFixtureInfo, testProjectName);

            return coverage;
        }

        private LineCoverage[] RunTestFixture(TestFixtureDetails testFixtureDetails,
            CompiledTestFixtureInfo compiledTestFixtureInfo,
            string testProjectName)
        {
            var testFixtureExecutionScriptParameters = new TestFixtureExecutionScriptParameters
            {
                TestFixtureTypeFullName = testFixtureDetails.FullClassName,
                TestFixtureAssemblyName=testFixtureDetails.AssemblyName,
                TestCases = new List<TestExecutionScriptParameters>(),
                TestFixtureSetUpMethodName = testFixtureDetails.TestFixtureSetUpMethodName,
                TestFixtureTearDownMethodName = testFixtureDetails.TestFixtureTearDownMethodName,
                TestSetUpMethodName = testFixtureDetails.TestSetUpMethodName,
                TestTearDownMethodName = testFixtureDetails.TestTearDownMethodName,
            };

            foreach (var testCase in testFixtureDetails.Cases)
            {
                var scriptParameters = new TestExecutionScriptParameters
                {
                    TestName = testCase.MethodName,
                    TestParameters = testCase.Arguments,
                    IsAsync = testCase.IsAsync
                };

                testFixtureExecutionScriptParameters.TestCases.Add(scriptParameters);
            }

            var results = _testExecutorScriptEngine.RunTestFixture(compiledTestFixtureInfo.AllReferences, testFixtureExecutionScriptParameters);

            return GetCoverage(testFixtureDetails, compiledTestFixtureInfo, testProjectName, results);
        }

        private static LineCoverage[] GetCoverage(TestFixtureDetails testFixtureDetails,
            CompiledTestFixtureInfo compiledTestFixtureInfo, string testProjectName, ITestRunResult[] results)
        {
            var coverage = new List<LineCoverage>(results.Length);

            foreach (var testRunResult in results)
            {
                var methodSyntaxNode = testFixtureDetails.Cases.
                    FirstOrDefault(x => x.MethodName == testRunResult.TestName)?.SyntaxNode;

                var partialCoverage = testRunResult.GetCoverage(methodSyntaxNode, testProjectName, compiledTestFixtureInfo.TestDocumentPath);
                coverage.AddRange(partialCoverage);
            }

            return coverage.OrderBy(x => x.TestPath).ToArray();
        }
    }
}