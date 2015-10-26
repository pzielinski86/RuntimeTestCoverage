using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage
{
    public interface ISolutionExplorer
    {
        void Open();

        MetadataReference[] GetAllReferences(string projectName);
        SyntaxTree OpenFile(string path);

        ISemanticModel GetSemanticModelByDocument(string docPath);
        void PopulateWithRewrittenAuditNodes(AuditVariablesMap auditVariablesMap);
        IEnumerable<SyntaxTree> LoadProjectSyntaxTrees(Project project, params string[] excludedDocuments);
        Assembly[] LoadCompiledAssemblies(params string[] excludedProjects);
        Solution Solution { get; }
        string SolutionPath { get; }
        IEnumerable<Document> GetAllDocuments();
        Project GetProjectByDocument(string documentPath);
        MetadataReference[] GetProjectReferences(Project project1);
    }
}