﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public class LineCoverageCalc
    {
        private readonly ITestExplorer _testExplorer;
        private readonly ICompiler _compiler;
        private readonly ITestRunner _testRunner;

        public LineCoverageCalc(ITestExplorer testExplorer,
            ICompiler compiler,
            ITestRunner testRunner)
        {
            _testExplorer = testExplorer;
            _compiler = compiler;
            _testRunner = testRunner;
        }

        public LineCoverage[] CalculateForAllTests(RewriteResult rewritenResult)
        {
            var compiledItems = _compiler.Compile(rewritenResult.ToCompilationItems());
            var allAssemblies = compiledItems.Select(x => x.DllPath).ToArray();

            var finalCoverage = new List<LineCoverage>();

            foreach (Project project in rewritenResult.Items.Keys)
            {
                foreach (RewrittenDocument rewrittenItem in rewritenResult.Items[project])
                {
                    if (!rewrittenItem.ContainsTest)
                        continue;

                    var testProjectCompiltedItem = compiledItems.Single(x => x.Project == project);
                    ISemanticModel semanticModel = testProjectCompiltedItem.GetSemanticModel(rewrittenItem.SyntaxTree);

                    LineCoverage[] partialCoverage = _testRunner.RunAllTestsInDocument(rewrittenItem,
                        semanticModel,
                        project,
                        allAssemblies);

                    if (partialCoverage != null)
                        finalCoverage.AddRange(partialCoverage);
                }
            }
            return finalCoverage.ToArray();
        }

        public LineCoverage[] CalculateForMethod(Project project, RewrittenDocument rewrittenDocument, MethodDeclarationSyntax method)
        {
            ICompiledItem[] newCompiledItems;

            string[] rewrittenAssemblies = CompileDocument(project, rewrittenDocument, out newCompiledItems);

            ISemanticModel semanticModel = newCompiledItems[0].GetSemanticModel(rewrittenDocument.SyntaxTree);
            LineCoverage[] fullCoverage = _testRunner.RunTest(project,
                rewrittenDocument,
                method,
                semanticModel, rewrittenAssemblies);

            if (fullCoverage == null)
                fullCoverage = CalculateCoverageForReferencedTests(project, rewrittenDocument, rewrittenAssemblies);

            return fullCoverage.ToArray();
        }

        public LineCoverage[] CalculateForDocument(Project project, RewrittenDocument rewrittenDocument)
        {
            ICompiledItem[] newCompiledItems;

            string[] allAssemblies = CompileDocument(project, rewrittenDocument, out newCompiledItems);

            ISemanticModel semanticModel = newCompiledItems[0].GetSemanticModel(rewrittenDocument.SyntaxTree);
            LineCoverage[] fullCoverage = _testRunner.RunAllTestsInDocument(rewrittenDocument, semanticModel, project, allAssemblies);

            if (fullCoverage == null)
                fullCoverage = CalculateCoverageForReferencedTests(project, rewrittenDocument, allAssemblies);

            return fullCoverage.ToArray();
        }

        private LineCoverage[] CalculateCoverageForReferencedTests(Project project,
            RewrittenDocument rewrittenDocument,
            string[] allAssemblies)
        {
            List<LineCoverage> finalCoverage = new List<LineCoverage>();
            var referencedTests = _testExplorer.GetReferencedTests(rewrittenDocument, project.Name);

            foreach (RewrittenDocument referencedTest in referencedTests)
            {
                var semanticModel = _testExplorer.SolutionExplorer.GetSemanticModelByDocument(referencedTest.DocumentPath);
                var testProject = _testExplorer.SolutionExplorer.GetProjectByDocument(referencedTest.DocumentPath);

                var coverage = _testRunner.RunAllTestsInDocument(referencedTest, semanticModel, testProject, allAssemblies);
                finalCoverage.AddRange(coverage);
            }

            return finalCoverage.ToArray();
        }

        private string[] CompileDocument(Project project, RewrittenDocument rewrittenDocument, out ICompiledItem[] newItems)
        {
            List<string> assemblies = _testExplorer.SolutionExplorer.GetCompiledAssemblies(project.Name).ToList();

            SyntaxTree[] projectTrees = _testExplorer.SolutionExplorer.LoadRewrittenProjectSyntaxTrees(project, rewrittenDocument.DocumentPath).ToArray();

            var allProjectTrees = projectTrees.Union(new[] { rewrittenDocument.SyntaxTree }).ToArray();
            var compiledItems = _compiler.Compile(new CompilationItem(project, allProjectTrees), assemblies);
            assemblies.AddRange(compiledItems.Select(x => x.DllPath));

            newItems = compiledItems;

            return assemblies.ToArray();
        }
    }
}
