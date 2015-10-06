using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.CoverageCalculation
{
    public class TestCase
    {
        public object[] Arguments { get; set; }
        public string MethodName { get; set; }
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public MethodDeclarationSyntax SyntaxNode { get; set; }
    }
}