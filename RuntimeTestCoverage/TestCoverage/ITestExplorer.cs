using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace TestCoverage
{
    public interface ITestExplorer
    {
        Task<TestProject[]> GetAllTestProjectsAsync();        
        Task<Project[]> GetAllTestProjectsWithCoveredProjectsAsync();
        Task<Project[]> GetUnignoredTestProjectsWithCoveredProjectsAsync();
    }
}