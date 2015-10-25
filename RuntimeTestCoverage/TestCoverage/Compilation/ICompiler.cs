using System.Collections.Generic;
using System.Reflection;
using TestCoverage.Rewrite;

namespace TestCoverage.Compilation
{
    public interface ICompiler
    {
        CompiledItem[] Compile(IEnumerable<CompilationItem> allItems, AuditVariablesMap auditVariablesMap);
        CompiledItem[] Compile(CompilationItem item, IEnumerable<Assembly> references, AuditVariablesMap auditVariablesMap);
    }
}