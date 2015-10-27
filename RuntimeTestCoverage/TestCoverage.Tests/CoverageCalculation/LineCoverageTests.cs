using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using TestCoverage.CoverageCalculation;
using TestCoverage.Extensions;
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
            const string varName = "HelloWorldSample.HelloWorld.HelloWorld.Method_243";
            var auditVariablesMap = new AuditVariablesMap();
            auditVariablesMap.Map[varName] = new AuditVariablePlaceholder(@"c:\HelloWorld.cs", "node_path", 243);
            var testNode = CSharpSyntaxTree.ParseText("class HelloWorldTests{" +
                                                      " public void Method()" +
                                                      "{}" +
                                                      "}");

            var testMethodNode = testNode.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();

            // act
            var coverage = LineCoverage.EvaluateAuditVariable(auditVariablesMap, varName, testMethodNode, "HelloWorldTestsSample", "HelloWorldTests");

            // act
            Assert.That(coverage.TestPath,Is.EqualTo("HelloWorldTestsSample.HelloWorldTests.HelloWorldTests.Method"));
        }

        [Test]
        public void EvaluateAuditVariable_Should_Extract_Path()
        {
            // arrange
            const string varName = "HelloWorldSample.HelloWorld.HelloWorld.Method_243";
            var auditVariablesMap = new AuditVariablesMap();
            auditVariablesMap.Map[varName] = new AuditVariablePlaceholder(@"c:\HelloWorld.cs", "node_path", 243);
            var testNode = CSharpSyntaxTree.ParseText("");

            // act
            var coverage = LineCoverage.EvaluateAuditVariable(auditVariablesMap, varName, testNode.GetRoot(), "HelloWorldTestsSample", "HelloWorldTests");

            // act
            Assert.That(coverage.Path, Is.EqualTo("node_path"));
        }

        [Test]
        public void EvaluateAuditVariable_Should_Extract_Span()
        {
            // arrange
            const string varName = "HelloWorldSample.HelloWorld.HelloWorld.Method_243";
            var auditVariablesMap = new AuditVariablesMap();
            auditVariablesMap.Map[varName] = new AuditVariablePlaceholder(@"c:\HelloWorld.cs", "node_path", 243);
            var testNode = CSharpSyntaxTree.ParseText("");

            // act
            var coverage = LineCoverage.EvaluateAuditVariable(auditVariablesMap, varName, testNode.GetRoot(), "HelloWorldTestsSample", "HelloWorldTests");

            // act
            Assert.That(coverage.Span, Is.EqualTo(243));
        }
    }
}