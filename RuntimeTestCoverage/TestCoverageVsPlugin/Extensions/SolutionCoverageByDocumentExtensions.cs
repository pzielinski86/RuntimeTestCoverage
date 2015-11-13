using System.Collections.Generic;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin.Extensions
{
    public static class SolutionCoverageByDocumentExtensions
    {
        public static void MergeByNodePath(this Dictionary<string, List<LineCoverage>> source, 
            List<LineCoverage> newCoverage,
            string testMethodNodePath)
        {
            RemoveByTestMethodNodePath(source, testMethodNodePath);

            foreach (var lineCoverage in newCoverage)
            {
                source[lineCoverage.DocumentPath].Add(lineCoverage);
            }
        }

        private static void RemoveByTestMethodNodePath(Dictionary<string, List<LineCoverage>> source, string testMethodNodePath)
        {
            foreach (var documentCoverage in source.Values)
            {
                for (int i = 0; i < documentCoverage.Count; i++)
                {
                    if (documentCoverage[i].TestPath == testMethodNodePath)
                        documentCoverage.RemoveAt(i--);
                }
            }
        }
    }
}