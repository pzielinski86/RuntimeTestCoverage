﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NSubstitute;
using NUnit.Framework;
using System.Linq;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.Rewrite
{
    [TestFixture]
    public class SolutionRewriterTests
    {
        private SolutionRewriter _solutionRewriter;
        private IAuditVariablesRewriter _auditVariablesRewriter;

        [SetUp]
        public void Setup()
        {
            _auditVariablesRewriter = Substitute.For<IAuditVariablesRewriter>();

            _solutionRewriter = new SolutionRewriter(_auditVariablesRewriter);
        }

        [Test]
        public void Should_ReturnValidDocumentPathAndRewrittenSyntaxTree()
        {
            const string sourceCode = "class SampleClass{" +
                                         "public void Test(int a){}" +
                                      "}";

            const string documentPath = "documentPath";

            SyntaxNode rewrittenNode = CSharpSyntaxTree.ParseText(sourceCode).GetRoot();
            _auditVariablesRewriter.Rewrite(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SyntaxNode>()).Returns(rewrittenNode);

            RewrittenDocument rewrittenDocument = _solutionRewriter.RewriteDocument("projectName", documentPath, sourceCode);

            Assert.That(rewrittenDocument.DocumentPath, Is.EqualTo(documentPath));
            Assert.That(rewrittenDocument.SyntaxTree, Is.EqualTo(rewrittenNode.SyntaxTree));
        }

        [Test]
        public void Should_RewriteOneDocument()
        {
            // arrange
            const string sourceCode = "class SampleClass{}";
            SyntaxNode node = CSharpSyntaxTree.ParseText(sourceCode).GetRoot();

            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo.dll", LanguageNames.CSharp);
            string documentPath = "c:\\helloworld.cs";
            DocumentInfo documentInfo = DocumentInfo.Create(DocumentId.CreateNewId(project.Id), "HelloWorld.cs",
                filePath: documentPath);
            Document document = workspace.AddDocument(documentInfo);


            _auditVariablesRewriter.Rewrite(Arg.Any<string>(), documentPath, Arg.Any<SyntaxNode>()).Returns(node);

            // act
            RewriteResult result = _solutionRewriter.RewriteAllClasses(workspace.CurrentSolution.Projects);

            // assert
            Assert.That(result.Items.Count, Is.EqualTo(1));
            Assert.That(result.Items.Keys.First().Id, Is.EqualTo(project.Id));

            Assert.That(result.Items.Values.First().Count, Is.EqualTo(1));
            Assert.That(result.Items.Values.First().First().DocumentPath, Is.EqualTo(document.FilePath));
            Assert.That(result.Items.Values.First().First().SyntaxTree, Is.EqualTo(node.SyntaxTree));
        }

        [Test]
        public void Should_RewriteTwoDocumentsInTheSameProject()
        {
            const string sourceCode = "class SampleClass{}";
            SyntaxNode node = CSharpSyntaxTree.ParseText(sourceCode).GetRoot();

            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo.dll", LanguageNames.CSharp);

            DocumentInfo documentInfo1 = DocumentInfo.Create(DocumentId.CreateNewId(project.Id), "HelloWorld.cs",
                filePath: "c:\\helloworld.cs");
            DocumentInfo documentInfo2 = DocumentInfo.Create(DocumentId.CreateNewId(project.Id), "HelloWorld2.cs",
                filePath: "c:\\helloworld2.cs");

            workspace.AddDocument(documentInfo1);
            workspace.AddDocument(documentInfo2);

            _auditVariablesRewriter.Rewrite(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SyntaxNode>()).Returns(node);

            RewriteResult result = _solutionRewriter.RewriteAllClasses(workspace.CurrentSolution.Projects);

            Assert.That(result.Items.Count, Is.EqualTo(1));
            Assert.That(result.Items.Values.First().Count, Is.EqualTo(2));
            Assert.That(result.Items.Values.First().First().DocumentPath, Is.EqualTo(documentInfo1.FilePath));
            Assert.That(result.Items.Values.First().Last().DocumentPath, Is.EqualTo(documentInfo2.FilePath));
        }

        [Test]
        public void Should_RewriteTwoDocumentsFromDifferentProjects()
        {
            const string sourceCode = "class SampleClass{}";
            SyntaxNode node = CSharpSyntaxTree.ParseText(sourceCode).GetRoot();

            var workspace = new AdhocWorkspace();
            var project1 = workspace.AddProject("foo.dll", LanguageNames.CSharp);
            var project2 = workspace.AddProject("foo2.dll", LanguageNames.CSharp);

            DocumentInfo documentInfo1 = DocumentInfo.Create(DocumentId.CreateNewId(project1.Id), "HelloWorld.cs",
                filePath: "c:\\helloworld.cs");
            DocumentInfo documentInfo2 = DocumentInfo.Create(DocumentId.CreateNewId(project2.Id), "HelloWorld2.cs",
                filePath: "c:\\helloworld2.cs");

            workspace.AddDocument(documentInfo1);
            workspace.AddDocument(documentInfo2);

            _auditVariablesRewriter.Rewrite(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SyntaxNode>()).Returns(node);

            RewriteResult result = _solutionRewriter.RewriteAllClasses(workspace.CurrentSolution.Projects);

            Assert.That(result.Items.Count, Is.EqualTo(2));
            Assert.That(result.Items.Keys.First().Id, Is.EqualTo(project1.Id));
            Assert.That(result.Items.Keys.Last().Id, Is.EqualTo(project2.Id));
        }

        private Solution SetupSolutionWithOneProject(params string[] documentNames)
        {
            var workspace = new AdhocWorkspace();
            var project1 = workspace.AddProject("foo.dll", LanguageNames.CSharp);

            foreach (var documentPath in documentNames)
            {
                DocumentInfo documentInfo1 = DocumentInfo.Create(DocumentId.CreateNewId(project1.Id), documentPath,
                    filePath: documentPath + ".cs");
                workspace.AddDocument(documentInfo1);
            }
            return workspace.CurrentSolution;
        }
    }
}