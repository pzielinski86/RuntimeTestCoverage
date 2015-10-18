using System;
using System.IO;
using System.Reflection;

namespace TestCoverage
{
    public sealed class AppDomainSolutionCoverageEngine:ISolutionCoverageEngine,IDisposable
    {
        private ISolutionCoverageEngine _coverageEngine;
        private readonly AppDomain _appDomain;

        public AppDomainSolutionCoverageEngine()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var appDomainSetup = new AppDomainSetup {LoaderOptimization = LoaderOptimization.MultiDomain};

            _appDomain = AppDomain.CreateDomain("coverage", null, appDomainSetup);
            Type engineType = typeof (SolutionCoverageEngine);

            string path = Path.Combine(Directory.GetCurrentDirectory(), engineType.Assembly.ManifestModule.Name);
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
            AppDomain.Unload(_appDomain);
        }
    }
}