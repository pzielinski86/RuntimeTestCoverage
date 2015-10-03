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
        private readonly ISolutionExplorer _solutionExplorer;
        private readonly IAuditVariablesRewriter _auditVariablesRewriter;
        private readonly IContentWriter _contentWriter;

        public SolutionRewriter(ISolutionExplorer solutionExplorer, IAuditVariablesRewriter auditVariablesRewriter, IContentWriter contentWriter)
        {
            _solutionExplorer = solutionExplorer;
            _auditVariablesRewriter = auditVariablesRewriter;
            _contentWriter = contentWriter;
        }

        public RewrittenDocument RewriteDocument(string projectName, string documentPath, string documentContent)
        {
            AuditVariablesMap auditVariablesMap = new AuditVariablesMap();

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent);
            SyntaxNode syntaxNode = syntaxTree.GetRoot();

            SyntaxNode rewrittenNode = _auditVariablesRewriter.Rewrite(projectName, documentPath, syntaxNode,
                auditVariablesMap);

            _contentWriter.Write(new RewrittenItemInfo(documentPath, rewrittenNode.SyntaxTree));

            return new RewrittenDocument(auditVariablesMap, rewrittenNode.SyntaxTree, documentPath);
        }

        public RewriteResult RewriteAllClasses()
        {
            var rewrittenItems = new Dictionary<Project, List<RewrittenItemInfo>>();
            var auditVariablesMap = new AuditVariablesMap();

            foreach (Project project in _solutionExplorer.Solution.Projects)
            {
                foreach (Document document in project.Documents)
                {
                    SyntaxNode syntaxNode = document.GetSyntaxRootAsync().Result;

                    RewrittenDocument rewrittenDocument = RewriteDocument(project.Name, document.FilePath, syntaxNode.ToString());                    
                    RewrittenItemInfo rewrittenItemInfo = new RewrittenItemInfo(document.FilePath, rewrittenDocument.SyntaxTree);

                    if (!rewrittenItems.ContainsKey(document.Project))
                        rewrittenItems[document.Project] = new List<RewrittenItemInfo>();

                    rewrittenItems[document.Project].Add(rewrittenItemInfo);
                    auditVariablesMap.Map.Merge(rewrittenDocument.AuditVariablesMap.Map);
                }
            }

            return new RewriteResult(rewrittenItems, auditVariablesMap);
        }
    }
}