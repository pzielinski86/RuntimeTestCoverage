using System.Collections.Generic;
using TestCoverage.CoverageCalculation;

namespace TestCoverage.Storage
{
    public interface ICoverageStore
    {
        void AppendByDocumentPath(string documentPath,IEnumerable<LineCoverage> coverage);

        void AppendByMethodNodePath(string testPath, IEnumerable<LineCoverage> coverage);
        void WriteAll(IEnumerable<LineCoverage> coverage);

        LineCoverage[] ReadAll();

    }
}