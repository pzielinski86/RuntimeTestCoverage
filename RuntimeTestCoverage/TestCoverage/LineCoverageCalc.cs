using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.MSBuild;
using TestCoverage.Compilation;

namespace TestCoverage
{
    internal class LineCoverageCalc
    {
        private readonly SolutionExplorer _solutionExplorer;
        public LineCoverageCalc(SolutionExplorer solutionExplorer)
        {
            _solutionExplorer = solutionExplorer;
        }

        public Dictionary<string,LineCoverage[]> CalculateForAllTests(string solutionPath, RewriteResult rewriteResult)
        {
            SyntaxNode[] testClasses = GetTestClasses(rewriteResult);

            var compiler = new Compiler();
            CompiledItem[] compiledItems = compiler.Compile(rewriteResult.ToCompilationItems().ToArray(), rewriteResult.AuditVariablesMap);

            Assembly[] assemblies = compiledItems.Select(c => c.EmitAndSave()).ToArray();

            var coverage = new Dictionary<string,List<LineCoverage>>();
            var allReferences = _solutionExplorer.GetAllReferences().ToArray();

            foreach (SyntaxNode testClass in testClasses)
            {
                var coverageByDocument = RunAllTests(allReferences,testClass, assemblies, rewriteResult.AuditVariablesMap);

                foreach (string docPath in coverageByDocument.Keys)
                {
                    if(!coverage.ContainsKey(docPath))
                        coverage[docPath]=new List<LineCoverage>();

                    coverage[docPath].AddRange(coverageByDocument[docPath]);
                }
            }

            return coverage.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        public Dictionary<string, LineCoverage[]> CalculateForTest(RewrittenDocument rewrittenDocument,Project project, string className, string methodName)
        {
            List<Assembly> assemblies = _solutionExplorer.LoadCompiledAssemblies(project.Name).ToList();
            _solutionExplorer.LoadRewritenAuditNodes(rewrittenDocument.AuditVariablesMap);

            SyntaxTree[] projectTrees = _solutionExplorer.LoadProjectSyntaxTrees(project,rewrittenDocument.DocumentPath).ToArray();

            ClassDeclarationSyntax classNode = rewrittenDocument.SyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single(d => d.Identifier.Text == className);

            MethodDeclarationSyntax methodNode =
                classNode.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Single(d => d.Identifier.Text == methodName);

            var compiler=new Compiler();
            var allProjectTrees = projectTrees.Union(new[] {rewrittenDocument.SyntaxTree}).ToArray();
            CompiledItem[] compiledDocuments = compiler.Compile(new CompilationItem(project, allProjectTrees), assemblies.ToArray(),rewrittenDocument.AuditVariablesMap);
            assemblies.AddRange(compiledDocuments.Select(x => x.EmitAndSave()));

            var executor = new TestExecutorScriptEngine();
            var allReferences = _solutionExplorer.GetAllReferences().ToArray();

            Dictionary<string, bool> setVariables = executor.RunTest(allReferences,assemblies.ToArray(), className, methodNode, rewrittenDocument.AuditVariablesMap);

            Dictionary<string, List<LineCoverage>> coverage=new Dictionary<string, List<LineCoverage>>();

            PopulateCoverageFromVariableNames(coverage,rewrittenDocument.AuditVariablesMap,setVariables.Keys,methodNode);

             return coverage.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        private LineCoverage EvaluateAuditVariable(AuditVariablesMap auditVariablesMap, string variableName,SyntaxNode methodNode)
        {
            LineCoverage lineCoverage = new LineCoverage();
            lineCoverage.TestPath = NodePathBuilder.BuildPath(methodNode);
            lineCoverage.Path = auditVariablesMap.Map[variableName].NodePath;
            lineCoverage.Span = auditVariablesMap.Map[variableName].SpanStart;

            return lineCoverage;
        }

        private Dictionary<string,List<LineCoverage>> RunAllTests(MetadataReference[] allReferences, SyntaxNode testClass, Assembly[] assemblies, AuditVariablesMap auditVariablesMap)
        {
            string className = testClass.ChildTokens().Single(t => t.Kind() == SyntaxKind.IdentifierToken).ValueText;
            SyntaxNode[] testMethods = GetTestMethods(testClass);

            var executor = new TestExecutorScriptEngine();
            var coverage = new Dictionary<string, List<LineCoverage>>();

            foreach (SyntaxNode testMethod in testMethods)
            {
                Dictionary<string, bool> setVariables = executor.RunTest(allReferences,assemblies, className, testMethod,auditVariablesMap);
                PopulateCoverageFromVariableNames(coverage, auditVariablesMap, setVariables.Keys, testMethod);
            }

            return coverage;
        }

        private void PopulateCoverageFromVariableNames(Dictionary<string, List<LineCoverage>> coverageByDocument, AuditVariablesMap auditVariablesMap, IEnumerable<string> variableNames, SyntaxNode testMethod)
        {

            foreach (string varName in variableNames)
            {
                LineCoverage lineCoverage = EvaluateAuditVariable(auditVariablesMap, varName, testMethod);

                string docPath = auditVariablesMap.Map[varName].DocumentPath;
                if (!coverageByDocument.ContainsKey(docPath))
                    coverageByDocument[docPath] = new List<LineCoverage>();

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

        private static SyntaxNode[] GetTestClasses(RewriteResult rewriteResult)
        {
            IEnumerable<SyntaxNode> allNodes = rewriteResult.Items.Values.SelectMany(x => x).Select(i => i.SyntaxTree.GetRoot());

            return allNodes.SelectMany(
                        t => t.DescendantNodes()
                                .OfType<AttributeSyntax>()
                                .Where(a => a.Name.ToString() == "TestFixture")
                                .Select(a => a.Parent.Parent)).ToArray();
        }
    }
}
