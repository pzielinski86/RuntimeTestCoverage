using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestCoverage.Compilation;

namespace TestCoverage
{
    public class SolutionExplorer : MarshalByRefObject, ISolutionExplorer
    {
        private readonly Solution _solution;

        public SolutionExplorer(string solutionPath)
        {
            var props = new Dictionary<string, string>();
            props["CheckForSystemRuntimeDependency"] = "true";

            var workspace = MSBuildWorkspace.Create(props);
            // TODO: Get rid of blocking a thread
            _solution = workspace.OpenSolutionAsync(solutionPath).Result;
        }

        public string[] GetAllProjectReferences(string projectName)
        {
            var project = Solution.Projects.First(x => x.Name == projectName);
            var allReferences = new HashSet<MetadataReference>();

            PopulateWithReferences(allReferences, project);

            return allReferences.Select(x => x.Display).ToArray();
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

        public IEnumerable<SyntaxTree> LoadProjectSyntaxTrees(Project project, params string[] excludedDocuments)
        {
            foreach (var document in project.Documents)
            {
                if (excludedDocuments.Any(x => PathHelper.AreEqual(x, document.FilePath)))
                    continue;

                yield return document.GetSyntaxTreeAsync().Result;
            }
        }

        public string[] GetCompiledAssemblies(params string[] excludedProjects)
        {
            List<string> allAssemblies = new List<string>();

            foreach (Project project in Solution.Projects)
            {
                if (excludedProjects.Contains(project.Name))
                    continue;

                string assemblyPath = Path.Combine(Config.WorkingDirectory, PathHelper.GetCoverageDllName(project.Name));
                if (File.Exists(assemblyPath))
                {
                    allAssemblies.Add(assemblyPath);
                }
            }

            return allAssemblies.ToArray();
        }

        public Solution Solution => _solution;

        public string SolutionPath => Solution.FilePath;

        public IEnumerable<Document> GetAllDocuments()
        {
            return _solution.Projects.SelectMany(project => project.Documents);
        }

        public Project GetProjectByDocument(string documentPath)
        {
            var project = _solution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == documentPath));

            return project;
        }

    }
}