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

        public TestRunner(ITestsExtractor testsExtractor,ITestExecutorScriptEngine testExecutorScriptEngine)
        {
            _testsExtractor = testsExtractor;
            _testExecutorScriptEngine = testExecutorScriptEngine;
        }

        public LineCoverage[] RunAllTestsInDocument(RewrittenDocument rewrittenDocument, CompiledItem compiledTestProject, Assembly[] allAssemblies)
        { 
            var compiledTestInfo = new CompiledTestFixtureInfo();
            compiledTestInfo.TestProjectReferences = compiledTestProject.Project.MetadataReferences.ToArray();
            compiledTestInfo.TestDocumentPath = rewrittenDocument.DocumentPath;
            compiledTestInfo.AllAssemblies = allAssemblies;
            compiledTestInfo.AuditVariablesMap = rewrittenDocument.AuditVariablesMap;
            compiledTestInfo.TestProjectCompilationItem = compiledTestProject;

            var coverage = new List<LineCoverage>();

            var testClasses = _testsExtractor.GetTestClasses(rewrittenDocument.SyntaxTree.GetRoot());

            if (testClasses.Length == 0)
                return null;

            foreach (ClassDeclarationSyntax testClass in testClasses)
            {                
                compiledTestInfo.TestClass = testClass;

                var partialCoverage = RunAllTestsInFixture(compiledTestInfo);
                coverage.AddRange(partialCoverage);
            }

            return coverage.ToArray();
        }

        public LineCoverage[] RunAllTestsInFixture(CompiledTestFixtureInfo compiledTestFixtureInfo)
        {
            var semanticModel =
                compiledTestFixtureInfo.TestProjectCompilationItem.GetSemanticModel(
                    compiledTestFixtureInfo.TestClass.SyntaxTree);

            TestFixtureDetails testFixtureDetails = _testsExtractor.GetTestFixtureDetails(compiledTestFixtureInfo.TestClass, semanticModel);

            string testsProjectName = PathHelper.GetCoverageDllName(compiledTestFixtureInfo.TestProjectCompilationItem.Project.Name);
            testFixtureDetails.AssemblyName = compiledTestFixtureInfo.AllAssemblies.Single(x => x.GetName().Name == testsProjectName).FullName;

            var coverage = new List<LineCoverage>();

            foreach (TestCase testCase in testFixtureDetails.Cases)
            {
                var setVariables = _testExecutorScriptEngine.RunTest(compiledTestFixtureInfo.TestProjectReferences, compiledTestFixtureInfo.AllAssemblies, testCase, compiledTestFixtureInfo.AuditVariablesMap);
                var partialCoverage = GetCoverageFromVariableNames(compiledTestFixtureInfo.AuditVariablesMap, setVariables, testCase, testsProjectName, compiledTestFixtureInfo.TestDocumentPath);
                coverage.AddRange(partialCoverage);
            }

            return coverage.ToArray();
        }

        private LineCoverage[] GetCoverageFromVariableNames(AuditVariablesMap auditVariablesMap, TestRunResult testRunResult, TestCase testMethod, string testProjectName, string testDocumentPath)
        {
            List<LineCoverage> coverage = new List<LineCoverage>();
            string testDocName = Path.GetFileNameWithoutExtension(testDocumentPath);

            foreach (string varName in testRunResult.SetAuditVars)
            {
                string docPath = auditVariablesMap.Map[varName].DocumentPath;

                LineCoverage lineCoverage = EvaluateAuditVariable(auditVariablesMap, varName, testMethod, testProjectName, testDocName);
                if (testRunResult.AssertionFailed)
                {
                    if (lineCoverage.Path == lineCoverage.TestPath && varName != testRunResult.SetAuditVars.Last())
                        lineCoverage.IsSuccess = true;
                    else
                        lineCoverage.IsSuccess = false;
                }
                else
                    lineCoverage.IsSuccess = true;


                lineCoverage.DocumentPath = docPath;
                lineCoverage.TestDocumentPath = testDocumentPath;

                coverage.Add(lineCoverage);
            }

            return coverage.ToArray();
        }

        private LineCoverage EvaluateAuditVariable(AuditVariablesMap auditVariablesMap, string variableName, TestCase testCase, string testProjectName, string testDocName)
        {
            LineCoverage lineCoverage = new LineCoverage
            {
                TestPath = NodePathBuilder.BuildPath(testCase.SyntaxNode, testDocName, testProjectName),
                Path = auditVariablesMap.Map[variableName].NodePath,
                Span = auditVariablesMap.Map[variableName].SpanStart
            };

            return lineCoverage;
        }
    }
}