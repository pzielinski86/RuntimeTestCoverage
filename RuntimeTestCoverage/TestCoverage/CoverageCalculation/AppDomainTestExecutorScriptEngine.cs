using System;
using System.IO;
using System.Reflection;

namespace TestCoverage.CoverageCalculation
{
    public sealed class AppDomainTestExecutorScriptEngine : ITestExecutorScriptEngine, IDisposable
    {
        private AppDomain _appDomain;
        private readonly TestExecutorScriptEngine _engine;

        public AppDomainTestExecutorScriptEngine()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var appDomainSetup = new AppDomainSetup { LoaderOptimization = LoaderOptimization.MultiDomain };

            _appDomain = AppDomain.CreateDomain("testing_sandbox", null, appDomainSetup);
            Type engineType = typeof(TestExecutorScriptEngine);

            string currentDir = Path.GetDirectoryName(GetType().Assembly.Location);
            string path = Path.Combine(currentDir, engineType.Assembly.ManifestModule.Name);
            _engine = (TestExecutorScriptEngine)_appDomain.CreateInstanceFromAndUnwrap(path,
                engineType.FullName);
        }

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = typeof(SolutionCoverageEngine).Assembly;

            if (args.Name == assembly.FullName)
            {                
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

                return Assembly.LoadFrom(assembly.Location);
            }

            return null;
        }

        public ITestRunResult RunTest(string[] references, string code)
        {
            return _engine.RunTest(references, code);
        }

        public void Dispose()
        {
            AppDomain.Unload(_appDomain);
            _appDomain = null;
        }
    }
}