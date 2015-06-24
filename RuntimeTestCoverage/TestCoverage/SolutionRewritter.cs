using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace TestCoverage
{
    public class SolutionRewritter
    {
        public RewrittenItemInfo RewriteTestClass(RewriteResult rewriteResult, string documentName)
        {
            RewrittenItemInfo rewrittenItem = rewriteResult.Items.Single(i => i.Document.Name == documentName);
            rewriteResult.AuditVariablesMap.ClearByDocumentName(documentName);

            var walker = new LineCoverageWalker();
            SyntaxNode syntaxNode = rewrittenItem.Document.GetSyntaxRootAsync().Result;

            walker.Visit(syntaxNode);

            var lineCoverageRewriter = new LineCoverageRewriter(rewriteResult.AuditVariablesMap, documentName, walker.AuditVariablePositions);
            SyntaxNode rewrittenNode = lineCoverageRewriter.Visit(syntaxNode);

            rewrittenItem.SyntaxTree = rewrittenNode.SyntaxTree;

            return rewrittenItem;
        }
        public RewriteResult RewriteAllClasses(string pathToSolution)
        {
            Project[] allProjects = GetProjects(pathToSolution);

            var rewrittenItems = new List<RewrittenItemInfo>();
            var auditVariablesMap = new AuditVariablesMap();

            foreach (Project project in allProjects)
            {
                foreach (Document document in GetAcceptableDocuments(project.Documents))

                {
                    var walker = new LineCoverageWalker();
                    SyntaxNode syntaxNode = document.GetSyntaxRootAsync().Result;

                    walker.Visit(syntaxNode);

                    var lineCoverageRewriter = new LineCoverageRewriter(auditVariablesMap, document.Name, walker.AuditVariablePositions);
                    SyntaxNode rewrittenNode = lineCoverageRewriter.Visit(syntaxNode);

                    rewrittenItems.Add(new RewrittenItemInfo(document, rewrittenNode.SyntaxTree));
                }
            }

            return new RewriteResult(rewrittenItems.ToArray(), auditVariablesMap);
        }

        private IEnumerable<Document> GetAcceptableDocuments(IEnumerable<Document> documents)
        {
            string[] excludedFiles = { "AssemblyInfo.cs" };

            return
                documents.Where(d => Regex.IsMatch(Path.GetFileNameWithoutExtension(d.Name), @"^[a-zA-Z0-9]+$"))
                    .Where(d => !excludedFiles.Contains(d.Name));
        }

        private Project[] GetProjects(string pathToSolution)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();

            Solution solutionToAnalyze = workspace.OpenSolutionAsync(pathToSolution).Result;

            return solutionToAnalyze.Projects.ToArray();
        }
    }
}