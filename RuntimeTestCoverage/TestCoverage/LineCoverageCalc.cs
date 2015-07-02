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
using TestCoverageSandbox;

namespace TestCoverage
{
    internal class LineCoverageCalc
    {
        private readonly SolutionExplorer _solutionExplorer;
        public LineCoverageCalc(SolutionExplorer solutionExplorer)
        {
            _solutionExplorer = solutionExplorer;
        }

        public LineCoverage[] CalculateForAllTests(string solutionPath, RewriteResult rewriteResult)
        {
            SyntaxNode[] testClasses = GetTestClasses(rewriteResult);

            var compiler = new Compiler();
            CompiledItem[] compiledItems = compiler.Compile(rewriteResult.ToCompilationItems().ToArray(), rewriteResult.AuditVariablesMap);

            Assembly[] assemblies = compiledItems.Select(c => c.EmitAndSave()).ToArray();

            var coverage = new List<LineCoverage>();
            var allReferences = _solutionExplorer.GetAllReferences().ToArray();

            foreach (SyntaxNode testClass in testClasses)
            {
                LineCoverage[] lineCoverage = RunAllTests(allReferences,testClass, assemblies, rewriteResult.AuditVariablesMap);

                coverage.AddRange(lineCoverage);
            }

            return coverage.ToArray();
        }

        public LineCoverage[] CalculateForTest(RewrittenDocument rewrittenDocument, string className, string methodName)
        {
            Assembly[] assemblies = _solutionExplorer.LoadCompiledAssemblies();
            _solutionExplorer.LoadRewritenAuditNodes(rewrittenDocument.AuditVariablesMap);

            ClassDeclarationSyntax classNode = rewrittenDocument.SyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single(d => d.Identifier.Text == className);

            MethodDeclarationSyntax methodNode =
                classNode.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Single(d => d.Identifier.Text == methodName);

            var executor = new TestExecutorScriptEngine();
            var allReferences = _solutionExplorer.GetAllReferences().ToArray();

            Dictionary<string, bool> setVariables = executor.RunTest(allReferences,assemblies, className, methodNode, rewrittenDocument.AuditVariablesMap);

            var coverage =
                setVariables.Keys.Select(
                    name => EvaluateAuditVariable(rewrittenDocument.AuditVariablesMap, name, methodNode)).ToArray();

            return coverage;
        }

        private LineCoverage EvaluateAuditVariable(AuditVariablesMap auditVariablesMap, string variableName,SyntaxNode methodNode)
        {
            LineCoverage lineCoverage = new LineCoverage();
            lineCoverage.TestPath = GetPath(methodNode);
            lineCoverage.Path = ExtractPathFromVariableName(variableName);
            lineCoverage.Span = auditVariablesMap.Map[variableName];

            return lineCoverage;
        }

        private string ExtractPathFromVariableName(string varName)
        {
            for (int i = varName.Length - 1; i >= 0; i--)
            {
                if (varName[i] == '_')
                {
                    return varName.Substring(0, i);
                }
            }

            throw new ArgumentException("Passed argument is not audit variable.");
        }


        private LineCoverage[] RunAllTests(MetadataReference[] allReferences, SyntaxNode testClass, Assembly[] assemblies, AuditVariablesMap auditVariablesMap)
        {
            string className = testClass.ChildTokens().Single(t => t.Kind() == SyntaxKind.IdentifierToken).ValueText;
            SyntaxNode[] testMethods = GetTestMethods(testClass);

            var executor = new TestExecutorScriptEngine();
            var coverage = new List<LineCoverage>();

            foreach (SyntaxNode testMethod in testMethods)
            {
                Dictionary<string, bool> setVariables = executor.RunTest(allReferences,assemblies, className, testMethod,auditVariablesMap);
       
                var methodCoverage =
                setVariables.Keys.Select(
                    name => EvaluateAuditVariable(auditVariablesMap, name, testMethod)).ToArray();

                coverage.AddRange(methodCoverage);

            }
            return coverage.ToArray();
        }

        private string GetPath(SyntaxNode node)
        {
            var parent = node;
            StringBuilder path = new StringBuilder();

            while (parent != null)
            {
                if (parent is MethodDeclarationSyntax)
                    path.Insert(0, ((MethodDeclarationSyntax)parent).Identifier.Text + ".");

                if (parent is ClassDeclarationSyntax)
                    path.Insert(0, ((ClassDeclarationSyntax)parent).Identifier.Text + ".");

                if (parent is NamespaceDeclarationSyntax)
                    path.Insert(0, ((NamespaceDeclarationSyntax)parent).Name + ".");

                parent = parent.Parent;
            }
            return path.ToString();
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
