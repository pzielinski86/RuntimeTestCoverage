using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using TestCoverage;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin
{
    public interface IVsSolutionTestCoverage
    {
        Dictionary<string, List<LineCoverage>> SolutionCoverageByDocument { get; }
        void LoadCurrentCoverage();
        Task CalculateForAllDocumentsAsync();
        Task CalculateForSelectedMethodAsync(string projectName, int span, SyntaxNode rootNode);
        Task CalculateForDocumentAsync(string projectName, string documentPath, string documentContent);
        void CalculateForDocument(string projectName, string documentPath, string documentContent);
    }
}