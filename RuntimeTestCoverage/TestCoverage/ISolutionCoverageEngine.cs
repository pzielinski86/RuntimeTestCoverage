using System.Runtime.Remoting;

namespace TestCoverage
{
    public interface ISolutionCoverageEngine
    {
        void Init(string solutionPath);
        CoverageResult CalculateForAllDocuments();
        CoverageResult CalculateForDocument(string projectName, string documentPath, string documentContent);
        CoverageResult CalculateForTest(string projectName, string documentPath, string documentContent, string className, string methodName);
    }
}