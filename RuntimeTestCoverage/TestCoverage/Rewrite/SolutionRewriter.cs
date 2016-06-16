using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.Rewrite
{
    public class SolutionRewriter
    {
        private readonly IAuditVariablesRewriter _auditVariablesRewriter;

        public SolutionRewriter(IAuditVariablesRewriter auditVariablesRewriter)
        {
            _auditVariablesRewriter = auditVariablesRewriter;
        }

        public RewrittenDocument RewriteDocumentWithAssemblyInfo(Project currentProject, Project[] allProjects, string documentPath, string documentContent)
        {
            var rewrittenDocument = RewriteDocument(currentProject.Name, documentPath, documentContent);
            var internalAttrDoc = CreateInternalVisibleToAttributeDocument(currentProject, allProjects);

            var statements = new SyntaxList<StatementSyntax>();            
            statements.Add(SyntaxFactory.ExpressionStatement(rewrittenDocument.SyntaxTree.GetRoot()));
            statements.Add(SyntaxFactory.ExpressionStatement(invocation));
            var wrapper = SyntaxFactory.Block(statements);
        }

        private RewrittenDocument RewriteDocument(string projectName, string documentPath, string documentContent)
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

                var internalVisibleToAttrDoc = CreateInternalVisibleToAttributeDocument(project, allProjects);
                if (internalVisibleToAttrDoc != null)
                    rewrittenItems[project].Add(internalVisibleToAttrDoc);
            }

            return new RewriteResult(rewrittenItems);
        }

        private string CreateInternalVisibleToAttribute(Project project)
        {
            string attribute =
                $"[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"{PathHelper.GetCoverageDllName(project.Name)}\")]";

            return attribute;
        }

        private RewrittenDocument CreateInternalVisibleToAttributeDocument(Project project, Project[] allProjects)
        {
            var testProjects = allProjects.Where(x => x.ProjectReferences.Any(y => y.ProjectId == project.Id)).ToArray();
            if (testProjects.Length == 0)
                return null;

            var propertiesBuilder = new StringBuilder();

            foreach (var testProject in testProjects)
            {
                var attribute = CreateInternalVisibleToAttribute(testProject);

                propertiesBuilder.AppendLine(attribute);
            }

            var tree = CSharpSyntaxTree.ParseText(propertiesBuilder.ToString());
            RewrittenDocument document = new RewrittenDocument(tree, Path.GetTempFileName() + ".cs");

            return document;
        }
    }
}