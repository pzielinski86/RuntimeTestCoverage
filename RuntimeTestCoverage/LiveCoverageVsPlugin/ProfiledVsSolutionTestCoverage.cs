using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveCoverageVsPlugin.Performance;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.CoverageCalculation;
using TestCoverage.Tasks;

namespace LiveCoverageVsPlugin
{
    class ProfiledVsSolutionTestCoverage : IVsSolutionTestCoverage
    {
        private readonly IVsSolutionTestCoverage vsSolutionTestCoverage;

        public ProfiledVsSolutionTestCoverage(IVsSolutionTestCoverage vsSolutionTestCoverage)
        {
            this.vsSolutionTestCoverage = vsSolutionTestCoverage;
        }
        public string SolutionPath => vsSolutionTestCoverage.SolutionPath;

        public Dictionary<string, List<LineCoverage>> SolutionCoverageByDocument => vsSolutionTestCoverage.SolutionCoverageByDocument;

        public Workspace MyWorkspace => vsSolutionTestCoverage.MyWorkspace;

        public Task<bool> CalculateForAllDocumentsAsync()
        {
            Benchmark benchmark = new Benchmark("ProfiledVsSolutionTestCoverage.CalculateForAllDocuments");

            return vsSolutionTestCoverage.CalculateForAllDocumentsAsync().ContinueWith((task) =>
            {
                benchmark.Stop();
                return task.Result;
            }, TaskScheduler.Default);
        }

        public Task<bool> CalculateForDocumentAsync(string projectName, string documentPath, string documentContent)
        {
            Benchmark benchmark = new Benchmark("ProfiledVsSolutionTestCoverage.CalculateForDocumentAsync");

            return vsSolutionTestCoverage.CalculateForDocumentAsync(projectName, documentPath, documentContent).ContinueWith((task) =>
            {
                benchmark.Stop();
                return task.Result;
            }, TaskScheduler.Default);
        }

        public Task<bool> CalculateForSelectedMethodAsync(string projectName, MethodDeclarationSyntax method)
        {
            Benchmark benchmark = new Benchmark("ProfiledVsSolutionTestCoverage.CalculateForSelectedMethodAsync");

            return vsSolutionTestCoverage.CalculateForSelectedMethodAsync(projectName, method).ContinueWith((task) =>
            {
                benchmark.Stop();
                return task.Result;
            }, TaskScheduler.Default);
        }

        public void Dispose()
        {
            Benchmark.Profile("ProfiledVsSolutionTestCoverage.Dispose", () => vsSolutionTestCoverage.Dispose());
        }

        public void LoadCurrentCoverage()
        {
            Benchmark.Profile("ProfiledVsSolutionTestCoverage.LoadCurrentCoverage", () => vsSolutionTestCoverage.LoadCurrentCoverage());
        }

        public void Reinit()
        {
            Benchmark.Profile("ProfiledVsSolutionTestCoverage.Reinit", () => vsSolutionTestCoverage.Reinit());
        }

        public void RemoveByPath(string filePath)
        {
            Benchmark.Profile("ProfiledVsSolutionTestCoverage.RemoveByPath", () => vsSolutionTestCoverage.RemoveByPath(filePath));
        }
    }
}
