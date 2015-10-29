﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage
{
    public class SolutionExplorer : ISolutionExplorer
    {
        private readonly string _solutionPath;
        private readonly MSBuildWorkspace _workspace;
        private Solution _solution;

        public SolutionExplorer(string solutionPath)
        {
            var props = new Dictionary<string, string>();
            props["CheckForSystemRuntimeDependency"] = "true";

            _solutionPath = solutionPath;

            _workspace = MSBuildWorkspace.Create(props);
        }

        public void Open()
        {
            _solution = _workspace.OpenSolutionAsync(_solutionPath).Result;
        }

        public MetadataReference[] GetAllProjectReferences(string projectName)
        {
            var project = Solution.Projects.First(x => x.Name == projectName);
            var allReferences = new HashSet<MetadataReference>();

            PopulateWithReferences(allReferences,project);

            return allReferences.ToArray();
        }

        private void PopulateWithReferences(HashSet<MetadataReference> allReferences, Project project)
        {
            foreach (var reference in project.MetadataReferences)
            {
                allReferences.Add(reference);
            }

            foreach (ProjectReference projectReference in project.ProjectReferences)
            {
                var referencedProject = Solution.Projects.First(x => x.Id == projectReference.ProjectId);

                PopulateWithReferences(allReferences, referencedProject);
            }
        }

        public SyntaxTree OpenFile(string path)
        {
            //TODO Convert to async
            return GetAllDocuments().First(x => x.FilePath == path).GetSyntaxTreeAsync().Result;
        }

        public ISemanticModel GetSemanticModelByDocument(string docPath)
        {
            Document document = GetAllDocuments().First(x => x.FilePath == docPath);
            // TODO - convert to async
            return new RoslynSemanticModel(document.GetSemanticModelAsync().Result);
        }

        public void PopulateWithRewrittenAuditNodes(AuditVariablesMap auditVariablesMap)
        {
            foreach (var document in GetAllDocuments())
            {
                var rewrittenFilePath = PathHelper.GetRewrittenFilePath(document.FilePath);

                if (File.Exists(rewrittenFilePath))
                {
                    string content = File.ReadAllText(rewrittenFilePath);
                    ExtractAuditVariables(auditVariablesMap, content);
                }
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

        public _Assembly[] LoadCompiledAssemblies(params string[] excludedProjects)
        {
            List<Assembly> allAssemblies = new List<Assembly>();

            foreach (Project project in Solution.Projects)
            {
                if (excludedProjects.Contains(project.Name))
                    continue;

                string assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), PathHelper.GetCoverageDllName(project.Name));
                if (File.Exists(assemblyPath))
                {
                    Assembly assembly = Assembly.LoadFile(assemblyPath);
                    allAssemblies.Add(assembly);
                }
            }

            return allAssemblies.ToArray();
        }

        public Solution Solution => _solution;

        public string SolutionPath => _solutionPath;

        public IEnumerable<Document> GetAllDocuments()
        {
            return _solution.Projects.SelectMany(project => project.Documents);
        }

        public Project GetProjectByDocument(string documentPath)
        {
            var project = _solution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == documentPath));

            return project;
        }

        // TODO: Refactor
        private static void ExtractAuditVariables(AuditVariablesMap auditVariablesMap, string content)
        {
            int auditVariablePos = 0;

            while (true)
            {
                auditVariablePos = content.IndexOf(string.Format("AuditVariables.Coverage.Add(\""), auditVariablePos);
                if (auditVariablePos == -1)
                    break;

                int startQuote = content.IndexOf("\"", auditVariablePos + 1);
                int endQuote = content.IndexOf("\"", startQuote + 1);

                string varName = content.Substring(startQuote + 1, endQuote - startQuote - 1);

                string fullLine = content.Substring(auditVariablePos,
                    content.IndexOf("\n", auditVariablePos + 1) - auditVariablePos);

                int startCommentPos = fullLine.IndexOf(@"//", 0);

                string documentPath = fullLine.Substring(startCommentPos + 2, fullLine.Length - startCommentPos - 2);

                string nodePath = varName;

                for (int i = nodePath.Length - 1; i >= 0; i--)
                {
                    if (nodePath[i] == '_')
                    {
                        nodePath = nodePath.Substring(0, i);
                        break;
                    }
                }

                var placeholder = new AuditVariablePlaceholder(documentPath, nodePath, AuditVariablesMap.ExtractSpanFromVariableName(varName));
                auditVariablesMap.Map[varName] = placeholder;

                auditVariablePos = endQuote + 1;
            }
        }
    }
}