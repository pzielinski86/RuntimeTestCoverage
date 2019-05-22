using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestCoverage.Compilation;
using TestCoverage.Storage;

namespace TestCoverage
{
    public class SolutionExplorer : ISolutionExplorer
    {
        private readonly IRewrittenDocumentsStorage _rewrittenDocumentsStorage;
        private readonly Workspace _myWorkspace;

        public SolutionExplorer(IRewrittenDocumentsStorage rewrittenDocumentsStorage, Workspace myWorkspace)
        {
            _rewrittenDocumentsStorage = rewrittenDocumentsStorage;
            _myWorkspace = myWorkspace;
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
            foreach (MetadataReference reference in project.MetadataReferences)
            {                
                allReferences.Add(reference);
            }
        }

        public SyntaxTree OpenFile(string path)
        {
            //TODO Convert to async
            var document = GetAllDocuments().First(x => x.FilePath == path);

            return document.GetSyntaxTreeAsync().Result;
        }

        public ISemanticModel GetSemanticModelByDocument(string docPath)
        {
            Document document = GetAllDocuments().First(x => x.FilePath == docPath);
            // TODO - convert to async
            return new RoslynSemanticModel(document.GetSemanticModelAsync().Result);
        }

        public IEnumerable<SyntaxTree> LoadRewrittenProjectSyntaxTrees(Project project,
            params string[] excludedDocuments)
        {
            return _rewrittenDocumentsStorage.GetRewrittenDocuments(Solution.FilePath, project.Name, excludedDocuments);
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

        public Solution Solution => _myWorkspace.CurrentSolution;

        public string SolutionPath => Solution.FilePath;

        public IEnumerable<Document> GetAllDocuments()
        {
            return Solution.Projects.SelectMany(project => project.Documents);
        }

        public Project GetProjectByDocument(string documentPath)
        {
            var project = Solution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == documentPath));

            return project;
        }
    }
}