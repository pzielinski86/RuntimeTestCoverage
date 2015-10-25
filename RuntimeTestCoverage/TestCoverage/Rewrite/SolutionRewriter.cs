using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TestCoverage.Extensions;

namespace TestCoverage.Rewrite
{
    public class SolutionRewriter
    {
        private readonly IAuditVariablesRewriter _auditVariablesRewriter;
        private readonly IContentWriter _contentWriter;

        public SolutionRewriter(IAuditVariablesRewriter auditVariablesRewriter, IContentWriter contentWriter)
        {
            _auditVariablesRewriter = auditVariablesRewriter;
            _contentWriter = contentWriter;
        }

        public RewrittenDocument RewriteDocument(Project project, string documentPath, string documentContent)
        {
            AuditVariablesMap auditVariablesMap = new AuditVariablesMap();

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent);
            SyntaxNode syntaxNode = syntaxTree.GetRoot();

            SyntaxNode rewrittenNode = _auditVariablesRewriter.Rewrite(project.Name, documentPath, syntaxNode,
                auditVariablesMap);

            _contentWriter.Write(documentPath, rewrittenNode.SyntaxTree);

            return new RewrittenDocument(auditVariablesMap, rewrittenNode.SyntaxTree, documentPath);
        }

        public RewriteResult RewriteAllClasses(IEnumerable<Project> projects)
        {
            var rewrittenItems = new Dictionary<Project, List<RewrittenDocument>>();
            var auditVariablesMap = new AuditVariablesMap();

            foreach (Project project in projects)
            {
                foreach (Document document in project.Documents)
                {
                    SyntaxNode syntaxNode = document.GetSyntaxRootAsync().Result;

                    RewrittenDocument rewrittenDocument = RewriteDocument(project, document.FilePath, syntaxNode.ToFullString());

                    if (!rewrittenItems.ContainsKey(document.Project))
                        rewrittenItems[document.Project] = new List<RewrittenDocument>();

                    rewrittenItems[document.Project].Add(rewrittenDocument);
                    auditVariablesMap.Map.Merge(rewrittenDocument.AuditVariablesMap.Map);
                    rewrittenDocument.AuditVariablesMap = auditVariablesMap;
                }
            }

            return new RewriteResult(rewrittenItems, auditVariablesMap);
        }
    }
}