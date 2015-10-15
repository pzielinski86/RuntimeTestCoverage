using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage
{
    public class TestProject
    {
        public TestProject()
        {
            TestFixtures=new ClassDeclarationSyntax[0];
        }
        public Project Project { get; set; }
        public ClassDeclarationSyntax[] TestFixtures { get; set; }
        public bool IsCoverageEnabled { get; set; }
    }
}