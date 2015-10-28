using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
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
            string[] variables = { "var1" };
            var testResult = new TestRunResult(variables, false, null);
            var auditLog = new AuditVariablesMap();
            auditLog.Map[variables[0]] = new AuditVariablePlaceholder(@"c:\HelloWorld.cs", "Node_path", 2342);

            var testNode = CSharpSyntaxTree.ParseText("");

            // act
            LineCoverage[] totalCoverage = testResult.GetCoverage(auditLog, testNode.GetRoot(), "SampleHelloWorldTests", "HelloWorldTests");

            // assert
            Assert.That(totalCoverage.Length, Is.EqualTo(1));
            Assert.That(totalCoverage[0].IsSuccess, Is.EqualTo(true));
        }

        [Test]
        public void GetCoverage_Should_MarkOnlyLastLine_As_FailedOne_When_BothLinesFailed_And_TheyAreInTestDocument()
        {
            // arrange
            string[] variables = { "SampleHelloWorldTests.HelloWorldTests.HelloWorldTests.TestMethod_1",
                "SampleHelloWorldTests.HelloWorldTests.HelloWorldTests.TestMethod_2" };

            var testResult = new TestRunResult(variables, true, null);
            var auditLog = new AuditVariablesMap();
            string testNodePath = "SampleHelloWorldTests.HelloWorldTests.HelloWorldTests.TestMethod";

            auditLog.Map[variables[0]] = new AuditVariablePlaceholder(@"c:\HelloWorldTests.cs", testNodePath, 1);
            auditLog.Map[variables[1]] = new AuditVariablePlaceholder(@"c:\HelloWorldTests.cs", testNodePath, 2);

            var testNode = CSharpSyntaxTree.ParseText("class HelloWorldTests{" +
                                                      " public void TestMethod()" +
                                                      "{}" +
                                                      "}");


            var testMethodNode = testNode.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            // act
            LineCoverage[] totalCoverage = testResult.GetCoverage(auditLog, testMethodNode, "SampleHelloWorldTests", "HelloWorldTests");

            // assert
            Assert.That(totalCoverage.Length, Is.EqualTo(2));
            Assert.That(totalCoverage[0].IsSuccess, Is.EqualTo(true));
            Assert.That(totalCoverage[1].IsSuccess, Is.EqualTo(false));
        }

        [Test]
        public void GetCoverage_Should_BothLines_As_Failed_Ones_When_BothLinesFailed_And_TheyAreInNotTestDocument()
        {
            // arrange
            string[] variables = { "SampleHelloWorldTests.HelloWorldTests.HelloWorld.Method_1",
                "SampleHelloWorldTests.HelloWorldTests.HelloWorld.Method_2" };

            var testResult = new TestRunResult(variables, true, null);
            var auditLog = new AuditVariablesMap();
            string nodePath = "SampleHelloWorldTests.HelloWorldTests.HelloWorld.Method";

            auditLog.Map[variables[0]] = new AuditVariablePlaceholder(@"c:\HelloWorld.cs", nodePath, 1);
            auditLog.Map[variables[1]] = new AuditVariablePlaceholder(@"c:\HelloWorld.cs", nodePath, 2);

            var testNode = CSharpSyntaxTree.ParseText("class HelloWorldTests{" +
                                                      " public void TestMethod()" +
                                                      "{}" +
                                                      "}");

            var testMethodNode = testNode.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            // act
            LineCoverage[] totalCoverage = testResult.GetCoverage(auditLog, testMethodNode, "SampleHelloWorldTests", "HelloWorldTests");

            // assert
            Assert.That(totalCoverage.Length, Is.EqualTo(2));
            Assert.That(totalCoverage[0].IsSuccess, Is.EqualTo(false));
            Assert.That(totalCoverage[1].IsSuccess, Is.EqualTo(false));
        }
    }
}