using System.Collections.Generic;
using TestCoverage.CoverageCalculation;

namespace TestCoverage.Storage
{
    public interface ICoverageStore
    {
        void Append(IEnumerable<LineCoverage> coverage);

        void WriteAll(IEnumerable<LineCoverage> coverage);

        LineCoverage[] ReadAll();

        LineCoverage[] ReadByDocument(string documentPath);
    }
}