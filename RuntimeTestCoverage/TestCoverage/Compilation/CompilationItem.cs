using Microsoft.CodeAnalysis;

namespace TestCoverage.Compilation
{
    public class CompilationItem
    {
        public Project Project { get; private set; }
        public SyntaxTree[] SyntaxTrees { get; private set; }

        public CompilationItem(Project project, SyntaxTree[]
            syntaxTrees)
        {
            Project = project;
            SyntaxTrees = syntaxTrees;
        }
    }
}