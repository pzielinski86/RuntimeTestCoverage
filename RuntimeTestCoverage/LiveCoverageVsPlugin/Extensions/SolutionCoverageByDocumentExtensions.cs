using System.Collections.Generic;
using System.Linq;
using TestCoverage;
using TestCoverage.CoverageCalculation;

namespace LiveCoverageVsPlugin.Extensions
{
    public static class SolutionCoverageByDocumentExtensions
    {
        public static string[] GetTestPaths(this Dictionary<string, List<LineCoverage>> source, string sutPath)
        {
            return source.SelectMany(x => x.Value.Where(y => y.NodePath == sutPath).Select(y => y.TestPath)).ToArray();

        }
        public static void MarkMethodAsCompilationError(this Dictionary<string, List<LineCoverage>> source, string path, string errorMsg)
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

        public static void MarkDocumentAsCompilationError(this Dictionary<string, List<LineCoverage>> source, string docPath, string errorMsg)
        {
            foreach (var documentCoverage in source.Values)
            {
                for (int i = 0; i < documentCoverage.Count; i++)
                {
                    if (documentCoverage[i].TestDocumentPath == docPath || documentCoverage[i].TestDocumentPath == docPath)
                    {
                        documentCoverage[i].IsSuccess = false;
                        documentCoverage[i].ErrorMessage = errorMsg;
                    }
                }
            }
        }

        public static void UpdateDocumentCoverage(this Dictionary<string, List<LineCoverage>> source, string recalculatedDocument, CoverageResult result)
        {
            foreach (var documentCoverage in source.Values)
            {
                for (int i = 0; i < documentCoverage.Count; i++)
                {
                    if (documentCoverage[i].DocumentPath == recalculatedDocument || documentCoverage[i].TestDocumentPath == recalculatedDocument)
                    {
                        documentCoverage.RemoveAt(i);
                        i--;
                    }
                }
            }

            foreach (string docPath in result.CoverageByDocument.Keys)
            {
                source[docPath] = result.CoverageByDocument[docPath].ToList();
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