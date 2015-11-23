﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading.Tasks;

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