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
        Task CalculateForAllDocumentsAsync();
        Task<bool> CalculateForSelectedMethodAsync(string projectName, MethodDeclarationSyntax method);
        Task<bool> CalculateForDocumentAsync(string projectName, string documentPath, string documentContent);
    }
}