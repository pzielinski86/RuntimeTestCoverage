using System;
using System.Reflection;

namespace TestCoverage
{
    public sealed class AppDomainSolutionCoverageEngine:ISolutionCoverageEngine,IDisposable
    {
        private ISolutionCoverageEngine _coverageEngine;
        private AppDomain _appDomain;

        public AppDomainSolutionCoverageEngine()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var appDomainSetup = new AppDomainSetup {LoaderOptimization = LoaderOptimization.MultiDomain};

            _appDomain = AppDomain.CreateDomain("coverage", null, appDomainSetup);
            Type engineType = typeof (SolutionCoverageEngine);

            _coverageEngine = (SolutionCoverageEngine)_appDomain.CreateInstanceFromAndUnwrap(engineType.Assembly.ManifestModule.Name,
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

        public CoverageResult CalculateForTest(string projectName, string documentPath, string documentContent, string className,
            string methodName)
        {
            return _coverageEngine.CalculateForTest(projectName, documentPath, documentContent, className, methodName);
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