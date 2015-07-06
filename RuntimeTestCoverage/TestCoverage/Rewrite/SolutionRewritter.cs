using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TestCoverage.Rewrite
{
    internal class SolutionRewritter
    {
        private readonly SolutionExplorer _solutionExplorer;

        public SolutionRewritter(SolutionExplorer solutionExplorer)
        {
            _solutionExplorer = solutionExplorer;
        }

        public RewrittenDocument RewriteDocument(string projectName,string documentPath, string documentContent)
        {
            AuditVariablesMap auditVariablesMap = new AuditVariablesMap();

            var walker = new LineCoverageWalker(projectName);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent);
            
            SyntaxNode syntaxNode = syntaxTree.GetRoot();

            walker.Visit(syntaxNode);

            foreach (var auditVariablePlaceholderPosition in walker.AuditVariablePlaceholderPositions)
            {
                auditVariablePlaceholderPosition.DocumentPath = documentPath;
            }

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

                    LineCoverageWalker walker=new LineCoverageWalker(project.Name);
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