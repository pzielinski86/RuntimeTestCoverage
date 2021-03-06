﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiveCoverageVsPlugin.Extensions;
using LiveCoverageVsPlugin.Logging;
using log4net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Storage;
using TestCoverage.Tasks;
using Task = System.Threading.Tasks.Task;

namespace LiveCoverageVsPlugin
{
    public sealed class VsSolutionTestCoverage : IVsSolutionTestCoverage, IDisposable
    {
        // Singleton
        private static IVsSolutionTestCoverage _vsSolutionTestCoverage;

        private ILog logger = LogFactory.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());

        public string SolutionPath => MyWorkspace.CurrentSolution.FilePath;
        public Workspace MyWorkspace { get; }
        private readonly ISolutionCoverageEngine _solutionCoverageEngine;
        private readonly ICoverageStore _coverageStore;
        private static readonly object SyncObject = new object();
        public Dictionary<string, List<LineCoverage>> SolutionCoverageByDocument { get; private set; }

        public VsSolutionTestCoverage(Workspace myWorkspace,
            ISolutionCoverageEngine solutionCoverageEngine,
            ICoverageStore coverageStore)
        {
            MyWorkspace = myWorkspace;
            _solutionCoverageEngine = solutionCoverageEngine;
            _coverageStore = coverageStore;

            SolutionCoverageByDocument = new Dictionary<string, List<LineCoverage>>();
        }

        public static IVsSolutionTestCoverage CreateInstanceIfDoesNotExist(Workspace myWorkspace,
            ISolutionCoverageEngine solutionCoverageEngine,
            ICoverageStore coverageStore)
        {
            if (_vsSolutionTestCoverage == null)
            {
                lock (SyncObject)
                {
                    if (_vsSolutionTestCoverage == null)
                    {
                        _vsSolutionTestCoverage = new ProfiledVsSolutionTestCoverage(new VsSolutionTestCoverage(myWorkspace, solutionCoverageEngine, coverageStore));
                        _vsSolutionTestCoverage.Reinit();
                        _vsSolutionTestCoverage.LoadCurrentCoverage();
                    }
                }
            }

            return _vsSolutionTestCoverage;
        }

        public async Task<bool> CalculateForAllDocumentsAsync()
        {
            logger.Info("Calculating coverage for all documents");

            CoverageResult coverage;
            Reinit();

            try
            {
                coverage = await _solutionCoverageEngine.CalculateForAllDocumentsAsync();
            }
            catch (TestCoverageCompilationException e)
            {
                SolutionCoverageByDocument.Clear();
                logger.Error(e);
                return false;
            }

            SolutionCoverageByDocument = coverage.CoverageByDocument.ToDictionary(x => x.Key, x => x.Value.ToList());

            return true;
        }

        public Task<bool> CalculateForSelectedMethodAsync(string projectName, MethodDeclarationSyntax method)
        {
            var task = Task.Factory.StartNew(() =>
            {
                List<LineCoverage> coverage;

                try
                {
                    var result = _solutionCoverageEngine.CalculateForMethod(projectName, method);

                    coverage = result.CoverageByDocument.SelectMany(x => x.Value).ToList();
                }
                catch (TestCoverageCompilationException e)
                {
                    string path = NodePathBuilder.BuildPath(method,
                        Path.GetFileNameWithoutExtension(method.SyntaxTree.FilePath), projectName);

                    SolutionCoverageByDocument.MarkMethodAsCompilationError(path, e.ToString());
                    logger.Error(e);
                    return false;
                }


                SolutionCoverageByDocument.MergeByNodePath(coverage);

                return true;
            }, CancellationToken.None, TaskCreationOptions.None, PriorityScheduler.BelowNormal);

            return task;
        }

        public void Reinit()
        {
            _solutionCoverageEngine.Init(MyWorkspace);
        }

        public Task<bool> CalculateForDocumentAsync(string projectName, string documentPath, string documentContent)
        {
            return Task.Run(() => CalculateForDocument(projectName, documentPath, documentContent));
        }

        public void RemoveByPath(string filePath)
        {
            if (!SolutionCoverageByDocument.ContainsKey(filePath))
                return;

            var allTestPaths = SolutionCoverageByDocument[filePath].Select(x => x.TestPath).ToArray();

            SolutionCoverageByDocument.Remove(filePath);

            foreach (var documentCoverage in SolutionCoverageByDocument.Values)
            {
                documentCoverage.RemoveAll(x => allTestPaths.Contains(x.TestPath));
            }
        }

        private bool CalculateForDocument(string projectName, string documentPath, string documentContent)
        {
            CoverageResult coverage;

            try
            {
                coverage = _solutionCoverageEngine.CalculateForDocument(projectName, documentPath, documentContent);
            }
            catch (TestCoverageCompilationException e)
            {
                SolutionCoverageByDocument.MarkDocumentAsCompilationError(documentPath, e.ToString());
                logger.Error(e);
                return false;
            }

            SolutionCoverageByDocument.UpdateDocumentCoverage(documentPath, coverage);

            return true;
        }

        public void Dispose()
        {
            _vsSolutionTestCoverage = null;
        }

        public void LoadCurrentCoverage()
        {  
            LineCoverage[] coverage = _coverageStore.ReadAll();

            SolutionCoverageByDocument = coverage.GroupBy(x => x.DocumentPath).ToDictionary(x => x.Key, x => x.ToList());
        }
    }
}