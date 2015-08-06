using System;
using System.Reflection;
using System.Runtime.Remoting;
using Microsoft.CodeAnalysis;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestExecutorScriptEngine
    {
        Tuple<string[],bool> RunTest(MetadataReference[] references, Assembly[] assemblies, SyntaxNode method, AuditVariablesMap auditVariablesMap);
        object GetLifetimeService();
        object InitializeLifetimeService();
        ObjRef CreateObjRef(Type requestedType);
    }
}