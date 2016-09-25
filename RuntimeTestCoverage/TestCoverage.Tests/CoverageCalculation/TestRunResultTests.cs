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
            var variables = new[] { new AuditVariablePlaceholder("HelloWorldTests.cs", "", 1) };
            var testResult = new TestRunResult("test_name",variables, null,false);

            var testNode = CSharpSyntaxTree.ParseText("");
            // act
            LineCoverage[] totalCoverage = testResult.GetCoverage(testNode.GetRoot(), "SampleHelloWorldTests", "HelloWorldTests.cs");

            // assert
            Assert.That(totalCoverage.Length, Is.EqualTo(1));
            Assert.That(totalCoverage[0].IsSuccess, Is.EqualTo(true));
        }

        [Test]
        public void GetCoverage_Should_MarkOnlyLastLine_As_FailedOne_When_ExceptionWasThrown_And_WeAreInTestDocument()
        {
            // arrange
            string testNodePath = "SampleHelloWorldTests.HelloWorldTests.HelloWorldTests.TestMethod";
            var variables = new[] {
                new AuditVariablePlaceholder(@"c:\HelloWorldTests.cs", testNodePath, 1),
                new AuditVariablePlaceholder(@"c:\HelloWorldTests.cs", testNodePath, 2) };

            var testResult = new TestRunResult("test_name",variables, "Assertion failed",false);


            var testNode = CSharpSyntaxTree.ParseText("class HelloWorldTests{" +
                                                      " public void TestMethod()" +
                                                      "{}" +
                                                      "}");


            var testMethodNode = testNode.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            // act
            LineCoverage[] totalCoverage = testResult.GetCoverage(testMethodNode, "SampleHelloWorldTests", @"c:\HelloWorldTests.cs");

            // assert
            Assert.That(totalCoverage.Length, Is.EqualTo(2));
            Assert.That(totalCoverage[0].IsSuccess, Is.EqualTo(true));
            Assert.IsNull(totalCoverage[0].ErrorMessage);

            Assert.That(totalCoverage[1].IsSuccess, Is.EqualTo(false));
            Assert.That(totalCoverage[1].ErrorMessage, Is.EqualTo(testResult.ErrorMessage)); 
        }
        
        [Test]
        public void GetCoverage_ShouldMarkAllSutAsFailed_When_AssertionFailed_ButNoOtherExceptionWasThrown()
        {
            // arrange 
            string nodePath = "SampleHelloWorldTests.HelloWorldTests.HelloWorld.Method";

            var variables = new[] {
                new AuditVariablePlaceholder(@"c:\HelloWorld.cs", nodePath, 1),
                new AuditVariablePlaceholder(@"c:\HelloWorld.cs", nodePath, 2),
                new AuditVariablePlaceholder(@"c:\HelloWorldTests.cs", nodePath, 3)};


            var testResult = new TestRunResult("test_name", variables, "assertion error", true);

            var testNode = CSharpSyntaxTree.ParseText("class HelloWorldTests{" +
                                                      " public void TestMethod()" +
                                                      "{}" +
                                                      "}");

            var testMethodNode = testNode.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            // act
            LineCoverage[] totalCoverage = testResult.GetCoverage(testMethodNode, "SampleHelloWorldTests", @"c:\HelloWorldTests.cs");

            // assert
            Assert.That(totalCoverage.Length, Is.EqualTo(3));
            Assert.That(totalCoverage[0].IsSuccess, Is.EqualTo(false));
            Assert.That(totalCoverage[1].IsSuccess, Is.EqualTo(false));
        }

        [Test]
        public void GetCoverage_ShouldMarkInTestFile_OnlyAssertionLineAsFailed_When_AssertionFailed_ButNoOtherExceptionWasThrown()
        {
            // arrange 
            string nodePath = "SampleHelloWorldTests.HelloWorldTests.HelloWorld.Method";

            var variables = new[] {
                new AuditVariablePlaceholder(@"c:\HelloWorld.cs", nodePath, 1),
                new AuditVariablePlaceholder(@"c:\HelloWorldTests.cs", nodePath, 2),
                new AuditVariablePlaceholder(@"c:\HelloWorldTests.cs", nodePath, 3)};


            var testResult = new TestRunResult("test_name", variables, "assertion error", true);

            var testNode = CSharpSyntaxTree.ParseText("class HelloWorldTests{" +
                                                      " public void TestMethod()" +
                                                      "{}" +
                                                      "}");

            var testMethodNode = testNode.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            // act
            LineCoverage[] totalCoverage = testResult.GetCoverage(testMethodNode, "SampleHelloWorldTests", @"c:\HelloWorldTests.cs");

            // assert
            Assert.That(totalCoverage.Length, Is.EqualTo(3));
            Assert.That(totalCoverage[1].IsSuccess, Is.EqualTo(true));
            Assert.That(totalCoverage[2].IsSuccess, Is.EqualTo(false));
        }

        [Test]
        public void GetCoverage_Should_MarkOnlyLastLine_As_FailedOne_When_ExceptionWasThrown_And_WeAreInSutDocument()
        {
            // arrange 
            string nodePath = "SampleHelloWorldTests.HelloWorldTests.HelloWorld.Method";

            var variables = new[] {
                new AuditVariablePlaceholder(@"c:\HelloWorld.cs", nodePath, 1),
                new AuditVariablePlaceholder(@"c:\HelloWorld.cs", nodePath, 2),
                new AuditVariablePlaceholder(@"c:\HelloWorldTests.cs", nodePath, 3)};


            var testResult = new TestRunResult("test_name",variables, "error",false);

            var testNode = CSharpSyntaxTree.ParseText("class HelloWorldTests{" +
                                                      " public void TestMethod()" +
                                                      "{}" +
                                                      "}");

            var testMethodNode = testNode.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            // act
            LineCoverage[] totalCoverage = testResult.GetCoverage(testMethodNode, "SampleHelloWorldTests", @"c:\HelloWorldTests.cs");

            // assert
            Assert.That(totalCoverage.Length, Is.EqualTo(3));
            Assert.That(totalCoverage[0].IsSuccess, Is.EqualTo(true));
            Assert.That(totalCoverage[1].IsSuccess, Is.EqualTo(false));

            Assert.That(totalCoverage[0].ErrorMessage, Is.Null);
            Assert.That(totalCoverage[1].ErrorMessage, Is.EqualTo(testResult.ErrorMessage));
        }
    }
}