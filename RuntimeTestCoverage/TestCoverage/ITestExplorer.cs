﻿using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using TestCoverage.Rewrite;

namespace TestCoverage
{
    public interface ITestExplorer
    {
        Task<TestProject[]> GetAllTestProjectsAsync();        
        Task<Project[]> GetAllTestProjectsWithCoveredProjectsAsync();
        Task<Project[]> GetUnignoredTestProjectsWithCoveredProjectsAsync();

        ISolutionExplorer SolutionExplorer { get; }
        RewrittenDocument[] GetReferencedTests(RewrittenDocument rewrittenDocument, string projectName);
    }
}