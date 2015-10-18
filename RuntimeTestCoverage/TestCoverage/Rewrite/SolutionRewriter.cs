﻿using System;
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

        public RewriteResult RewriteAllClasses(IEnumerable<Project> projects)
        {
            var rewrittenItems = new Dictionary<Project, List<RewrittenItemInfo>>();
            var auditVariablesMap = new AuditVariablesMap();

            foreach (Project project in projects)
            {
                foreach (Document document in project.Documents)
                {
                    SyntaxNode syntaxNode = document.GetSyntaxRootAsync().Result;

                    RewrittenDocument rewrittenDocument = RewriteDocument(project.Name, document.FilePath, syntaxNode.ToFullString());
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