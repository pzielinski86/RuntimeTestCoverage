using System.Collections.Generic;
using System.Threading.Tasks;
using TestCoverage;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin
{
    public interface IVsSolutionTestCoverage
    {
        Dictionary<string, List<LineCoverage>> SolutionCoverageByDocument { get; }
        void LoadCurrentCoverage();
        void CalculateForAllDocuments();
        Task CalculateForDocumentAsync(string projectName, string documentPath, string documentContent);
        void CalculateForDocument(string projectName, string documentPath, string documentContent);
        Task<ISolutionCoverageEngine> InitAsync(bool forcToRecreate);
    }
}