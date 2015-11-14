using System.Collections.Generic;
using System.Linq;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin.Extensions
{
    public static class SolutionCoverageByDocumentExtensions
    {
        public static void MergeByNodePath(this Dictionary<string, List<LineCoverage>> source, 
            List<LineCoverage> newCoverage)
        {
            string[] testMethods = source.Values.SelectMany(x => x).Select(x => x.TestPath).Distinct().ToArray();

            RemoveByTestMethodNodePath(source, testMethods);

            foreach (var lineCoverage in newCoverage)
            {
                source[lineCoverage.DocumentPath].Add(lineCoverage);
            }
        }

        private static void RemoveByTestMethodNodePath(Dictionary<string, List<LineCoverage>> source, string[] testMethodNodePath)
        {
            foreach (var documentCoverage in source.Values)
            {
                for (int i = 0; i < documentCoverage.Count; i++)
                {
                    if (testMethodNodePath.Contains(documentCoverage[i].TestPath ))
                        documentCoverage.RemoveAt(i--);
                }
            }
        }
    }
}