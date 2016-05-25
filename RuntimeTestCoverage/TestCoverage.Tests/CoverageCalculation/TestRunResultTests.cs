using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Linq;
using TestCoverage.CoverageCalculation;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.CoverageCalculation
{
    [TestFixture]
    public class TestRunResultTests
    {
        [Test]
        public void GetCoverage_Should_ReturnOnePassedCoverage_When_ThereIsOneVariable_And_AssertionDidNotFail()
        {
            // arrange
            var variables = new[] { new AuditVariablePlaceholder("doc.cs", "", 1) };
            var testResult = new TestRunResult(variables, false, null);

            var testNode = CSharpSyntaxTree.ParseText("");

            // act
            LineCoverage[] totalCoverage = testResult.GetCoverage(testNode.GetRoot(), "SampleHelloWorldTests", "HelloWorldTests");

            // assert
            Assert.That(totalCoverage.Length, Is.EqualTo(1));
            Assert.That(totalCoverage[0].IsSuccess, Is.EqualTo(true));
        }

        [Test]
        public void GetCoverage_Should_MarkOnlyLastLine_As_FailedOne_When_ExceptionWasThrown_And_TheyAreInTestDocument()
        {
            // arrange
            string testNodePath = "SampleHelloWorldTests.HelloWorldTests.HelloWorldTests.TestMethod";
            var variables = new[] {
                new AuditVariablePlaceholder(@"c:\HelloWorldTests.cs", testNodePath, 1),
                new AuditVariablePlaceholder(@"c:\HelloWorldTests.cs", testNodePath, 2) };

            var testResult = new TestRunResult(variables, true, "Assertion failed");


            var testNode = CSharpSyntaxTree.ParseText("class HelloWorldTests{" +
                                                      " public void TestMethod()" +
                                                      "{}" +
                                                      "}");


            var testMethodNode = testNode.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            // act
            LineCoverage[] totalCoverage = testResult.GetCoverage(testMethodNode, "SampleHelloWorldTests", "HelloWorldTests");

            // assert
            Assert.That(totalCoverage.Length, Is.EqualTo(2));
            Assert.That(totalCoverage[0].IsSuccess, Is.EqualTo(true));
            Assert.IsNull(totalCoverage[0].ErrorMessage);

            Assert.That(totalCoverage[1].IsSuccess, Is.EqualTo(false));
            Assert.That(totalCoverage[1].ErrorMessage, Is.EqualTo(testResult.ErrorMessage)); 
        }

        [Test]
        public void GetCoverage_Should_MarkBothLines_As_Failed_Ones_When_ExceptionWasThrown_And_TheyAreInNotTestDocument()
        {
            // arrange
            string nodePath = "SampleHelloWorldTests.HelloWorldTests.HelloWorld.Method";

            var variables = new[] {
                new AuditVariablePlaceholder(@"c:\HelloWorldTests.cs", nodePath, 1),
                new AuditVariablePlaceholder(@"c:\HelloWorldTests.cs", nodePath, 2) };


            var testResult = new TestRunResult(variables, true, null);

            var testNode = CSharpSyntaxTree.ParseText("class HelloWorldTests{" +
                                                      " public void TestMethod()" +
                                                      "{}" +
                                                      "}");

            var testMethodNode = testNode.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            // act
            LineCoverage[] totalCoverage = testResult.GetCoverage(testMethodNode, "SampleHelloWorldTests", "HelloWorldTests");

            // assert
            Assert.That(totalCoverage.Length, Is.EqualTo(2));
            Assert.That(totalCoverage[0].IsSuccess, Is.EqualTo(false));
            Assert.That(totalCoverage[1].IsSuccess, Is.EqualTo(false));

            Assert.That(totalCoverage[0].ErrorMessage, Is.EqualTo(testResult.ErrorMessage));
            Assert.That(totalCoverage[1].ErrorMessage, Is.EqualTo(testResult.ErrorMessage));
        }
    }
}