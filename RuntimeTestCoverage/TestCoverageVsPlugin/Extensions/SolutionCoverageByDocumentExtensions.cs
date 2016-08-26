using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin.Extensions
{
    public static class SolutionCoverageByDocumentExtensions
    {
        public static string[] GetTestPaths(this Dictionary<string, List<LineCoverage>> source, string sutPath)
        {
            return source.SelectMany(x => x.Value.Where(y => y.NodePath == sutPath).Select(y => y.TestPath)).ToArray();

        }
        public static void MarkAsCompilationError(this Dictionary<string, List<LineCoverage>> source, string path, string errorMsg)
        {
            string[] testPaths = source.GetTestPaths(path);

            foreach (var documentCoverage in source.Values)
            {
                for (int i = 0; i < documentCoverage.Count; i++)
                {
                    if (testPaths.Contains(documentCoverage[i].TestPath) || documentCoverage[i].NodePath == path)
                    {
                        documentCoverage[i].IsSuccess = false;
                        documentCoverage[i].ErrorMessage = errorMsg;
                    }
                }
            }
        }

        public static void MergeByNodePath(this Dictionary<string, List<LineCoverage>> source,
            List<LineCoverage> newCoverage)
        {
            string[] testPaths = newCoverage.Select(x => x.TestPath).Distinct().ToArray();            

            RemoveByTestPaths(source, testPaths);

            foreach (var lineCoverage in newCoverage)
            {
                if (!source.ContainsKey(lineCoverage.DocumentPath))
                    source[lineCoverage.DocumentPath] = new List<LineCoverage>();

                source[lineCoverage.DocumentPath].Add(lineCoverage);
            }
        }

        private static void RemoveByTestPaths(Dictionary<string, List<LineCoverage>> source, string[] testPaths)
        {
            foreach (var documentCoverage in source.Values)
            {
                for (int i = 0; i < documentCoverage.Count; i++)
                {
                    if (testPaths.Contains(documentCoverage[i].TestPath))
                        documentCoverage.RemoveAt(i--);
                }
            }
        }
    }
}