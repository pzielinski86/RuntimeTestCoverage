using System.Collections.Generic;
using TestCoverage.CoverageCalculation;

namespace TestCoverage.Storage
{
    public interface ICoverageStore
    {
        void Add(IEnumerable<LineCoverage> coverage);

        LineCoverage[] ReadAll();

        LineCoverage[] ReadByDocument(string documentPath);
    }
}