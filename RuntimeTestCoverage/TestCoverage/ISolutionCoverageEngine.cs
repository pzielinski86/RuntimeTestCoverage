using System;
using System.Runtime.Remoting;
using Microsoft.CodeAnalysis;

namespace TestCoverage
{
    public interface ISolutionCoverageEngine:IDisposable
    {
        void Init(string solutionPath);
        CoverageResult CalculateForAllDocuments();
        CoverageResult CalculateForDocument(string projectName, string documentPath, string documentContent);

        bool IsDisposed { get; }
    }
}