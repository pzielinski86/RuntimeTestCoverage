using System;
using System.Reflection;
using System.Runtime.Remoting;
using Microsoft.CodeAnalysis;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestExecutorScriptEngine
    {
        ITestRunResult RunTest(MetadataReference[] references, Assembly[] assemblies, TestCase method, AuditVariablesMap auditVariablesMap);
    }
}