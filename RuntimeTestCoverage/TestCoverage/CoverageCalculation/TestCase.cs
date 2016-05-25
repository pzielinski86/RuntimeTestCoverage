using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public class TestCase
    {
        public TestCase(TestFixtureDetails testFixture)
        {
            TestFixture = testFixture;
            Arguments=new object[0];
        }

        public TestFixtureDetails TestFixture { get; }

        public object[] Arguments { get; set; }
        public string MethodName { get; set; }

        public MethodDeclarationSyntax SyntaxNode { get; set; }
        public bool IsAsync { get; set; }

    }
}