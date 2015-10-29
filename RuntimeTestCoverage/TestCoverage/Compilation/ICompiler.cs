using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using TestCoverage.Rewrite;

namespace TestCoverage.Compilation
{
    public interface ICompiler
    {
        ICompiledItem[] Compile(IEnumerable<CompilationItem> allItems, AuditVariablesMap auditVariablesMap);
        ICompiledItem[] Compile(CompilationItem item, IEnumerable<_Assembly> references, AuditVariablesMap auditVariablesMap);
    }
}