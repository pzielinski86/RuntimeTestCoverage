using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage
{
    public interface ISolutionExplorer
    {
        MetadataReference[] GetAllProjectReferences(string projectName);
        SyntaxTree OpenFile(string path);
        ISemanticModel GetSemanticModelByDocument(string docPath);
        IEnumerable<SyntaxTree> LoadProjectSyntaxTrees(Project project, params string[] excludedDocuments);
        _Assembly[] LoadCompiledAssemblies(params string[] excludedProjects);
        Solution Solution { get; }
        string SolutionPath { get; }
        IEnumerable<Document> GetAllDocuments();
        Project GetProjectByDocument(string documentPath);
    }
}