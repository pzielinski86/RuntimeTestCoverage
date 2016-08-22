using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using TestCoverage;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Extensions;
using TestCoverage.Monitors;
using TestCoverage.Storage;
using TestCoverageVsPlugin.Annotations;
using TestCoverageVsPlugin.Extensions;
using TestCoverageVsPlugin.Logging;
using Task = System.Threading.Tasks.Task;

namespace TestCoverageVsPlugin
{
    public sealed class VsSolutionTestCoverage : IVsSolutionTestCoverage, IDisposable
    {
        // Singleton
        private static VsSolutionTestCoverage _vsSolutionTestCoverage;

        public string SolutionPath => MyWorkspace.CurrentSolution.FilePath;
        public Workspace MyWorkspace { get; }
        private readonly ISolutionCoverageEngine _solutionCoverageEngine;
        private readonly ICoverageStore _coverageStore;
        private static readonly object SyncObject = new object();
        public Dictionary<string, List<LineCoverage>> SolutionCoverageByDocument { get; private set; }

        // TODO: Remove logger from constructor
        public VsSolutionTestCoverage(Workspace myWorkspace,
            ISolutionCoverageEngine solutionCoverageEngine,
            ICoverageStore coverageStore,
            ISolutionWatcher solutionWatcher)
        {
            MyWorkspace = myWorkspace;
            _solutionCoverageEngine = solutionCoverageEngine;
            _coverageStore = coverageStore;
            solutionWatcher.Start();
            solutionWatcher.DocumentRemoved += SolutionWatcher_DocumentRemoved;

            SolutionCoverageByDocument = new Dictionary<string, List<LineCoverage>>();
        }  

        public static VsSolutionTestCoverage CreateInstanceIfDoesNotExist(Workspace myWorkspace, 
            ISolutionCoverageEngine solutionCoverageEngine, 
            ICoverageStore coverageStore, 
            ISolutionWatcher solutionWatcher)
        {
            if (_vsSolutionTestCoverage == null)
            {
                lock (SyncObject)
                {
                    if (_vsSolutionTestCoverage == null)
                    {
                        _vsSolutionTestCoverage = new VsSolutionTestCoverage(myWorkspace, solutionCoverageEngine, coverageStore,solutionWatcher);
                        _vsSolutionTestCoverage.Reinit();
                        _vsSolutionTestCoverage.LoadCurrentCoverage();
                    }
                }
            }

            return _vsSolutionTestCoverage;
        }

        public async Task CalculateForAllDocumentsAsync()
        {
            LogFactory.CurrentLogger.Info("Calculating coverage for all documents");

            CoverageResult coverage;
            Reinit();

            try
            {
                var task = Task.Factory.StartNew(() => _solutionCoverageEngine.CalculateForAllDocumentsAsync().Result);
                coverage = await task;
            }

            catch (TestCoverageCompilationException e)
            {
                SolutionCoverageByDocument.Clear();
                LogFactory.CurrentLogger.Error(e.ToString());
                return;
            }

            SolutionCoverageByDocument = coverage.CoverageByDocument.ToDictionary(x => x.Key, x => x.Value.ToList());
        }

        public Task<bool> CalculateForSelectedMethodAsync(string projectName, MethodDeclarationSyntax method)
        {
            var task = Task.Factory.StartNew<bool>(() =>
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

                    SolutionCoverageByDocument.MarkAsCompilationError(path,e.ToString());
                    LogFactory.CurrentLogger.Error(e.ToString());
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

        private void RemoveByPath(string filePath)
        {
            var allTestPaths = SolutionCoverageByDocument[filePath].Select(x => x.TestPath).ToArray();    

            SolutionCoverageByDocument.Remove(filePath);

            foreach (var documentCoverage in SolutionCoverageByDocument.Values)
            {
                documentCoverage.RemoveAll(x => allTestPaths.Contains(x.TestPath));
            }
        }

        private void SolutionWatcher_DocumentRemoved(object sender, DocumentRemovedEventArgs e)
        {
            RemoveByPath(e.DocumentPath);
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
                SolutionCoverageByDocument.Clear();
                LogFactory.CurrentLogger.Error(e.ToString());
                return false;
            }

            UpdateSolutionCoverage(coverage);

            return true;
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

        public void LoadCurrentCoverage()
        {
            LineCoverage[] coverage = _coverageStore.ReadAll();

            SolutionCoverageByDocument = coverage.GroupBy(x => x.DocumentPath).ToDictionary(x => x.Key, x => x.ToList());
        }
    }
}