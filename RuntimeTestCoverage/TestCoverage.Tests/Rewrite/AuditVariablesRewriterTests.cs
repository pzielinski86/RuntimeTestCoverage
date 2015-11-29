using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSubstitute;
using NUnit.Framework;
using System.Linq;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.Rewrite
{
    [TestFixture]
    public class AuditVariablesRewriterTests
    {
        private IAuditVariablesWalker _auditVariablesWalkerMock;
        private AuditVariablesRewriter _rewriter;

        [SetUp]
        public void Setup()
        {
            _auditVariablesWalkerMock = Substitute.For<IAuditVariablesWalker>();
            _rewriter = new AuditVariablesRewriter(_auditVariablesWalkerMock);
        }

        [Test]
        public void Should_CallNodeWalker()
        {
            const string sourceCode = @"namespace SampleNamespace
                                {
                                    class SampleClass
                                    {
                                        public void SampleMethod()
                                        {
                                            if(true)
                                            {
                                               if(true)
                                               {
                                                    
                                               }
                                            }                    
                                        }
                                    }
                                }";
            const string documentPath = "documentPath";
            const string projectName = "projectName";

            var tree = CSharpSyntaxTree.ParseText(sourceCode);

            AuditVariablePlaceholder[] auditVariablePlaceholders = new AuditVariablePlaceholder[2];
            auditVariablePlaceholders[0] = new AuditVariablePlaceholder(null, null, 0);
            auditVariablePlaceholders[1] = new AuditVariablePlaceholder(null, null, 0);

            _auditVariablesWalkerMock.Walk(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SyntaxNode>())
                .Returns(auditVariablePlaceholders);

            SyntaxNode root = tree.GetRoot();
            _rewriter.Rewrite(projectName, documentPath, root);

            _auditVariablesWalkerMock.Received(1).Walk(projectName, documentPath, root);
        }


        [Test]
        public void Should_RewriteInlineIf_To_BlockStatementWithAuditVariable()
        {
            const string sourceCode = @"namespace SampleNamespace
                                {
                                    class SampleClass
                                    {
                                        public void SampleMethod()
                                        {
                                            if(true)
                                                int a=1;                 
                                        }
                                    }
                                }";

            var tree = CSharpSyntaxTree.ParseText(sourceCode);

            AuditVariablePlaceholder[] auditVariablePlaceholders = new AuditVariablePlaceholder[2];
            auditVariablePlaceholders[0] = new AuditVariablePlaceholder(null, null, 0);
            auditVariablePlaceholders[1] = new AuditVariablePlaceholder(null, "IfNode", 0);

            _auditVariablesWalkerMock.Walk(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SyntaxNode>())
                .Returns(auditVariablePlaceholders);

            // act
            var rewrittenNode=_rewriter.Rewrite("projectName", "documentPath", tree.GetRoot());
            
            // assert
            var ifStatement = rewrittenNode.DescendantNodes().OfType<IfStatementSyntax>().Single();
            var statements =
                ifStatement.ChildNodes()
                    .OfType<BlockSyntax>()
                    .Single()
                    .DescendantNodes()
                    .OfType<StatementSyntax>()
                    .ToArray();

            Assert.That(statements.Length, Is.EqualTo(2));
            Assert.That(statements[0].ToFullString(),Is.StringContaining("IfNode"));
        }

        [Test]
        public void Should_RewriteInlineWhile_To_BlockStatementWithAuditVariable()
        {
            const string sourceCode = @"namespace SampleNamespace
                                {
                                    class SampleClass
                                    {
                                        public void SampleMethod()
                                        {
                                            while(true)
                                                a++;
                                        }
                                    }
                                }";

            var tree = CSharpSyntaxTree.ParseText(sourceCode);

            AuditVariablePlaceholder[] auditVariablePlaceholders = new AuditVariablePlaceholder[2];
            auditVariablePlaceholders[0] = new AuditVariablePlaceholder(null, null, 0);
            auditVariablePlaceholders[1] = new AuditVariablePlaceholder(null, "WhileNode", 0);

            _auditVariablesWalkerMock.Walk(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SyntaxNode>())
                .Returns(auditVariablePlaceholders);

            // act
            var rewrittenNode = _rewriter.Rewrite("projectName", "documentPath", tree.GetRoot());

            // assert
            var whileStatement = rewrittenNode.DescendantNodes().OfType<WhileStatementSyntax>().Single();
            var statements =
                whileStatement.ChildNodes()
                    .OfType<BlockSyntax>()
                    .Single()
                    .DescendantNodes()
                    .OfType<StatementSyntax>()
                    .ToArray();

            Assert.That(statements.Length, Is.EqualTo(2));
            Assert.That(statements[0].ToFullString(), Is.StringContaining("WhileNode"));
        }

        [Test]
        public void Should_RewriteInlineElseStatement_To_BlockStatementWithAuditVariable()
        {
            const string sourceCode = @"namespace SampleNamespace
                                {
                                    class SampleClass
                                    {
                                        public void SampleMethod()
                                        {
                                            if(b==43)
                                                int a=1;                 
                                            else
                                               int b=45;
                                        }
                                    }
                                }";

            var tree = CSharpSyntaxTree.ParseText(sourceCode);

            AuditVariablePlaceholder[] auditVariablePlaceholders = new AuditVariablePlaceholder[3];
            auditVariablePlaceholders[0] = new AuditVariablePlaceholder("", null, 0);
            auditVariablePlaceholders[1] = new AuditVariablePlaceholder(null, null, 0);
            auditVariablePlaceholders[2] = new AuditVariablePlaceholder(null, "ElseNode", 0);

            _auditVariablesWalkerMock.Walk(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SyntaxNode>())
                .Returns(auditVariablePlaceholders);

            // act
            var rewrittenNode = _rewriter.Rewrite("projectName", "documentPath", tree.GetRoot());

            // assert
            var ifStatement = rewrittenNode.DescendantNodes().OfType<IfStatementSyntax>().Single();
            var statements =
                ifStatement.Else.ChildNodes()
                    .OfType<BlockSyntax>()
                    .Single()
                    .DescendantNodes()
                    .OfType<StatementSyntax>()
                    .ToArray();

            Assert.That(statements.Length, Is.EqualTo(2));
            Assert.That(statements[0].ToFullString(), Is.StringContaining("ElseNode"));
        }

      
        [Test]
        public void ShouldNot_RemoveOriginalNodeContainingLocalVariable()
        {
            const string sourceCode = @"namespace SampleNamespace
                                {
                                    class SampleClass
                                    {
                                        public void SampleMethod()
                                        {
                                            int a=4;
                                        }
                                    }
                                }";

            var tree = CSharpSyntaxTree.ParseText(sourceCode);

            AuditVariablePlaceholder[] auditVariablePlaceholders = new AuditVariablePlaceholder[1];
            auditVariablePlaceholders[0] = new AuditVariablePlaceholder(null, null, 0);
            _auditVariablesWalkerMock.Walk(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SyntaxNode>()).Returns(auditVariablePlaceholders);

            var rewrittenNode = _rewriter.Rewrite("projectName", "documentPath", tree.GetRoot());

            SyntaxNode originalNode = rewrittenNode.DescendantNodes().OfType<BlockSyntax>().First().ChildNodes().Last();

            Assert.That(originalNode.ToString().Trim(), Is.EqualTo("int a=4;"));
        }
    }
}
