using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace TestCoverage
{
    public sealed class AppDomainSolutionCoverageEngine:ISolutionCoverageEngine
    {
        private ISolutionCoverageEngine _coverageEngine;
        private readonly AppDomain _appDomain;
        private bool _isDisposed;

        public AppDomainSolutionCoverageEngine()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var appDomainSetup = new AppDomainSetup {LoaderOptimization = LoaderOptimization.MultiDomain};

            _appDomain = AppDomain.CreateDomain("coverage", null, appDomainSetup);
            Type engineType = typeof (SolutionCoverageEngine);

            string currentDir = Path.GetDirectoryName(GetType().Assembly.Location);
            string path = Path.Combine(currentDir, engineType.Assembly.ManifestModule.Name);
            _coverageEngine = (SolutionCoverageEngine)_appDomain.CreateInstanceFromAndUnwrap(path,
                engineType.FullName);            
        }

        public void Init(string solutionPath)
        {
            _coverageEngine.Init(solutionPath);
        }

        public CoverageResult CalculateForAllDocuments()
        {
            return _coverageEngine.CalculateForAllDocuments();
        }

        public CoverageResult CalculateForDocument(string projectName, string documentPath, string documentContent)
        {
            return _coverageEngine.CalculateForDocument(projectName, documentPath, documentContent);
        }

        public bool IsDisposed => _isDisposed;

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = typeof (SolutionCoverageEngine).Assembly;
            
            if (args.Name == assembly.FullName) 
            {         
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                
                return Assembly.LoadFrom(assembly.Location);
            }

            return null;
        }

        public void Dispose()
        {
            _coverageEngine = null;
            _isDisposed = true;
            AppDomain.Unload(_appDomain);
        }
    }
}