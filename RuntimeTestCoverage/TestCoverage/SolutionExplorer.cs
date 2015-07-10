using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TestCoverage.Rewrite;

namespace TestCoverage
{
    public class SolutionExplorer
    {
        private readonly string _solutionPath;
        private readonly MSBuildWorkspace _workspace;
        private Solution _solution;

        public SolutionExplorer(string solutionPath)
        {
            _solutionPath = solutionPath;

            _workspace = MSBuildWorkspace.Create();
        }

        public void Open()
        {
            _solution = _workspace.OpenSolutionAsync(_solutionPath).Result;
        }

        public void PopulateWithRewrittenAuditNodes(AuditVariablesMap auditVariablesMap)
        {
            foreach (var document in GetAllDocuments())
            {
                string content = File.ReadAllText(PathHelper.GetRewrittenFilePath(document.FilePath));
                ExtractAuditVariables(auditVariablesMap, content);
            }
        }

        public IEnumerable<SyntaxTree> LoadProjectSyntaxTrees(Project project, params string[] excludedDocuments)
        {
            foreach (var document in project.Documents)
            {
                if (excludedDocuments.Contains(document.FilePath))
                    continue;

                yield return document.GetSyntaxTreeAsync().Result;
            }
        }

        public Assembly[] LoadCompiledAssemblies(params string[] excludedProjects)
        {
            List<Assembly> allAssemblies = new List<Assembly>();

            foreach (Project project in Solution.Projects)
            {
                if (excludedProjects.Contains(project.Name))
                    continue;

                Assembly assembly = Assembly.LoadFrom(PathHelper.GetCoverageDllName(project.Name));
                allAssemblies.Add(assembly);
            }

            return allAssemblies.ToArray();
        }

        public IEnumerable<MetadataReference> GetAllReferences()
        {
            return _solution.Projects.SelectMany(project => project.MetadataReferences);
        }

        public Solution Solution
        {
            get { return _solution; }
        }

        public string SolutionPath
        {
            get { return _solutionPath; }
        }

        public IEnumerable<Document> GetAllDocuments()
        {
            return _solution.Projects.SelectMany(project => project.Documents);
        }

        public Project GetProjectByDocument(string documentPath)
        {
            return _solution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == documentPath));
        }

        private static void ExtractAuditVariables(AuditVariablesMap auditVariablesMap, string content)
        {
            int auditVariablePos = 0;

            while (true)
            {
                auditVariablePos = content.IndexOf(string.Format("AuditVariables.Coverage[\""), auditVariablePos);
                if (auditVariablePos == -1)
                    break;

                int startQuote = content.IndexOf("\"", auditVariablePos + 1);
                int endQuote = content.IndexOf("\"", startQuote + 1);

                string varName = content.Substring(startQuote + 1, endQuote - startQuote - 1);
                string fullLine = content.Substring(auditVariablePos,
                    content.IndexOf("\n", auditVariablePos + 1) - auditVariablePos);

                int startCommentPos = fullLine.IndexOf(@"//", 0);

                string documentPath = fullLine.Substring(startCommentPos + 2, fullLine.Length - startCommentPos - 2);

                var placeholder = new AuditVariablePlaceholder(documentPath, varName, AuditVariablesMap.ExtractSpanFromVariableName(varName));
                auditVariablesMap.Map[varName] = placeholder;

                auditVariablePos = endQuote + 1;
            }
        }
    }
}