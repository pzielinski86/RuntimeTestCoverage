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
    public class SolutionRewritter
    {
        public Tuple<AuditVariablesMap, SyntaxTree> RewriteTestClass(string documentName, string documentContent)
        {
            AuditVariablesMap auditVariablesMap = new AuditVariablesMap();

            var walker = new LineCoverageWalker();
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent);
            SyntaxNode syntaxNode = syntaxTree.GetRoot();

            walker.Visit(syntaxNode);

            var lineCoverageRewriter = new LineCoverageRewriter(auditVariablesMap, documentName, walker.AuditVariablePositions);
            SyntaxNode rewrittenNode = lineCoverageRewriter.Visit(syntaxNode);

            return new Tuple<AuditVariablesMap, SyntaxTree>(auditVariablesMap, rewrittenNode.SyntaxTree);
        }
        public RewriteResult RewriteAllClasses(string pathToSolution)
        {
            Project[] allProjects = GetProjects(pathToSolution);

            var rewrittenItems = new List<RewrittenItemInfo>();
            var auditVariablesMap = new AuditVariablesMap();


            foreach (Document document in GetDocuments(pathToSolution))

            {
                var walker = new LineCoverageWalker();
                SyntaxNode syntaxNode = document.GetSyntaxRootAsync().Result;

                walker.Visit(syntaxNode);

                var lineCoverageRewriter = new LineCoverageRewriter(auditVariablesMap, document.Name, walker.AuditVariablePositions);
                SyntaxNode rewrittenNode = lineCoverageRewriter.Visit(syntaxNode);

                rewrittenItems.Add(new RewrittenItemInfo(document, rewrittenNode.SyntaxTree));
            }

            return new RewriteResult(rewrittenItems.ToArray(), auditVariablesMap);
        }

        public IEnumerable<Document> GetDocuments(string solutionPath)
        {
            Project[] allProjects = GetProjects(solutionPath);


            foreach (Project project in allProjects)
            {
                foreach (Document document in GetAcceptableDocuments(project.Documents))

                {
                    yield return document;
                }
            }

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