using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Linq;
using TestCoverage.CoverageCalculation;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.CoverageCalculation
{
    [TestFixture]
    public class LineCoverageTests
    {
        [Test]
        public void EvaluateAuditVariable_Should_Extract_TestPath()
        {
            // arrange
            var variable = new AuditVariablePlaceholder(null,"HelloWorldSample.HelloWorld.HelloWorld.Method_243",1);
            var testNode = CSharpSyntaxTree.ParseText("class HelloWorldTests{" +
                                                      " public void Method()" +
                                                      "{}" +
                                                      "}");

            var testMethodNode = testNode.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();

            // act
            var coverage = LineCoverage.EvaluateAuditVariable(variable, testMethodNode, "HelloWorldTestsSample", "HelloWorldTests");

            // act
            Assert.That(coverage.TestPath,Is.EqualTo("HelloWorldTestsSample.HelloWorldTests.HelloWorldTests.Method"));
        }

        [Test]
        public void EvaluateAuditVariable_Should_Extract_Path()
        {
            // arrange
            var variable = new AuditVariablePlaceholder(@"c:\HelloWorld.cs", "node_path", 243);
          
            var testNode = CSharpSyntaxTree.ParseText("");

            // act
            var coverage = LineCoverage.EvaluateAuditVariable(variable,testNode.GetRoot(), "HelloWorldTestsSample", "HelloWorldTests");

            // act
            Assert.That(coverage.NodePath, Is.EqualTo("node_path"));
        }

        [Test]
        public void EvaluateAuditVariable_Should_Extract_Span()
        {
            // arrange
            var variable = new AuditVariablePlaceholder(@"c:\HelloWorld.cs", "node_path", 243);
            var testNode = CSharpSyntaxTree.ParseText("");

            // act
            var coverage = LineCoverage.EvaluateAuditVariable(variable, testNode.GetRoot(), "HelloWorldTestsSample", "HelloWorldTests");

            // act
            Assert.That(coverage.Span, Is.EqualTo(243));
        }
    }
}