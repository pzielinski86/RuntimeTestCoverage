using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Compilation;
using TestCoverage.Extensions;
using TestCoverage.Rewrite;
using TestCoverage.Storage;

namespace TestCoverage.CoverageCalculation
{
    internal class LineCoverageCalc
    {
        private readonly ISolutionExplorer _solutionExplorer;
        private readonly ICompiler _compiler;
        private readonly ICoverageStore _coverageStore;
        private readonly ITestRunner _testRunner;

        public LineCoverageCalc(ISolutionExplorer solutionExplorer,
            ICompiler compiler,
            ICoverageStore coverageStore,
            ITestRunner testRunner)
        {
            _solutionExplorer = solutionExplorer;
            _compiler = compiler;
            _coverageStore = coverageStore;
            _testRunner = testRunner;
        }

        public LineCoverage[] CalculateForAllTests(RewriteResult rewritenResult)
        {
            CompiledItem[] compiledLibs = _compiler.Compile(rewritenResult.ToCompilationItems(), rewritenResult.AuditVariablesMap);
            var allAssemblies = compiledLibs.Select(x => x.Assembly).ToArray();

            var finalCoverage = new List<LineCoverage>();

            foreach (Project project in rewritenResult.Items.Keys)
            {
                foreach (RewrittenDocument rewrittenItem in rewritenResult.Items[project])
                {
                    var testProjectCompiltedItem = compiledLibs.Single(x => x.Project == project);
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
            CompiledItem[] newCompiledItems;

            Assembly[] allAssemblies = CompileDocument(project, rewrittenDocument, out newCompiledItems);
            string docName = Path.GetFileNameWithoutExtension(rewrittenDocument.DocumentPath);

            ISemanticModel semanticModel = newCompiledItems[0].GetSemanticModel(rewrittenDocument.SyntaxTree);
            LineCoverage[] fullCoverage = _testRunner.RunAllTestsInDocument(rewrittenDocument, semanticModel, project, allAssemblies);

            if (fullCoverage == null)
            {
                List<LineCoverage> finalCoverage = new List<LineCoverage>();

                var referencedTests = GetReferencedTests(rewrittenDocument.SyntaxTree.GetRoot(), docName, rewrittenDocument.AuditVariablesMap, project.Name);

                foreach (RewrittenDocument referencedTest in referencedTests)
                {
                    semanticModel = _solutionExplorer.GetSemanticModelByDocument(referencedTest.DocumentPath);
                    var testProject = _solutionExplorer.GetProjectByDocument(referencedTest.DocumentPath);

                    var coverage = _testRunner.RunAllTestsInDocument(referencedTest, semanticModel, testProject, allAssemblies);
                    finalCoverage.AddRange(coverage);
                }

                fullCoverage = finalCoverage.ToArray();
            }


            return fullCoverage.ToArray();
        }


        private RewrittenDocument[] GetReferencedTests(SyntaxNode root, string documentName, AuditVariablesMap auditVariablesMap, string projectName)
        {
            var methods = root.GetPublicMethods();
            var currentCoverage = _coverageStore.ReadAll();
            var rewrittenDocuments = new List<RewrittenDocument>();
            ;
            foreach (var method in methods)
            {
                string path = NodePathBuilder.BuildPath(method, documentName, projectName);

                foreach (var docCoverage in currentCoverage.Where(x => x.Path == path))
                {
                    if (rewrittenDocuments.All(x => x.DocumentPath != docCoverage.TestDocumentPath))
                    {
                        SyntaxTree testRoot = _solutionExplorer.OpenFile(docCoverage.TestDocumentPath);

                        var rewrittenDocument = new RewrittenDocument(auditVariablesMap, testRoot, docCoverage.TestDocumentPath);
                        rewrittenDocuments.Add(rewrittenDocument);
                    }
                }
            }

            return rewrittenDocuments.ToArray();
        }

        private Assembly[] CompileDocument(Project project, RewrittenDocument rewrittenDocument, out CompiledItem[] newItems)
        {
            List<Assembly> assemblies = _solutionExplorer.LoadCompiledAssemblies(project.Name).ToList();

            _solutionExplorer.PopulateWithRewrittenAuditNodes(rewrittenDocument.AuditVariablesMap);

            SyntaxTree[] projectTrees = _solutionExplorer.LoadProjectSyntaxTrees(project, rewrittenDocument.DocumentPath).ToArray();

            var allProjectTrees = projectTrees.Union(new[] { rewrittenDocument.SyntaxTree }).ToArray();
            CompiledItem[] compiledItems = _compiler.Compile(new CompilationItem(project, allProjectTrees), assemblies, rewrittenDocument.AuditVariablesMap);
            assemblies.AddRange(compiledItems.Select(x => x.Assembly));

            newItems = compiledItems;

            return assemblies.ToArray();
        }

    }
}
