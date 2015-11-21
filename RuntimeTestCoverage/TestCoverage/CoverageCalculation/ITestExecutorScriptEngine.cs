using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using Microsoft.CodeAnalysis;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestExecutorScriptEngine
    {
        ITestRunResult RunTest(string[] references, string code);
    }
}