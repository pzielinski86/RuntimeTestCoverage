using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin
{
    public interface IVsSolutionTestCoverage
    {
        string SolutionPath { get; }
        Dictionary<string, List<LineCoverage>> SolutionCoverageByDocument { get; }
        void LoadCurrentCoverage();
        Task CalculateForAllDocumentsAsync();
        Task CalculateForSelectedMethodAsync(string projectName, MethodDeclarationSyntax method);
        Task CalculateForDocumentAsync(string projectName, string documentPath, string documentContent);
        void CalculateForDocument(string projectName, string documentPath, string documentContent);
    }
}