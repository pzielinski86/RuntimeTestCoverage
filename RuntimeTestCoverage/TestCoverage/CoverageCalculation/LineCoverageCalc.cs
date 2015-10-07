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
using TestCoverage.Storage;

namespace TestCoverage.CoverageCalculation
{
    public class LineCoverageCalc
    {
        private readonly ISolutionExplorer _solutionExplorer;
        private readonly ICompiler _compiler;
        private readonly ICoverageStore _coverageStore;
        private readonly ITestsExtractor _testsExtractor;
        private readonly ITestExecutorScriptEngine _testExecutorScriptEngine;

        public LineCoverageCalc(ISolutionExplorer solutionExplorer,
            ICompiler compiler,
            ICoverageStore coverageStore,
            ITestsExtractor testsExtractor,
            ITestExecutorScriptEngine testExecutorScriptEngine)
        {
            _solutionExplorer = solutionExplorer;
            _compiler = compiler;
            _coverageStore = coverageStore;
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

                    foreach (ClassDeclarationSyntax testClass in _testsExtractor.GetTestClasses(rewrittenItem.SyntaxTree.GetRoot()))
                    {
                        var partialCoverage = RunAllTests(projectReferences, testClass, assemblies,
                                                                           rewritenResult.AuditVariablesMap, project.Name, rewrittenItem.DocumentPath);

                        finalCoverage.AddRange(partialCoverage);
                    }
                }
            }
            return finalCoverage.ToArray();
        }

        //TODO: refactor
        public LineCoverage[] CalculateForDocument(RewrittenDocument rewrittenDocument, Project project)
        {
            var finalCoverage = new List<LineCoverage>();
            var allAssemblies = CompileDocument(project, rewrittenDocument);
            

            string docName = Path.GetFileNameWithoutExtension(rewrittenDocument.DocumentPath);
            ClassDeclarationSyntax[] allTestClasses = _testsExtractor.GetTestClasses(rewrittenDocument.SyntaxTree.GetRoot());
            var testClassesByDocument=new Dictionary<string,ClassDeclarationSyntax[]>();

            if (allTestClasses.Length == 0)
                testClassesByDocument = GetReferencedTests(rewrittenDocument.SyntaxTree.GetRoot(), docName, project.Name);
            else
                testClassesByDocument[docName] = allTestClasses;

            foreach (var testDocument in testClassesByDocument)
            {
                var testProject = _solutionExplorer.GetProjectByDocument(testDocument.Key);

                MetadataReference[] projectReferences = _solutionExplorer.GetProjectReferences(testProject).ToArray();

                foreach (var classDeclarationSyntax in testDocument.Value)
                {
                    var partialCoverage =
                    RunAllTests(projectReferences, classDeclarationSyntax, allAssemblies.ToArray(), rewrittenDocument.AuditVariablesMap, project.Name, testDocument.Key);
                    
                    finalCoverage.AddRange(partialCoverage);
                }                
            }

            return finalCoverage.ToArray();
        }

        // TODO: Tests
        private Dictionary<string, ClassDeclarationSyntax[]> GetReferencedTests(SyntaxNode root, string documentName, string projectName)
        {
            // TODO: Make it to read only public methods
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var currentCoverage = _coverageStore.ReadAll();
            var allTestClasses = new Dictionary<string, ClassDeclarationSyntax[]>();

            foreach (var method in methods) 
            {
                string path = NodePathBuilder.BuildPath(method, documentName, projectName);

                foreach (var docCoverage in currentCoverage.Where(x => x.Path == path))
                {
                    if (!allTestClasses.ContainsKey(docCoverage.TestDocumentPath))
                    {
                        string testDocumentContent = File.ReadAllText(docCoverage.TestDocumentPath);
                        var testRoot = CSharpSyntaxTree.ParseText(testDocumentContent);
                        var testClasses = testRoot.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

                        allTestClasses[docCoverage.TestDocumentPath] = testClasses.ToArray();
                    }
                }
            }

            return allTestClasses;
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

        private LineCoverage[] RunAllTests(MetadataReference[] testProjectReferences, ClassDeclarationSyntax testClass, Assembly[] assemblies, AuditVariablesMap auditVariablesMap, string projectName, string testDocPath)
        {
            TestCase[] testCases = _testsExtractor.GetTestCases(testClass);
            var coverage = new List<LineCoverage>();

            foreach (TestCase testCase in testCases)
            {
                var setVariables = _testExecutorScriptEngine.RunTest(testProjectReferences, assemblies, testCase, auditVariablesMap);
                var partialCoverage = GetCoverageFromVariableNames(auditVariablesMap, setVariables, testCase, projectName, testDocPath);
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
                lineCoverage.TestDocumentPath = testDocumentPath;

                coverage.Add(lineCoverage);
            }

            return coverage.ToArray();
        }
    }
}
