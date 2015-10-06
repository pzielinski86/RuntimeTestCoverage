using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.CoverageCalculation
{
    public class NUnitTestExtractor : ITestsExtractor
    {
        public TestCase[] GetTestCases(ClassDeclarationSyntax testClass)
        {
            var foundTestCases= ExtractTestCases(testClass);
            if (foundTestCases.Length == 0)
                return foundTestCases;

            int maxPars = foundTestCases.Max(x => x.Arguments.Length);

            return foundTestCases.Where(x => x.Arguments.Length == maxPars).ToArray();
        }

        private TestCase[] ExtractTestCases(ClassDeclarationSyntax testClass)
        {
            var testCases=new List<TestCase>();
            var namespaceNode = testClass.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            string nameSpace = namespaceNode?.Name.ToString();

            foreach (AttributeSyntax attribute in testClass.DescendantNodes().OfType<AttributeSyntax>())
            {
                if (attribute.Name.ToString() == "Test")
                {
                    var testCase = ExtractTest(testClass, attribute, nameSpace);
                    testCases.Add(testCase);
                }

                else if (attribute.Name.ToString() == "TestCase")
                {
                    var testCase = ExtractTestCase(testClass, attribute, nameSpace);
                    testCases.Add(testCase);
                }
            }

            return testCases.ToArray();
        }

        private static TestCase ExtractTestCase(ClassDeclarationSyntax testClass, AttributeSyntax attribute, string nameSpace)
        {
            var testCase = new TestCase();
            var methodDeclarationSyntax = (MethodDeclarationSyntax) attribute.Parent.Parent;

            testCase.Arguments = new object[attribute.ArgumentList.Arguments.Count];

            for (int i = 0; i < testCase.Arguments.Length; i++)
            {
                AttributeArgumentSyntax testCaseArg = attribute.ArgumentList.Arguments[i];
                testCase.Arguments[i] = testCaseArg.Expression.GetFirstToken().Value;
            }

            testCase.SyntaxNode = methodDeclarationSyntax;
            testCase.Namespace = nameSpace;
            testCase.ClassName = testClass.Identifier.Text;
            testCase.MethodName = methodDeclarationSyntax.Identifier.ValueText;

            return testCase;
        }

        private static TestCase ExtractTest(ClassDeclarationSyntax testClass, AttributeSyntax attribute, string nameSpace)
        {
            var methodDeclarationSyntax = (MethodDeclarationSyntax) attribute.Parent.Parent;

            var testCase = new TestCase
            {
                SyntaxNode = methodDeclarationSyntax,
                Namespace = nameSpace,
                ClassName = testClass.Identifier.Text,
                MethodName = methodDeclarationSyntax.Identifier.ValueText,
                Arguments = new object[0]
            };

            return testCase;
        }

        public ClassDeclarationSyntax[] GetTestClasses(SyntaxNode root)
        {
            return root.DescendantNodes().OfType<AttributeSyntax>()
                .Where(a => a.Name.ToString() == "TestFixture")
                .Select(a => a.Parent.Parent).OfType<ClassDeclarationSyntax>().ToArray();
        }
    }
}