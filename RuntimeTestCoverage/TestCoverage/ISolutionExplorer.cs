using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using TestCoverage.Rewrite;

namespace TestCoverage
{
    public interface ISolutionExplorer
    {
        void Open();
        void PopulateWithRewrittenAuditNodes(AuditVariablesMap auditVariablesMap);
        IEnumerable<SyntaxTree> LoadProjectSyntaxTrees(Project project, params string[] excludedDocuments);
        Assembly[] LoadCompiledAssemblies(params string[] excludedProjects);
        Solution Solution { get; }
        string SolutionPath { get; }
        IEnumerable<Document> GetAllDocuments();
        string GetProjectNameByDocument(string documentPath);
        Project GetProjectByDocument(string documentPath);
        MetadataReference[] GetProjectReferences(Project project1);
    }
}