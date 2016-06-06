using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestCoverage.Rewrite
{
    public class SolutionRewriter
    {
        private readonly IAuditVariablesRewriter _auditVariablesRewriter;

        public SolutionRewriter(IAuditVariablesRewriter auditVariablesRewriter)
        {
            _auditVariablesRewriter = auditVariablesRewriter;
        }

        public RewrittenDocument RewriteDocument(string projectName, string documentPath, string documentContent)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent, CSharpParseOptions.Default.WithPreprocessorSymbols("FRAMEWORK"));
            SyntaxNode syntaxNode = syntaxTree.GetRoot();

            SyntaxNode rewrittenNode = _auditVariablesRewriter.Rewrite(projectName, documentPath, syntaxNode);

            return new RewrittenDocument(rewrittenNode.SyntaxTree, documentPath);
        }

        public RewriteResult RewriteAllClasses(IEnumerable<Project> projects)
        {
            var rewrittenItems = new Dictionary<Project, List<RewrittenDocument>>();
            var allProjects = projects.ToArray();

            foreach (Project project in allProjects)
            {
                foreach (Document document in project.Documents)
                {
                    SyntaxNode syntaxNode = document.GetSyntaxRootAsync().Result;

                    RewrittenDocument rewrittenDocument = RewriteDocument(project.Name, document.FilePath, syntaxNode.ToFullString());

                    if (!rewrittenItems.ContainsKey(document.Project))
                        rewrittenItems[document.Project] = new List<RewrittenDocument>();

                    rewrittenItems[document.Project].Add(rewrittenDocument);
                }

                AddInternalVisibleToAttribute(rewrittenItems, project, allProjects);
            }

            return new RewriteResult(rewrittenItems);
        }

        private void AddInternalVisibleToAttribute(Dictionary<Project, List<RewrittenDocument>> rewrittenDocuments, Project project,Project[] allProjects )
        {
            foreach (ProjectReference referencedProjectRef in project.ProjectReferences)
            {
                var referencedProject =
                    allProjects.First(x => x.Id == referencedProjectRef.ProjectId);

                string attribute =
                    $"[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"{PathHelper.GetCoverageDllName(project.Name)}\")]";

                if (!rewrittenDocuments.ContainsKey(referencedProject))
                    rewrittenDocuments.Add(referencedProject, new List<RewrittenDocument>());

                var node = CSharpSyntaxTree.ParseText(attribute);
                                
                RewrittenDocument document = new RewrittenDocument(node, Path.GetTempFileName() + ".cs");

                rewrittenDocuments[referencedProject].Add(document);
            }
        }
    }
}