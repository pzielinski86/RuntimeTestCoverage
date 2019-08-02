using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestCoverage.CoverageCalculation;

namespace TestCoverage.Tasks
{
    public interface IVsSolutionTestCoverage
    {
        string SolutionPath { get; }
        Dictionary<string, List<LineCoverage>> SolutionCoverageByDocument { get; }
        Workspace MyWorkspace { get; }
        void Reinit();
        void Dispose();
        void LoadCurrentCoverage();
        void RemoveByPath(string filePath);
        Task<bool> CalculateForAllDocumentsAsync();
        Task<bool> CalculateForSelectedMethodAsync(string projectName, MethodDeclarationSyntax method);
        Task<bool> CalculateForDocumentAsync(string projectName, string documentPath, string documentContent);
    }
}