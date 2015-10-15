using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestCoverage
{
    public interface ITestExplorer
    {
        Task<TestProject[]> GetTestProjectsAsync();
    }
}