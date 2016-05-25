using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using TestCoverage.Compilation;

namespace TestCoverage.CoverageCalculation
{
    public class NUnitTestExtractor : ITestsExtractor
    {
        public TestFixtureDetails GetTestFixtureDetails(ClassDeclarationSyntax fixtureNode, ISemanticModel semanticModel)
        {
            var fixture = ExtractTests(fixtureNode, semanticModel);
            fixture.AssemblyName = PathHelper.GetCoverageDllName(semanticModel.GetAssemblyName());

            return fixture;
        }

        public ClassDeclarationSyntax[] GetTestClasses(SyntaxNode root)
        {
            return root.DescendantNodes().OfType<AttributeSyntax>()
                .Where(a => a.Name.ToString() == "TestFixture")
                .Select(a => a.Parent.Parent).OfType<ClassDeclarationSyntax>().ToArray();
        }

        private TestFixtureDetails ExtractTests(ClassDeclarationSyntax testClass, ISemanticModel semanticModel)
        {
            var testFixture = new TestFixtureDetails();
            testFixture.FullClassName = semanticModel.GetFullName(testClass);

            foreach (MethodDeclarationSyntax methodNode in testClass.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                ExtractMethodTests(testFixture, methodNode, semanticModel);
            }

            return testFixture;
        }

        private void ExtractMethodTests(TestFixtureDetails testFixture, MethodDeclarationSyntax methodNode, ISemanticModel semanticModel)
        {
            var methodTestCases = new List<TestCase>();

            foreach (AttributeSyntax attribute in methodNode.DescendantNodes().OfType<AttributeSyntax>())
            {
                if (attribute.Name.ToString() == "Test")
                {
                    var testCase = ExtractTest(attribute, testFixture);
                    if (testCase != null)
                        methodTestCases.Add(testCase);
                }

                else if (attribute.Name.ToString() == "TestCase")
                {
                    var testCase = ExtractTestCase(attribute, testFixture, semanticModel);
                    if (testCase != null)
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
        private static TestCase ExtractTestCase(AttributeSyntax attribute, TestFixtureDetails testFixture, ISemanticModel semanticModel)
        {
            var methodDeclarationSyntax = GetAttributeMethod(attribute);
            bool isAsync = IsAsync(methodDeclarationSyntax);

            if (isAsync && IsVoid(methodDeclarationSyntax))
                return null;

            var testCase = new TestCase(testFixture);
            testCase.IsAsync = isAsync;

            testCase.Arguments = new object[attribute.ArgumentList.Arguments.Count];

            for (int i = 0; i < testCase.Arguments.Length; i++)
            {
                AttributeArgumentSyntax testCaseArg = attribute.ArgumentList.Arguments[i];

                object constantValue = semanticModel.GetConstantValue(testCaseArg.Expression);

                if (constantValue != null)
                    testCase.Arguments[i] = constantValue;
                else
                    testCase.Arguments[i] = testCaseArg.Expression.GetFirstToken().Value;
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
            bool isAsync = IsAsync(methodDeclarationSyntax);

            if (isAsync && IsVoid(methodDeclarationSyntax))
                return null;

            var testCase = new TestCase(testFixture)
            {
                SyntaxNode = methodDeclarationSyntax,
                IsAsync = isAsync,
                MethodName = methodDeclarationSyntax.Identifier.ValueText,
                Arguments = new string[0]
            };

            return testCase;
        }

        private static bool IsVoid(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            return IsAsync(methodDeclarationSyntax) && methodDeclarationSyntax.ReturnType.ToString() == "void";
        }

        private static bool IsAsync(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            return methodDeclarationSyntax.DescendantTokens().Any(x => x.ValueText == "async");
        }
    }
}