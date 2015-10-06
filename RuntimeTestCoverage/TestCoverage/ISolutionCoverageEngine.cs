using System;
using System.Runtime.Remoting;

namespace TestCoverage
{
    public interface ISolutionCoverageEngine:IDisposable
    {
        void Init(string solutionPath);
        CoverageResult CalculateForAllDocuments();
        CoverageResult CalculateForDocument(string projectName, string documentPath, string documentContent);
    }
}