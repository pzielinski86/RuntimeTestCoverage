using System;
using System.Reflection;
using System.Runtime.Remoting;
using Microsoft.CodeAnalysis;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestExecutorScriptEngine
    {
        TestRunResult RunTest(MetadataReference[] references, Assembly[] assemblies, SyntaxNode method, AuditVariablesMap auditVariablesMap);
    }
}