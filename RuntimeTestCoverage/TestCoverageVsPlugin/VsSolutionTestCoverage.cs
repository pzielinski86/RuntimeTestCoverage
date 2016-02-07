using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestCoverage;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Extensions;
using TestCoverage.Storage;
using TestCoverageVsPlugin.Extensions;
using Task = System.Threading.Tasks.Task;

namespace TestCoverageVsPlugin
{
    public sealed class VsSolutionTestCoverage : IVsSolutionTestCoverage,IDisposable
    {
        public string SolutionPath { get; }
        private readonly ISolutionCoverageEngine _solutionCoverageEngine;
        private readonly ICoverageStore _coverageStore;
        private static VsSolutionTestCoverage _vsSolutionTestCoverage;
        private static readonly object SyncObject = new object();
        private readonly object _sync = new object();
        private readonly ILogger _logger;

        public VsSolutionTestCoverage(string solutionPath,
            ISolutionCoverageEngine solutionCoverageEngine,
            ICoverageStore coverageStore,
            ILogger logger)
        {
            SolutionPath = solutionPath;
            _solutionCoverageEngine = solutionCoverageEngine;
            _coverageStore = coverageStore;
            _logger = logger;
            SolutionCoverageByDocument = new Dictionary<string, List<LineCoverage>>();
        }

        public static VsSolutionTestCoverage CreateInstanceIfDoesNotExist(string solutionPath, ISolutionCoverageEngine solutionCoverageEngine, ICoverageStore coverageStore, ILogger logger)
        {
            if (_vsSolutionTestCoverage == null)
            {
                lock (SyncObject)
                {
                    if (_vsSolutionTestCoverage == null)
                    {
                        _vsSolutionTestCoverage = new VsSolutionTestCoverage(solutionPath, solutionCoverageEngine, coverageStore, logger);
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

        public async Task CalculateForAllDocumentsAsync()
        {
            CoverageResult coverage;
            Reinit();

            try
            {
                coverage = await _solutionCoverageEngine.CalculateForAllDocumentsAsync();
            }
            catch (TestCoverageCompilationException e)
            {
                SolutionCoverageByDocument.Clear();
                _logger.Write(e.ToString());
                return;
            }

            SolutionCoverageByDocument = coverage.CoverageByDocument.ToDictionary(x => x.Key, x => x.Value.ToList());
        }

        public Task CalculateForSelectedMethodAsync(string projectName, MethodDeclarationSyntax method)
        {
            var task = Task.Factory.StartNew(() =>
            {
                if (method != null)
                {
                    List<LineCoverage> coverage;

                    try
                    {
                        var result = _solutionCoverageEngine.CalculateForMethod(projectName,method);

                        coverage = result.CoverageByDocument.SelectMany(x => x.Value).ToList();
                    }
                    catch (TestCoverageCompilationException e)
                    {
                        string path = NodePathBuilder.BuildPath(method,
                            Path.GetFileNameWithoutExtension(method.SyntaxTree.FilePath), projectName);

                        SolutionCoverageByDocument.RemvoeByPath(path);
                        _logger.Write(e.ToString());
                        return;
                    }

                    SolutionCoverageByDocument.MergeByNodePath(coverage);
                }
            },CancellationToken.None,TaskCreationOptions.None, PriorityScheduler.BelowNormal);

            return task;
        }

        public void Reinit()
        {
            _solutionCoverageEngine.Init(SolutionPath);
        }

        public Task CalculateForDocumentAsync(string projectName, string documentPath, string documentContent)
        {
            return Task.Run(() => CalculateForDocument(projectName, documentPath, documentContent));
        }

        public void CalculateForDocument(string projectName, string documentPath, string documentContent)
        {
            CoverageResult coverage;

            try
            {
                coverage = _solutionCoverageEngine.CalculateForDocument(projectName, documentPath, documentContent);
            }
            catch (TestCoverageCompilationException e)
            {
                SolutionCoverageByDocument.Clear();
                _logger.Write(e.ToString());
                return;
            }

            UpdateSolutionCoverage(coverage);
        }

        private void UpdateSolutionCoverage(CoverageResult coverage)
        {
            foreach (string docPath in coverage.CoverageByDocument.Keys)
            {
                SolutionCoverageByDocument[docPath] = coverage.CoverageByDocument[docPath].ToList();
            }
        }

        public void Dispose()
        {
            _vsSolutionTestCoverage = null;
        }
    }
}