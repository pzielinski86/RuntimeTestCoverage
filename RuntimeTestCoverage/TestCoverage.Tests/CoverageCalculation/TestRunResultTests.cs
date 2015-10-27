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
            string[] variables = new[] {"var1"};
            var testResult = new TestRunResult(variables, true, null);
            var auditLog=new AuditVariablesMap();
            auditLog.Map[variables[0]]=new AuditVariablePlaceholder(@"c:\HelloWorld.cs","Node_path",2342);

            var testNode = CSharpSyntaxTree.ParseText("class HelloWorldTests{" +
                                                " public void Method()" +
                                                "{}" +
                                                "}");

            var testMethodNode = testNode.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();

            // act
            LineCoverage[] totalCoverage=testResult.GetCoverage(auditLog, testMethodNode, "SampleHelloWorldTests", "HelloWorldTests");

            // assert
            Assert.That(totalCoverage.Length,Is.EqualTo(1));
            Assert.That(totalCoverage[0].IsSuccess,Is.EqualTo(1));
        }

    }
}