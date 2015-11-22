using System;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage
{
    public interface ISolutionCoverageEngine : IDisposable
    {
        void Init(string solutionPath);
        Task<CoverageResult> CalculateForAllDocumentsAsync();
        CoverageResult CalculateForDocument(string projectName, string documentPath, string documentContent);

        CoverageResult CalculateForMethod(string projectName, MethodDeclarationSyntax method);

        bool IsDisposed { get; }
    }
}