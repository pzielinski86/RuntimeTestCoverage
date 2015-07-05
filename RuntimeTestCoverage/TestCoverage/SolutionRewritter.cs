using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

namespace TestCoverage
{
    internal class SolutionRewritter
    {
        private readonly SolutionExplorer _solutionExplorer;
        public SolutionRewritter(SolutionExplorer solutionExplorer)
        {
            _solutionExplorer = solutionExplorer;
        }

        public RewrittenDocument RewriteDocument(string documentPath, string documentContent)
        {
            AuditVariablesMap auditVariablesMap = new AuditVariablesMap();

            var walker = new LineCoverageWalker();
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent);
            SyntaxNode syntaxNode = syntaxTree.GetRoot();

            walker.Visit(syntaxNode);

            var lineCoverageRewriter = new LineCoverageRewriter(auditVariablesMap, walker.AuditVariablePlaceholderPositions);
            SyntaxNode rewrittenNode = lineCoverageRewriter.Visit(syntaxNode);

            File.WriteAllText(PathHelper.GetRewrittenFilePath(documentPath), rewrittenNode.ToString());

            return new RewrittenDocument(auditVariablesMap, rewrittenNode.SyntaxTree,documentPath);
        }        

        public RewriteResult RewriteAllClasses(string pathToSolution)
        {
            var rewrittenItems = new Dictionary<Project, List<RewrittenItemInfo>>();
            var auditVariablesMap = new AuditVariablesMap();           

            foreach (Project project in _solutionExplorer.Solution.Projects)
            {
                foreach (Document document in project.Documents)
                {
                    SyntaxNode syntaxNode = document.GetSyntaxRootAsync().Result;

                    LineCoverageWalker walker=new LineCoverageWalker();
                    walker.Visit(syntaxNode);

                    var lineCoverageRewriter = new LineCoverageRewriter(auditVariablesMap, walker.AuditVariablePlaceholderPositions);
                    SyntaxNode rewrittenNode = lineCoverageRewriter.Visit(syntaxNode);
                    
                    File.WriteAllText(PathHelper.GetRewrittenFilePath(document.FilePath), rewrittenNode.ToString());

                    RewrittenItemInfo rewrittenItemInfo = new RewrittenItemInfo(document, rewrittenNode.SyntaxTree);

                    if (!rewrittenItems.ContainsKey(document.Project))
                    {
                        rewrittenItems[document.Project] = new List<RewrittenItemInfo>();
                    }

                    rewrittenItems[document.Project].Add(rewrittenItemInfo);                    
                }
            }
    

            return new RewriteResult(rewrittenItems, auditVariablesMap);
        }
    
 
    }
}