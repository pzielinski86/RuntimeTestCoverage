﻿using System.Collections.Generic;
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
            SyntaxNode[] testClasses = GetTestClasses(rewritenResult);

            var compiler = new Compiler();
            Assembly[] assemblies = compiler.Compile(rewritenResult.ToCompilationItems().ToArray(), rewritenResult.AuditVariablesMap);

            var coverage = new Dictionary<string, List<CoverageCalculation.LineCoverage>>();
            MetadataReference[] allReferences = _solutionExplorer.GetAllReferences().ToArray();

            foreach (SyntaxNode testClass in testClasses)
            {
                var coverageByDocument = RunAllTests(allReferences, testClass, assemblies, rewritenResult.AuditVariablesMap);

                foreach (string docPath in coverageByDocument.Keys)
                {
                    if (!coverage.ContainsKey(docPath))
                        coverage[docPath] = new List<CoverageCalculation.LineCoverage>();

                    coverage[docPath].AddRange(coverageByDocument[docPath]);
                }
            }

            return coverage.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        public Dictionary<string, CoverageCalculation.LineCoverage[]> CalculateForTest(RewrittenDocument rewrittenDocument, Project project, string className, string methodName)
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

            PopulateCoverageFromVariableNames(coverageByDocument, rewrittenDocument.AuditVariablesMap, setVariables.Keys, methodNode);

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

        private CoverageCalculation.LineCoverage EvaluateAuditVariable(AuditVariablesMap auditVariablesMap, string variableName, SyntaxNode methodNode)
        {
            CoverageCalculation.LineCoverage lineCoverage = new CoverageCalculation.LineCoverage
            {
                TestPath = NodePathBuilder.BuildPath(methodNode),
                Path = auditVariablesMap.Map[variableName].NodePath,
                Span = auditVariablesMap.Map[variableName].SpanStart
            };

            return lineCoverage;
        }

        private Dictionary<string, List<CoverageCalculation.LineCoverage>> RunAllTests(MetadataReference[] allReferences, SyntaxNode testClass, Assembly[] assemblies, AuditVariablesMap auditVariablesMap)
        {
            string className = testClass.ChildTokens().Single(t => t.Kind() == SyntaxKind.IdentifierToken).ValueText;
            SyntaxNode[] testMethods = GetTestMethods(testClass);

            var executor = new TestExecutorScriptEngine();
            var coverage = new Dictionary<string, List<CoverageCalculation.LineCoverage>>();

            foreach (SyntaxNode testMethod in testMethods)
            {
                Dictionary<string, bool> setVariables = executor.RunTest(allReferences, assemblies, className, testMethod, auditVariablesMap);
                PopulateCoverageFromVariableNames(coverage, auditVariablesMap, setVariables.Keys, testMethod);
            }

            return coverage;
        }

        private void PopulateCoverageFromVariableNames(Dictionary<string, List<CoverageCalculation.LineCoverage>> coverageByDocument, AuditVariablesMap auditVariablesMap, IEnumerable<string> variableNames, SyntaxNode testMethod)
        {
            foreach (string varName in variableNames)
            {
                CoverageCalculation.LineCoverage lineCoverage = EvaluateAuditVariable(auditVariablesMap, varName, testMethod);

                string docPath = auditVariablesMap.Map[varName].DocumentPath;
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
