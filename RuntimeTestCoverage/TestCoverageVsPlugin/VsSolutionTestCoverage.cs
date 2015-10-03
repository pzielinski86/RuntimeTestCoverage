using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using TestCoverage;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin
{
    public class VsSolutionTestCoverage
    {
        private readonly SolutionExplorer _solutionExplorer;
        private readonly string _solutionPath;
        private readonly DTE _dte;
        private Dictionary<string, List<LineCoverage>> _solutionCoverageByDocument;

        public VsSolutionTestCoverage(string solutionPath, DTE dte)
        {
            _solutionPath = solutionPath;
            _dte = dte;
            _solutionExplorer=new SolutionExplorer(_solutionPath);
            _solutionExplorer.Open();
        }

        public Dictionary<string, List<LineCoverage>> SolutionCoverageByDocument
        {
            get { return _solutionCoverageByDocument; }
            set { _solutionCoverageByDocument = value; }
        }

        public void CalculateForAllDocuments()
        {
            using (var engine = new AppDomainSolutionCoverageEngine())
            {
                engine.Init(_solutionPath);

                CoverageResult coverage = engine.CalculateForAllDocuments();
                _solutionCoverageByDocument = coverage.CoverageByDocument.ToDictionary(x => x.Key, x => x.Value.ToList());
            }
        }      
       
        public Task CalculateForDocumentAsync(string documentPath, string documentContent)
        {
            return Task.Factory.StartNew(() => CalculateForDocument(documentPath, documentContent));
        }

        public void CalculateForDocument(string documentPath, string documentContent)
        { 
            var selectedProject = _solutionExplorer.GetProjectByDocument(documentPath);

            string path = string.Format("{0}.{1}", selectedProject.Name, Path.GetFileNameWithoutExtension(documentPath));
            ClearCoverageByDocument(path);

            using (var engine = new AppDomainSolutionCoverageEngine())
            {
                engine.Init(_solutionPath);

                CoverageResult coverage = engine.CalculateForDocument(selectedProject.Name, documentPath, documentContent);
                UpdateSolutionCoverage(coverage);
            }               
        }
    
        public void ClearCoverageByDocument(string path)
        {
            foreach (string docPath in _solutionCoverageByDocument.Keys)
            {
                List<LineCoverage> documentCoverage = _solutionCoverageByDocument[docPath];

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
                if (coverage.CoverageByDocument.ContainsKey(docPath))
                    _solutionCoverageByDocument[docPath].AddRange(coverage.CoverageByDocument[docPath]);
                else
                    _solutionCoverageByDocument[docPath] = coverage.CoverageByDocument[docPath].ToList();
            }
        }             
    }
}