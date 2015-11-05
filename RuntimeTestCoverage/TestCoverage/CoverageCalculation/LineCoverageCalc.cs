using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Compilation;
using TestCoverage.Extensions;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    internal class LineCoverageCalc
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
            var allAssemblies = compiledItems.Select(x => x.Assembly).ToArray();

            var finalCoverage = new List<LineCoverage>();

            foreach (Project project in rewritenResult.Items.Keys)
            {
                foreach (RewrittenDocument rewrittenItem in rewritenResult.Items[project])
                {
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


        public LineCoverage[] CalculateForDocument(Project project, RewrittenDocument rewrittenDocument)
        {
            ICompiledItem[] newCompiledItems;

            _Assembly[] allAssemblies = CompileDocument(project, rewrittenDocument, out newCompiledItems);

            ISemanticModel semanticModel = newCompiledItems[0].GetSemanticModel(rewrittenDocument.SyntaxTree);
            LineCoverage[] fullCoverage = _testRunner.RunAllTestsInDocument(rewrittenDocument, semanticModel, project, allAssemblies);

            if (fullCoverage == null)
                fullCoverage = CalculateCoverageForReferencedTests(project, rewrittenDocument, allAssemblies);

            return fullCoverage.ToArray();
        }

        private LineCoverage[] CalculateCoverageForReferencedTests(Project project, 
            RewrittenDocument rewrittenDocument,
            _Assembly[] allAssemblies)
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

        private _Assembly[] CompileDocument(Project project, RewrittenDocument rewrittenDocument, out ICompiledItem[] newItems)
        {
            List<_Assembly> assemblies = _testExplorer.SolutionExplorer.LoadCompiledAssemblies(project.Name).ToList();

            SyntaxTree[] projectTrees = _testExplorer.SolutionExplorer.LoadProjectSyntaxTrees(project, rewrittenDocument.DocumentPath).ToArray();

            var allProjectTrees = projectTrees.Union(new[] { rewrittenDocument.SyntaxTree }).ToArray();
            var compiledItems = _compiler.Compile(new CompilationItem(project, allProjectTrees), assemblies);
            assemblies.AddRange(compiledItems.Select(x => x.Assembly));

            newItems = compiledItems;

            return assemblies.ToArray();
        }
    }
}
