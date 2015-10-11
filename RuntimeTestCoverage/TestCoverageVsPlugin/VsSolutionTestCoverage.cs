using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TestCoverage;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin
{
    public class VsSolutionTestCoverage
    {
        private readonly ISolutionExplorer _solutionExplorer;
        private readonly Func<ISolutionCoverageEngine> _solutionCoverageFactory;

        public VsSolutionTestCoverage(ISolutionExplorer solutionExplorer, 
            Func<ISolutionCoverageEngine> solutionCoverageFactory)
        {
            _solutionCoverageFactory = solutionCoverageFactory;
            _solutionExplorer = solutionExplorer;
            _solutionExplorer.Open();
            SolutionCoverageByDocument=new Dictionary<string, List<LineCoverage>>();
        }

        public Dictionary<string, List<LineCoverage>> SolutionCoverageByDocument { get; private set; }

        public void CalculateForAllDocuments()
        {
            using (ISolutionCoverageEngine engine = _solutionCoverageFactory())
            {
                engine.Init(_solutionExplorer.SolutionPath);

                CoverageResult coverage;

                try
                {
                    coverage = engine.CalculateForAllDocuments();
                }
                catch (TestCoverageCompilationException e)
                {
                    SolutionCoverageByDocument.Clear();
                    return;
                }

                SolutionCoverageByDocument = coverage.CoverageByDocument.ToDictionary(x => x.Key, x => x.Value.ToList());
            }
        }      
       
        public Task CalculateForDocumentAsync(string projectName, string documentPath, string documentContent)
        {
            return Task.Factory.StartNew(() => CalculateForDocument(projectName,documentPath, documentContent));
        }

        public void CalculateForDocument(string projectName, string documentPath, string documentContent)
        { 
            string path = $"{projectName}.{Path.GetFileNameWithoutExtension(documentPath)}";
            ClearDataCoveredByPath(path);

            using (var engine = _solutionCoverageFactory())
            {
                engine.Init(_solutionExplorer.SolutionPath);

                CoverageResult coverage;

                try
                {
                    coverage = engine.CalculateForDocument(projectName, documentPath, documentContent);
                }
                catch (TestCoverageCompilationException e)
                {
                    SolutionCoverageByDocument.Clear();
                    return;
                }

                UpdateSolutionCoverage(coverage);
            }               
        }
    
        public void ClearDataCoveredByPath(string path)
        {
            foreach (string docPath in SolutionCoverageByDocument.Keys)
            {
                List<LineCoverage> documentCoverage = SolutionCoverageByDocument[docPath];

                for (int i = 0; i < documentCoverage.Count; i++)
                {
                    if (documentCoverage[i].TestPath.StartsWith(path) ||
                        documentCoverage[i].Path.StartsWith(path))
                    {
                        documentCoverage.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private void UpdateSolutionCoverage(CoverageResult coverage)
        {
            foreach (string docPath in coverage.CoverageByDocument.Keys)
            {
                if (SolutionCoverageByDocument.ContainsKey(docPath))
                    SolutionCoverageByDocument[docPath].AddRange(coverage.CoverageByDocument[docPath]);
                else
                    SolutionCoverageByDocument[docPath] = coverage.CoverageByDocument[docPath].ToList();
            }
        }             
    }
}