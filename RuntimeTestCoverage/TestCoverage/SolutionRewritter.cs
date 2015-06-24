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
        public RewriteResult RewriteAllClasses(string pathToSolution)
        {
            Project[] allProjects = GetProjects(pathToSolution);

            var rewrittenItems = new List<RewrittenItemInfo>();
            var auditVariablesMap = new AuditVariablesMap();

            foreach (Project project in allProjects)
            {
                foreach (Document document in project.Documents.Where(d => Regex.IsMatch(Path.GetFileNameWithoutExtension(d.Name), @"^[a-zA-Z0-9]+$")))

                {
                    string documentName = document.Name;
                    if (documentName == "AssemblyInfo.cs")
                        continue;

                    var walker = new LineCoveravWalker();
                    walker.Visit(document.GetSyntaxRootAsync().Result);

                    var lineCoverageRewriter = new LineCoverageRewriter(auditVariablesMap, walker.AuditVariablePositions);

                    SyntaxNode rewrittenNode = lineCoverageRewriter.Visit(document.GetSyntaxRootAsync().Result);

                    rewrittenItems.Add(new RewrittenItemInfo(documentName, rewrittenNode.SyntaxTree));
                }
            }

            return new RewriteResult(rewrittenItems.ToArray(), auditVariablesMap);
        }

        private Project[] GetProjects(string pathToSolution)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();

            Solution solutionToAnalyze = workspace.OpenSolutionAsync(pathToSolution).Result;

            return solutionToAnalyze.Projects.ToArray();
        }
    }
}