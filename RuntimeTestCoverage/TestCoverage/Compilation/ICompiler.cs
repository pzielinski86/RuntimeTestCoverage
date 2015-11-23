using System.Collections.Generic;

namespace TestCoverage.Compilation
{
    public interface ICompiler
    {
        ICompiledItem[] Compile(IEnumerable<CompilationItem> allItems);
        ICompiledItem[] Compile(CompilationItem item, IEnumerable<string> references);
    }
}