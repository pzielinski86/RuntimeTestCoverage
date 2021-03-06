﻿using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace TestCoverage.CoverageCalculation
{
    public sealed class AppDomainTestExecutorScriptEngine : MarshalByRefObject, ITestExecutorScriptEngine, IDisposable
    {
        private AppDomain _appDomain;
        private readonly TestExecutorScriptEngine _engine;

        public AppDomainTestExecutorScriptEngine()
        {
            var appDomainSetup = new AppDomainSetup { LoaderOptimization = LoaderOptimization.MultiDomain };
            _appDomain = AppDomain.CreateDomain("testing_sandbox", null, appDomainSetup);
            Type engineType = typeof(TestExecutorScriptEngine);

            string currentDir = Path.GetDirectoryName(GetType().Assembly.Location);
            string path = Path.Combine(currentDir, engineType.Assembly.ManifestModule.Name);
            _engine = (TestExecutorScriptEngine)_appDomain.CreateInstanceFromAndUnwrap(path,
                engineType.FullName);


        }

        public ITestRunResult[] RunTestFixture(string[] references, TestFixtureExecutionScriptParameters pars)
        {
            return _engine.RunTestFixture(references, pars);
        }

        public void Dispose()
        {
            AppDomain.Unload(_appDomain);
            _appDomain = null;
        }

      
    }
}