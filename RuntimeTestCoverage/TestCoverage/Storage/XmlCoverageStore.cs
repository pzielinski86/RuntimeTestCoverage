using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TestCoverage.CoverageCalculation;

namespace TestCoverage.Storage
{
    public class XmlCoverageStore : ICoverageStore
    {
        private readonly string _filePath;

        public XmlCoverageStore(string solutionPath)
        {
            string solutionDir = Path.GetDirectoryName(solutionPath);
            string solutionName = Path.GetFileNameWithoutExtension(solutionPath);

            _filePath = Path.Combine(solutionDir, $"{solutionName}.coverage");
        }

        public void Append(string documentPath,IEnumerable<LineCoverage> coverage)
        {
            var currentCoverage = ReadAll().ToList();

            for (int i = 0; i < currentCoverage.Count; i++)
            {
                if (currentCoverage[i].DocumentPath == documentPath ||
                    currentCoverage[i].TestDocumentPath == documentPath)
                {
                    currentCoverage.RemoveAt(i);
                    i--;
                }
            }

            currentCoverage.AddRange(coverage);

            WriteAll(currentCoverage);
        }

        public void WriteAll(IEnumerable<LineCoverage> coverage)
        {
            using (var stream = new FileStream(_filePath, FileMode.OpenOrCreate))
            {
                var binaryFormatter = new BinaryFormatter {Binder = new AllowAllAssemblyVersionsDeserializationBinder()};

                binaryFormatter.Serialize(stream, coverage.ToArray());
            }
        }

        public LineCoverage[] ReadAll()
        {
            using (var stream = new FileStream(_filePath, FileMode.OpenOrCreate))
            {
                if (stream.Length.Equals(0))
                    return new LineCoverage[0];

                var binaryFormatter = new BinaryFormatter {Binder = new AllowAllAssemblyVersionsDeserializationBinder()};

                var coverage = (LineCoverage[])binaryFormatter.Deserialize(stream);

                return coverage;
            }
        }

        public LineCoverage[] ReadByDocument(string documentPath)
        {
            var coverage = ReadAll();

            return coverage.Where(x => x.DocumentPath == documentPath).ToArray();
        }
    }
}