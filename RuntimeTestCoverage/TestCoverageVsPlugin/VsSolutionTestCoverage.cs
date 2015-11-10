using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TestCoverage;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Storage;
using Task = System.Threading.Tasks.Task;

namespace TestCoverageVsPlugin
{
    public class VsSolutionTestCoverage : IVsSolutionTestCoverage
    {
        private readonly string _solutionPath;
        private readonly Func<ISolutionCoverageEngine> _solutionCoverageFactory;
        private ISolutionCoverageEngine _engine;
        private readonly ICoverageStore _coverageStore;
        private static VsSolutionTestCoverage _vsSolutionTestCoverage;
        private static readonly object SyncObject = new object();
        private readonly object _sync = new object();
        private ILogger _logger;

        public VsSolutionTestCoverage(string solutionPath,
            Func<ISolutionCoverageEngine> solutionCoverageFactory,
            ICoverageStore coverageStore,
            ILogger logger)
        {
            _solutionPath = solutionPath;
            _solutionCoverageFactory = solutionCoverageFactory;
            _coverageStore = coverageStore;
            _logger = logger;
            SolutionCoverageByDocument = new Dictionary<string, List<LineCoverage>>();
        }

        public Task<ISolutionCoverageEngine> InitAsync(bool forcToRecreate)
        {
            return Task.Run(() => Init(forcToRecreate));
        }

        private ISolutionCoverageEngine Init(bool forcToRecreate)
        {
            lock (_sync)
            {
                if (_engine == null || _engine.IsDisposed || forcToRecreate)
                {
                    _engine = _solutionCoverageFactory();
                    _engine.Init(_solutionPath);
                }
            }

            return _engine;
        }

        public static VsSolutionTestCoverage CreateInstanceIfDoesNotExist(string solutionPath, Func<ISolutionCoverageEngine> solutionCoverageFactory, ICoverageStore coverageStore, ILogger logger)
        {
            if (_vsSolutionTestCoverage == null)
            {
                lock (SyncObject)
                {
                    if (_vsSolutionTestCoverage == null)
                    {
                        _vsSolutionTestCoverage = new VsSolutionTestCoverage(solutionPath, solutionCoverageFactory,
                            coverageStore, logger);
                    }
                }
            }

            return _vsSolutionTestCoverage;
        }

        public Dictionary<string, List<LineCoverage>> SolutionCoverageByDocument { get; private set; }

        public void LoadCurrentCoverage()
        {
            LineCoverage[] coverage = _coverageStore.ReadAll();

            SolutionCoverageByDocument = coverage.GroupBy(x => x.DocumentPath).ToDictionary(x => x.Key, x => x.ToList());
        }

        public void CalculateForAllDocuments()
        {
            using (var engine = Init(false))
            {
                CoverageResult coverage;

                try
                {
                    coverage = engine.CalculateForAllDocuments();
                }
                catch (TestCoverageCompilationException e)
                {
                    SolutionCoverageByDocument.Clear();
                    _logger.Write(e.ToString());
                    return;
                }

                SolutionCoverageByDocument = coverage.CoverageByDocument.ToDictionary(x => x.Key, x => x.Value.ToList());
            }
        }

        public Task CalculateForDocumentAsync(string projectName, string documentPath, string documentContent)
        {
            return Task.Run(() => CalculateForDocument(projectName, documentPath, documentContent));
        }

        public void CalculateForDocument(string projectName, string documentPath, string documentContent)
        {
            using (var engine = Init(false))
            {
                CoverageResult coverage;

                try
                {
                    coverage = engine.CalculateForDocument(projectName, documentPath, documentContent);
                }
                catch (TestCoverageCompilationException e)
                {
                    SolutionCoverageByDocument.Clear();
                    _logger.Write(e.ToString());
                    return;
                }

                UpdateSolutionCoverage(coverage);
            }
        }

        private void UpdateSolutionCoverage(CoverageResult coverage)
        {
            foreach (string docPath in coverage.CoverageByDocument.Keys)
            {
                SolutionCoverageByDocument[docPath] = coverage.CoverageByDocument[docPath].ToList();
            }
        }
    }
}