using System.Collections.Generic;
using TestCoverage.CoverageCalculation;

namespace TestCoverage.Storage
{
    public interface ICoverageStore
    {
        void AppendByDocumentPath(string documentPath,IEnumerable<LineCoverage> coverage);

        void Append( IEnumerable<LineCoverage> coverage);
        void WriteAll(IEnumerable<LineCoverage> coverage);

        void RemoveByFile(string filePath);

        LineCoverage[] ReadAll();

        void RemoveByDocumentTestNodePath(string documentFilePath);
    }
}