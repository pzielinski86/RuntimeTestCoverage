using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace TestCoverage.Tests
{
    [TestFixture]
    public class NodePathBuilderTests
    {
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
            string path =  NodePathBuilder.BuildPath(methodNode, documentName, projectName);

            Assert.That(path, Is.EqualTo(expectedPath));
        }
    }
}


