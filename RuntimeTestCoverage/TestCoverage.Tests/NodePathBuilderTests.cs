using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Linq;

namespace TestCoverage.Tests
{

    [TestFixture]
    public class NodePathBuilderTests
    {
        [TestCase("Math.Helpers.Test","Test")]
        public void ShouldReturn_MethodName(string path, string expectedMethodName)
        {
            string methodName = NodePathBuilder.GetMethodName(path);
            
            Assert.That(methodName,Is.EqualTo(expectedMethodName));
        }


        [Test]
        public void Should_ReturnValidPath()
        {
            const string documentName = "tests";
            const string projectName = "coverage_project";
            const string expectedPath = "coverage_project.tests.SampleNamespace.SampleClass.SampleMethod";

            SyntaxTree tree = CSharpSyntaxTree.ParseText(@"namespace SampleNamespace
{
    class SampleClass
    {
        public void SampleMethod()
        {
            
        }
    }
}");
            SyntaxNode methodNode = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
            string path = NodePathBuilder.BuildPath(methodNode, documentName, projectName);

            Assert.That(path, Is.EqualTo(expectedPath));
        }

        [Test]
        public void Should_ReturnValidPath_When_NamespaceIsNotAvailable()
        {
            const string documentName = "tests";
            const string projectName = "coverage_project";
            const string expectedPath = "coverage_project.tests.SampleClass.SampleMethod";

            SyntaxTree tree = CSharpSyntaxTree.ParseText(@"
    class SampleClass
    {
        public void SampleMethod()
        {
            
        }
    }");
            SyntaxNode methodNode = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
            string path = NodePathBuilder.BuildPath(methodNode, documentName, projectName);

            Assert.That(path, Is.EqualTo(expectedPath));
        }
    }
}


