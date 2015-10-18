using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.CoverageCalculation
{
    public class NUnitTestExtractor : ITestsExtractor
    {
        public TestFixtureDetails GetTestFixtureDetails(ClassDeclarationSyntax fixtureNode)
        {
            return ExtractTestCases(fixtureNode);
        }

        public ClassDeclarationSyntax[] GetTestClasses(SyntaxNode root)
        {
            return root.DescendantNodes().OfType<AttributeSyntax>()
                .Where(a => a.Name.ToString() == "TestFixture")
                .Select(a => a.Parent.Parent).OfType<ClassDeclarationSyntax>().ToArray();
        }

        private TestFixtureDetails ExtractTestCases(ClassDeclarationSyntax testClass)
        {
            var testFixture=new TestFixtureDetails();
            var namespaceNode = testClass.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            testFixture.Namespace = namespaceNode?.Name.ToString();
            testFixture.ClassName = testClass.Identifier.ValueText;

            foreach (MethodDeclarationSyntax methodNode in testClass.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var methodTestCases = new List<TestCase>();

                foreach (AttributeSyntax attribute in methodNode.DescendantNodes().OfType<AttributeSyntax>())
                {
                    if (attribute.Name.ToString() == "Test")
                    {
                        var testCase = ExtractTest(attribute, testFixture);
                        methodTestCases.Add(testCase);
                    }

                    else if (attribute.Name.ToString() == "TestCase")
                    {
                        var testCase = ExtractTestCase(attribute, testFixture);
                        methodTestCases.Add(testCase);
                    }
                    else if (attribute.Name.ToString() == "SetUp")
                        testFixture.SetupMethodName = GetAttributeMethod(attribute).Identifier.ValueText;
                }

                if (methodTestCases.Count > 0)
                {
                    int maxPars = methodTestCases.Max(x => x.Arguments.Length);
                    testFixture.Cases.AddRange(methodTestCases.Where(x => x.Arguments.Length == maxPars));
                }
            }

            return testFixture;
        }
        
        private static TestCase ExtractTestCase(AttributeSyntax attribute, TestFixtureDetails testFixture)
        {
            var testCase = new TestCase(testFixture);
            var methodDeclarationSyntax = GetAttributeMethod(attribute);

            testCase.Arguments = new string[attribute.ArgumentList.Arguments.Count];

            for (int i = 0; i < testCase.Arguments.Length; i++)
            {
                AttributeArgumentSyntax testCaseArg = attribute.ArgumentList.Arguments[i];
                testCase.Arguments[i] = testCaseArg.Expression.GetText().ToString();
            }

            testCase.SyntaxNode = methodDeclarationSyntax;
            testCase.MethodName = methodDeclarationSyntax.Identifier.ValueText;

            return testCase;
        }

        private static MethodDeclarationSyntax GetAttributeMethod(AttributeSyntax attribute)
        {
            return (MethodDeclarationSyntax)attribute.Parent.Parent;
        }

        private static TestCase ExtractTest(AttributeSyntax attribute, TestFixtureDetails testFixture)
        {
            var methodDeclarationSyntax = GetAttributeMethod(attribute);

            var testCase = new TestCase(testFixture)
            {
                SyntaxNode = methodDeclarationSyntax,
                MethodName = methodDeclarationSyntax.Identifier.ValueText,
                Arguments = new string[0]
            };

            return testCase;
        }
    }
}