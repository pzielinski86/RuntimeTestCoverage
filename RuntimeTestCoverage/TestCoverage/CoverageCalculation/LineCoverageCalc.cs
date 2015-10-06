using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public class LineCoverageCalc
    {
        private readonly ISolutionExplorer _solutionExplorer;
        private readonly ICompiler _compiler;
        private readonly ITestsExtractor _testsExtractor;
        private readonly ITestExecutorScriptEngine _testExecutorScriptEngine;

        public LineCoverageCalc(ISolutionExplorer solutionExplorer, ICompiler compiler, ITestsExtractor testsExtractor, ITestExecutorScriptEngine testExecutorScriptEngine)
        {
            _solutionExplorer = solutionExplorer;
            _compiler = compiler;
            _testsExtractor = testsExtractor;
            _testExecutorScriptEngine = testExecutorScriptEngine;
        }

        public LineCoverage[] CalculateForAllTests(RewriteResult rewritenResult)
        {
            Assembly[] assemblies = _compiler.Compile(rewritenResult.ToCompilationItems(), rewritenResult.AuditVariablesMap);

            var finalCoverage = new List<LineCoverage>();

            foreach (Project project in rewritenResult.Items.Keys)
            {
                MetadataReference[] projectReferences = _solutionExplorer.GetProjectReferences(project);

                foreach (RewrittenItemInfo rewrittenItem in rewritenResult.Items[project])
                {
                    string testDocName = Path.GetFileNameWithoutExtension(rewrittenItem.DocumentPath);

                    foreach (ClassDeclarationSyntax testClass in _testsExtractor.GetTestClasses(rewrittenItem.SyntaxTree.GetRoot()))
                    {
                        var partialCoverage = RunAllTests(projectReferences, testClass, assemblies,
                                                                           rewritenResult.AuditVariablesMap, project.Name, testDocName);

                        finalCoverage.AddRange(partialCoverage);
                    }
                }
            }
            return finalCoverage.ToArray();
        }


        public LineCoverage[] CalculateForDocument(RewrittenDocument rewrittenDocument, Project project)
        {
            var finalCoverage = new List<LineCoverage>();
            var allAssemblies = CompileDocument(project, rewrittenDocument);

            MetadataReference[] projectReferences = _solutionExplorer.GetProjectReferences(project).ToArray();

            ClassDeclarationSyntax[] testClasses = _testsExtractor.GetTestClasses(rewrittenDocument.SyntaxTree.GetRoot());
            string testDocName = Path.GetFileNameWithoutExtension(rewrittenDocument.DocumentPath);

            foreach (ClassDeclarationSyntax testClass in testClasses)
            {
                var partialCoverage =
                    RunAllTests(projectReferences, testClass, allAssemblies.ToArray(), rewrittenDocument.AuditVariablesMap, project.Name, testDocName);

                finalCoverage.AddRange(partialCoverage);
            }

            return finalCoverage.ToArray();
        }

        private Assembly[] CompileDocument(Project project, RewrittenDocument rewrittenDocument)
        {
            List<Assembly> assemblies = _solutionExplorer.LoadCompiledAssemblies(project.Name).ToList();

            _solutionExplorer.PopulateWithRewrittenAuditNodes(rewrittenDocument.AuditVariablesMap);

            SyntaxTree[] projectTrees = _solutionExplorer.LoadProjectSyntaxTrees(project, rewrittenDocument.DocumentPath).ToArray();

            var compiler = new RoslynCompiler();
            var allProjectTrees = projectTrees.Union(new[] { rewrittenDocument.SyntaxTree }).ToArray();
            Assembly[] documentAssemblies = compiler.Compile(new CompilationItem(project, allProjectTrees), assemblies, rewrittenDocument.AuditVariablesMap);
            assemblies.AddRange(documentAssemblies);

            return assemblies.ToArray();
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

        private LineCoverage[] RunAllTests(MetadataReference[] testProjectReferences, ClassDeclarationSyntax testClass, Assembly[] assemblies, AuditVariablesMap auditVariablesMap, string projectName, string documentName)
        {
            TestCase[] testCases = _testsExtractor.GetTestCases(testClass);
            var coverage = new List<LineCoverage>();

            foreach (TestCase testCase in testCases)
            {
                var setVariables = _testExecutorScriptEngine.RunTest(testProjectReferences, assemblies, testCase, auditVariablesMap);
                var partialCoverage = GetCoverageFromVariableNames( auditVariablesMap, setVariables, testCase, projectName, documentName);
                coverage.AddRange(partialCoverage);
            }

            return coverage.ToArray();
        }

        private LineCoverage[] GetCoverageFromVariableNames(AuditVariablesMap auditVariablesMap, TestRunResult testRunResult, TestCase testMethod, string testProjectName, string testDocumentName)
        {
            List<LineCoverage> coverage = new List<LineCoverage>();

            foreach (string varName in testRunResult.SetAuditVars)
            {
                string docPath = auditVariablesMap.Map[varName].DocumentPath;

                LineCoverage lineCoverage = EvaluateAuditVariable(auditVariablesMap, varName, testMethod, testProjectName, testDocumentName);
                if (!testRunResult.AssertionFailed)
                {
                    if (lineCoverage.Path == lineCoverage.TestPath && varName != testRunResult.SetAuditVars.Last())
                        lineCoverage.IsSuccess = true;
                    else
                        lineCoverage.IsSuccess = false;
                }
                else
                    lineCoverage.IsSuccess = true;


                lineCoverage.DocumentPath = docPath;
                coverage.Add(lineCoverage);
            }

            return coverage.ToArray();
        }
    }
}
