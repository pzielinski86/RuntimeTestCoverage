using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.CoverageCalculation
{
    public class NUnitTestExtractor : ITestsExtractor
    {
        public TestCase[] GetTestCases(ClassDeclarationSyntax testClass)
        {
            var testMethods = testClass.DescendantNodes()
                .OfType<AttributeSyntax>()
                .Where(a => a.Name.ToString() == "Test")
                .Select(a => a.Parent.Parent).
                OfType<MethodDeclarationSyntax>().ToArray();


            return testMethods.Select(x=>ExtractTestCase(testClass,x)).ToArray();
        }

        private TestCase ExtractTestCase(ClassDeclarationSyntax testClass, MethodDeclarationSyntax methodDeclarationSyntax)
        {
            var testCase = new TestCase();

            testCase.Namespace = testClass.Ancestors().OfType<NamespaceDeclarationSyntax>().First().Name.ToString();
            testCase.ClassName = testClass.Identifier.Text;            
            testCase.MethodName = methodDeclarationSyntax.Identifier.ValueText;
            testCase.Arguments=new object[0];

            return testCase;
        }

        public SyntaxNode[] GetTestClasses(SyntaxNode root)
        {
            return root.DescendantNodes().OfType<AttributeSyntax>()
                .Where(a => a.Name.ToString() == "TestFixture")
                .Select(a => a.Parent.Parent).ToArray();
        }
    }
}