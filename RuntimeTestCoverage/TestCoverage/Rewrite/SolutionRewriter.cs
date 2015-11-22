using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TestCoverage.Extensions;

namespace TestCoverage.Rewrite
{
    internal class SolutionRewriter
    {
        private readonly IAuditVariablesRewriter _auditVariablesRewriter;

        public SolutionRewriter(IAuditVariablesRewriter auditVariablesRewriter)
        {
            _auditVariablesRewriter = auditVariablesRewriter;
        }

        public RewrittenDocument RewriteDocument(string projectName, string documentPath, string documentContent)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent,CSharpParseOptions.Default.WithPreprocessorSymbols("FRAMEWORK"));
            SyntaxNode syntaxNode = syntaxTree.GetRoot();

            SyntaxNode rewrittenNode = _auditVariablesRewriter.Rewrite(projectName, documentPath, syntaxNode);

            return new RewrittenDocument(rewrittenNode.SyntaxTree, documentPath);
        }

        public RewriteResult RewriteAllClasses(IEnumerable<Project> projects)
        {
            var rewrittenItems = new Dictionary<Project, List<RewrittenDocument>>();

            foreach (Project project in projects)
            {
                foreach (Document document in project.Documents)
                {
                    SyntaxNode syntaxNode = document.GetSyntaxRootAsync().Result;

                    RewrittenDocument rewrittenDocument = RewriteDocument(project.Name, document.FilePath, syntaxNode.ToFullString());

                    if (!rewrittenItems.ContainsKey(document.Project))
                        rewrittenItems[document.Project] = new List<RewrittenDocument>();

                    rewrittenItems[document.Project].Add(rewrittenDocument);
                }
            }

            return new RewriteResult(rewrittenItems);
        }
    }
}