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

        public LineCoverageCalc(ISolutionExplorer solutionExplorer,ICompiler compiler,ITestsExtractor testsExtractor,ITestExecutorScriptEngine testExecutorScriptEngine)
        {
            _solutionExplorer = solutionExplorer;
            _compiler = compiler;
            _testsExtractor = testsExtractor;
            _testExecutorScriptEngine = testExecutorScriptEngine;
        }

        public Dictionary<string, LineCoverage[]> CalculateForAllTests(RewriteResult rewritenResult)
        {
            Assembly[] assemblies = _compiler.Compile(rewritenResult.ToCompilationItems(), rewritenResult.AuditVariablesMap);

            var finalCoverage = new Dictionary<string, List<LineCoverage>>();
   
            foreach (Project project in rewritenResult.Items.Keys)
            {
                MetadataReference[] projectReferences = _solutionExplorer.GetProjectReferences(project);

                foreach (RewrittenItemInfo rewrittenItem in rewritenResult.Items[project])
                {
                    string testDocName = Path.GetFileNameWithoutExtension(rewrittenItem.DocumentPath);

                    foreach (SyntaxNode testClass in _testsExtractor.GetTestClasses(rewrittenItem.SyntaxTree.GetRoot()))
                    {
                        Dictionary<string, List<LineCoverage>> partialCoverage = RunAllTests(projectReferences, testClass, assemblies,
                                                                          rewritenResult.AuditVariablesMap, project.Name, testDocName);

                        MergeCoverage(finalCoverage, partialCoverage);
                    }
                }
            }
            return finalCoverage.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }


        public Dictionary<string, LineCoverage[]> CalculateForDocument(RewrittenDocument rewrittenDocument, Project project)
        {
            var finalCoverage = new Dictionary<string, List<LineCoverage>>();
            var allAssemblies = CompileDocument(project, rewrittenDocument);

            MetadataReference[] projectReferences = _solutionExplorer.GetProjectReferences(project).ToArray();

            SyntaxNode[] testClasses = _testsExtractor.GetTestClasses(rewrittenDocument.SyntaxTree.GetRoot());
            string testDocName = Path.GetFileNameWithoutExtension(rewrittenDocument.DocumentPath);

            foreach (SyntaxNode testClass in testClasses)
            {
                Dictionary<string, List<LineCoverage>> partialCoverage =
                    RunAllTests(projectReferences, testClass, allAssemblies.ToArray(), rewrittenDocument.AuditVariablesMap, project.Name, testDocName);

                MergeCoverage(finalCoverage, partialCoverage);
            }

            return finalCoverage.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        public Dictionary<string, LineCoverage[]> CalculateForTest(RewrittenDocument rewrittenDocument, Project project, string className, string methodName)
        {
            ClassDeclarationSyntax classNode = GetClassNodeByName(rewrittenDocument.SyntaxTree.GetRoot(), className);
            MethodDeclarationSyntax methodNode = GetMethodNodeByName(classNode, methodName);

            var allAssemblies = CompileDocument(project, rewrittenDocument);
            MetadataReference[] projectReferences = _solutionExplorer.GetProjectReferences(project).ToArray();
            var executor = new AppDomainTestExecutorScriptEngine();

            TestRunResult testRunResult = executor.RunTest(projectReferences, allAssemblies,  methodNode, rewrittenDocument.AuditVariablesMap);
            string testDocName = Path.GetFileNameWithoutExtension(rewrittenDocument.DocumentPath);

            var coverageByDocument = new Dictionary<string, List<LineCoverage>>();
            PopulateCoverageFromVariableNames(coverageByDocument, rewrittenDocument.AuditVariablesMap, testRunResult, methodNode, project.Name, testDocName);

            return coverageByDocument.ToDictionary(x => x.Key, x => x.Value.ToArray());
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

        private void MergeCoverage(Dictionary<string, List<LineCoverage>> finalCoverage, Dictionary<string, List<LineCoverage>> partialCoverage)
        {
            foreach (string docPath in partialCoverage.Keys)
            {
                if (!finalCoverage.ContainsKey(docPath))
                    finalCoverage[docPath] = new List<LineCoverage>();

                finalCoverage[docPath].AddRange(partialCoverage[docPath]);
            }
        }

        private MethodDeclarationSyntax GetMethodNodeByName(ClassDeclarationSyntax classNode, string methodName)
        {
            return classNode.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single(d => d.Identifier.Text == methodName);
        }

        private ClassDeclarationSyntax GetClassNodeByName(SyntaxNode root, string className)
        {
            return root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single(d => d.Identifier.Text == className);
        }

        private LineCoverage EvaluateAuditVariable(AuditVariablesMap auditVariablesMap, string variableName, SyntaxNode methodNode, string testProjectName, string testDocName)
        {
            LineCoverage lineCoverage = new LineCoverage
            {
                TestPath = NodePathBuilder.BuildPath(methodNode, testDocName, testProjectName),
                Path = auditVariablesMap.Map[variableName].NodePath,
                Span = auditVariablesMap.Map[variableName].SpanStart
            };

            return lineCoverage;
        }

        private Dictionary<string, List<LineCoverage>> RunAllTests(MetadataReference[] testProjectReferences, SyntaxNode testClass, Assembly[] assemblies, AuditVariablesMap auditVariablesMap, string projectName,string documentName)
        {            
            SyntaxNode[] testMethods = _testsExtractor.GetTestMethods(testClass);
            var coverage = new Dictionary<string, List<LineCoverage>>();

            foreach (SyntaxNode testMethod in testMethods)
            {
                var setVariables = _testExecutorScriptEngine.RunTest(testProjectReferences, assemblies,  testMethod, auditVariablesMap);
                PopulateCoverageFromVariableNames(coverage, auditVariablesMap, setVariables, testMethod, projectName,documentName);
            }

            return coverage;
        }

        private void PopulateCoverageFromVariableNames(Dictionary<string, List<LineCoverage>> coverageByDocument, AuditVariablesMap auditVariablesMap, TestRunResult testRunResult, SyntaxNode testMethod, string testProjectName,string testDocumentName)
        {
            foreach (string varName in testRunResult.SetAuditVars)
            {
                string docPath = auditVariablesMap.Map[varName].DocumentPath;

                LineCoverage lineCoverage = EvaluateAuditVariable(auditVariablesMap, varName, testMethod, testProjectName, testDocumentName);
                if (!testRunResult.AssertionFailed)
                {
                    if (lineCoverage.Path == lineCoverage.TestPath&&varName!= testRunResult.SetAuditVars.Last())
                        lineCoverage.IsSuccess = true;
                    else
                        lineCoverage.IsSuccess = false;
                }
                else
                    lineCoverage.IsSuccess = true;

                if (!coverageByDocument.ContainsKey(docPath))
                    coverageByDocument[docPath] = new List<LineCoverage>();

                coverageByDocument[docPath].Add(lineCoverage);
            }
        }
    }
}
