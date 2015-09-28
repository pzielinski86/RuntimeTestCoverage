using System;
using System.Reflection;

namespace TestCoverage
{
    public class AppDomainSolutionCoverageEngine:ISolutionCoverageEngine
    {
        private const string SandBoxDllName = "CoverageSandbox.dll";
        private ISolutionCoverageEngine _coverageEngine;

        public AppDomainSolutionCoverageEngine()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var appDomainSetup = new AppDomainSetup {LoaderOptimization = LoaderOptimization.MultiDomain};

            var domain = AppDomain.CreateDomain("coverage", null, appDomainSetup);
            _coverageEngine = (SolutionCoverageEngine)domain.CreateInstanceFromAndUnwrap(SandBoxDllName, typeof(SolutionCoverageEngine).FullName);            
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

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("TestCoverage"))
            {
                string path = Assembly.GetExecutingAssembly().Location;
                path = System.IO.Path.GetDirectoryName(path);

                return Assembly.LoadFrom(System.IO.Path.Combine(path, "TestCoverage.dll"));
            }
            return null;
        }
    }
}