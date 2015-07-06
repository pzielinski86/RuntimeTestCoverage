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
    internal class LineCoverageCalc
    {
        private readonly SolutionExplorer _solutionExplorer;
        public LineCoverageCalc(SolutionExplorer solutionExplorer)
        {
            _solutionExplorer = solutionExplorer;
        }

        public Dictionary<string, CoverageCalculation.LineCoverage[]> CalculateForAllTests(string solutionPath, RewriteResult rewritenResult)
        {
            Dictionary<Project, SyntaxNode[]> testClassesByProject = GetTestClasses(rewritenResult);

            var compiler = new Compiler();
            Assembly[] assemblies = compiler.Compile(rewritenResult.ToCompilationItems().ToArray(), rewritenResult.AuditVariablesMap);

            var coverage = new Dictionary<string, List<CoverageCalculation.LineCoverage>>();
            MetadataReference[] allReferences = _solutionExplorer.GetAllReferences().ToArray();

            foreach (Project project in testClassesByProject.Keys)
            {
                foreach (SyntaxNode testClass in testClassesByProject[project])
                {
                    var coverageByDocument = RunAllTests(allReferences, testClass, assemblies,
                        rewritenResult.AuditVariablesMap, project.Name);

                    foreach (string docPath in coverageByDocument.Keys)
                    {
                        if (!coverage.ContainsKey(docPath))
                            coverage[docPath] = new List<CoverageCalculation.LineCoverage>();

                        coverage[docPath].AddRange(coverageByDocument[docPath]);
                    }
                }
            }
            return coverage.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        public Dictionary<string, LineCoverage[]> CalculateForDocument(RewrittenDocument rewrittenDocument, Project project)
        {
            List<Assembly> assemblies = _solutionExplorer.LoadCompiledAssemblies(project.Name).ToList();
            var coverage = new Dictionary<string, List<CoverageCalculation.LineCoverage>>();
            _solutionExplorer.PopulateWithRewrittenAuditNodes(rewrittenDocument.AuditVariablesMap);

            SyntaxTree[] projectTrees = _solutionExplorer.LoadProjectSyntaxTrees(project, rewrittenDocument.DocumentPath).ToArray();
            SyntaxNode root = rewrittenDocument.SyntaxTree.GetRoot();
                        
            var compiler = new Compiler();
            var allProjectTrees = projectTrees.Union(new[] { rewrittenDocument.SyntaxTree }).ToArray();
            Assembly[] documentAssemblies = compiler.Compile(new CompilationItem(project, allProjectTrees), assemblies.ToArray(), rewrittenDocument.AuditVariablesMap);
            assemblies.AddRange(documentAssemblies);

            var executor = new TestExecutorScriptEngine();
            var allReferences = _solutionExplorer.GetAllReferences().ToArray();
            SyntaxNode[] testClasses = GetTestClasses(root);

            foreach (SyntaxNode testClass in testClasses)
            {
                var coverageByDocument = RunAllTests(allReferences, testClass, assemblies.ToArray(), rewrittenDocument.AuditVariablesMap, project.Name);

                foreach (string docPath in coverageByDocument.Keys)
                {
                    if (!coverage.ContainsKey(docPath))
                        coverage[docPath] = new List<CoverageCalculation.LineCoverage>();

                    coverage[docPath].AddRange(coverageByDocument[docPath]);
                }
            }

            return coverage.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        public Dictionary<string, LineCoverage[]> CalculateForTest(RewrittenDocument rewrittenDocument, Project project, string className, string methodName)
        {
            List<Assembly> assemblies = _solutionExplorer.LoadCompiledAssemblies(project.Name).ToList();
            _solutionExplorer.PopulateWithRewrittenAuditNodes(rewrittenDocument.AuditVariablesMap);

            SyntaxTree[] projectTrees = _solutionExplorer.LoadProjectSyntaxTrees(project, rewrittenDocument.DocumentPath).ToArray();

            ClassDeclarationSyntax classNode = GetClassNodeByName(rewrittenDocument.SyntaxTree.GetRoot(), className);
            MethodDeclarationSyntax methodNode = GetMethodNodeByName(classNode, methodName);

            var compiler = new Compiler();
            var allProjectTrees = projectTrees.Union(new[] { rewrittenDocument.SyntaxTree }).ToArray();
            Assembly[] documentAssemblies = compiler.Compile(new CompilationItem(project, allProjectTrees), assemblies.ToArray(), rewrittenDocument.AuditVariablesMap);
            assemblies.AddRange(documentAssemblies);

            var executor = new TestExecutorScriptEngine();
            var allReferences = _solutionExplorer.GetAllReferences().ToArray();

            Dictionary<string, bool> setVariables = executor.RunTest(allReferences, assemblies.ToArray(), className, methodNode, rewrittenDocument.AuditVariablesMap);

            var coverageByDocument = new Dictionary<string, List<CoverageCalculation.LineCoverage>>();

            PopulateCoverageFromVariableNames(coverageByDocument, rewrittenDocument.AuditVariablesMap, setVariables.Keys, methodNode,project.Name);

            return coverageByDocument.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        private static MethodDeclarationSyntax GetMethodNodeByName(ClassDeclarationSyntax classNode, string methodName)
        {
            return classNode.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single(d => d.Identifier.Text == methodName);
        }

        private static ClassDeclarationSyntax GetClassNodeByName(SyntaxNode root, string className)
        {
            return root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single(d => d.Identifier.Text == className);
        }

        private CoverageCalculation.LineCoverage EvaluateAuditVariable(AuditVariablesMap auditVariablesMap, string variableName, SyntaxNode methodNode,string projectName,string documentName)
        {
            CoverageCalculation.LineCoverage lineCoverage = new CoverageCalculation.LineCoverage
            {
                TestPath = NodePathBuilder.BuildPath(methodNode,projectName,documentName),
                Path = auditVariablesMap.Map[variableName].NodePath,
                Span = auditVariablesMap.Map[variableName].SpanStart
            };

            return lineCoverage;
        }

        private Dictionary<string, List<LineCoverage>> RunAllTests(MetadataReference[] allReferences, SyntaxNode testClass, Assembly[] assemblies, AuditVariablesMap auditVariablesMap, string projectName)
        {
            string className = testClass.ChildTokens().Single(t => t.Kind() == SyntaxKind.IdentifierToken).ValueText;
            SyntaxNode[] testMethods = GetTestMethods(testClass);

            var executor = new TestExecutorScriptEngine();
            var coverage = new Dictionary<string, List<CoverageCalculation.LineCoverage>>();

            foreach (SyntaxNode testMethod in testMethods)
            {
                Dictionary<string, bool> setVariables = executor.RunTest(allReferences, assemblies, className, testMethod, auditVariablesMap);
                PopulateCoverageFromVariableNames(coverage, auditVariablesMap, setVariables.Keys, testMethod,projectName);
            }

            return coverage;
        }

        private void PopulateCoverageFromVariableNames(Dictionary<string, List<LineCoverage>> coverageByDocument, AuditVariablesMap auditVariablesMap, IEnumerable<string> variableNames, SyntaxNode testMethod, string projectName)
        {
            foreach (string varName in variableNames)
            {
                string docPath = auditVariablesMap.Map[varName].DocumentPath;
                string documentName = Path.GetFileNameWithoutExtension(docPath);

                CoverageCalculation.LineCoverage lineCoverage = EvaluateAuditVariable(auditVariablesMap, varName, testMethod, projectName,documentName);
               
                if (!coverageByDocument.ContainsKey(docPath))
                    coverageByDocument[docPath] = new List<CoverageCalculation.LineCoverage>();

                coverageByDocument[docPath].Add(lineCoverage);
            }
        }

        private static SyntaxNode[] GetTestMethods(SyntaxNode testClass)
        {
            var testMethods = testClass.DescendantNodes()
                .OfType<AttributeSyntax>()
                .Where(a => a.Name.ToString() == "Test")
                .Select(a => a.Parent.Parent).ToArray();

            return testMethods;
        }

        private static Dictionary<Project,SyntaxNode[]> GetTestClasses(RewriteResult rewriteResult)
        {
            return rewriteResult.Items.ToDictionary(i => i.Key, i => i.Value.SelectMany(x=>GetTestClasses(x.SyntaxTree.GetRoot())).ToArray());
        }

        private static SyntaxNode[] GetTestClasses(SyntaxNode root)
        {
            return root.DescendantNodes().OfType<AttributeSyntax>()
                                .Where(a => a.Name.ToString() == "TestFixture")
                                .Select(a => a.Parent.Parent).ToArray();
        }
    }
}
