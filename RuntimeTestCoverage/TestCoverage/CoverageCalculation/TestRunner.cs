using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    internal class TestRunner : ITestRunner
    {
        private readonly ITestsExtractor _testsExtractor;
        private readonly ITestExecutorScriptEngine _testExecutorScriptEngine;
        private readonly ISolutionExplorer _solutionExplorer;

        public TestRunner(ITestsExtractor testsExtractor,ITestExecutorScriptEngine testExecutorScriptEngine,ISolutionExplorer solutionExplorer)
        {
            _testsExtractor = testsExtractor;
            _testExecutorScriptEngine = testExecutorScriptEngine;
            _solutionExplorer = solutionExplorer;
        }

        public LineCoverage[] RunAllTestsInDocument(RewrittenDocument rewrittenDocument, 
            ISemanticModel semanticModel, 
            Project project,
            Assembly[] allAssemblies)
        {
            var allReferences = _solutionExplorer.GetAllProjectReferences(project.Name);

            var compiledTestInfo = new CompiledTestFixtureInfo
            {
                TestProjectReferences = allReferences,
                TestDocumentPath = rewrittenDocument.DocumentPath,
                AllAssemblies = allAssemblies,
                AuditVariablesMap = rewrittenDocument.AuditVariablesMap,
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

        private LineCoverage[] RunAllTestsInFixture(CompiledTestFixtureInfo compiledTestFixtureInfo,string testProjectName)
        {
            TestFixtureDetails testFixtureDetails = _testsExtractor.GetTestFixtureDetails(compiledTestFixtureInfo.TestClass, compiledTestFixtureInfo.SemanticModel);
            string testsProjectName = PathHelper.GetCoverageDllName(testProjectName);

            var coverage = new List<LineCoverage>();

            foreach (TestCase testCase in testFixtureDetails.Cases)
            {
                ITestRunResult testResult = _testExecutorScriptEngine.RunTest(compiledTestFixtureInfo.TestProjectReferences, compiledTestFixtureInfo.AllAssemblies, testCase, compiledTestFixtureInfo.AuditVariablesMap);

                var partialCoverage = testResult.GetCoverage(compiledTestFixtureInfo.AuditVariablesMap, testCase.SyntaxNode, testsProjectName, compiledTestFixtureInfo.TestDocumentPath);

                coverage.AddRange(partialCoverage);
            }

            return coverage.ToArray();
        }
    }
}