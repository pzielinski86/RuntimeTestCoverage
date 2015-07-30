using System.Collections.Generic;
using System.Reflection;
using TestCoverage.Rewrite;

namespace TestCoverage.Compilation
{
    public interface ICompiler
    {
        Assembly[] Compile(IEnumerable<CompilationItem> allItems, AuditVariablesMap auditVariablesMap);
        Assembly[] Compile(CompilationItem item, IEnumerable<Assembly> references, AuditVariablesMap auditVariablesMap);
    }
}