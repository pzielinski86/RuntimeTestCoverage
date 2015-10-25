using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSubstitute;
using NSubstitute.Exceptions;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.Rewrite
{
    [TestFixture]
    public class AuditVariablesRewriterTests
    {
        private IAuditVariablesMap _auditVariablesMap;
        private IAuditVariablesWalker _auditVariablesWalkerMock;
        private AuditVariablesRewriter _rewriter;

        [SetUp]
        public void Setup()
        {
            _auditVariablesMap = Substitute.For<IAuditVariablesMap>();
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

            _auditVariablesMap.AddVariable(Arg.Any<AuditVariablePlaceholder>()).Returns("SampleVariableName");

            _auditVariablesMap.Map.Returns(new Dictionary<string, AuditVariablePlaceholder>()
            {
                {"SampleVariableName", new AuditVariablePlaceholder(documentPath,"nodePath",115)}
            });


            SyntaxNode root = tree.GetRoot();
            _rewriter.Rewrite(projectName, documentPath, root, _auditVariablesMap);

            _auditVariablesWalkerMock.Received(1).Walk(projectName, documentPath, root);
        }

        [Test]
        public void Should_AddVariableToMap()
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
            auditVariablePlaceholders[0] = new AuditVariablePlaceholder("c:\\1.cs", "path", 115);
            _auditVariablesMap.AddVariable(Arg.Any<AuditVariablePlaceholder>()).Returns("SampleVariableName");

            _auditVariablesWalkerMock.Walk(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SyntaxNode>())
                .Returns(auditVariablePlaceholders);

            _auditVariablesMap.Map.Returns(new Dictionary<string, AuditVariablePlaceholder>()
            {
                {"SampleVariableName", new AuditVariablePlaceholder("documentPath","nodePath",115)}
            });


            _rewriter.Rewrite("projectName", "documentPath", tree.GetRoot(), _auditVariablesMap);

            _auditVariablesMap.Received(1).AddVariable(auditVariablePlaceholders[0]);
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
            auditVariablePlaceholders[1] = new AuditVariablePlaceholder(null, null, 0);

            _auditVariablesWalkerMock.Walk(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SyntaxNode>())
                .Returns(auditVariablePlaceholders);

            _auditVariablesMap.AddVariable(Arg.Any<AuditVariablePlaceholder>()).Returns("SampleVariableName");
            _auditVariablesMap.Map.Returns(new Dictionary<string, AuditVariablePlaceholder>()
            {
                {"SampleVariableName", new AuditVariablePlaceholder("documentPath","nodePath",115)}
            });

            // act
            var rewrittenNode=_rewriter.Rewrite("projectName", "documentPath", tree.GetRoot(), _auditVariablesMap);
            
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
            Assert.That(statements[0].ToFullString(),Is.StringContaining("SampleVariableName"));
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
            auditVariablePlaceholders[0] = new AuditVariablePlaceholder(null, null, 0);
            auditVariablePlaceholders[1] = new AuditVariablePlaceholder(null, null, 0);
            auditVariablePlaceholders[2] = new AuditVariablePlaceholder(null, null, 0);

            _auditVariablesWalkerMock.Walk(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SyntaxNode>())
                .Returns(auditVariablePlaceholders);

            _auditVariablesMap.AddVariable(Arg.Any<AuditVariablePlaceholder>()).Returns("SampleVariableName");
            _auditVariablesMap.Map.Returns(new Dictionary<string, AuditVariablePlaceholder>()
            {
                {"SampleVariableName", new AuditVariablePlaceholder("documentPath","nodePath",115)}
            });

            // act
            var rewrittenNode = _rewriter.Rewrite("projectName", "documentPath", tree.GetRoot(), _auditVariablesMap);

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
            Assert.That(statements[0].ToFullString(), Is.StringContaining("SampleVariableName"));
        }

        [Test]
        public void Should_RewriteNestedIfs()
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

            var tree = CSharpSyntaxTree.ParseText(sourceCode);

            AuditVariablePlaceholder[] auditVariablePlaceholders = new AuditVariablePlaceholder[2];
            auditVariablePlaceholders[0] = new AuditVariablePlaceholder(null, null, 0);
            auditVariablePlaceholders[1] = new AuditVariablePlaceholder(null, null, 0);

            _auditVariablesWalkerMock.Walk(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SyntaxNode>())
                .Returns(auditVariablePlaceholders);

            _auditVariablesMap.AddVariable(Arg.Any<AuditVariablePlaceholder>()).Returns("SampleVariableName");
            _auditVariablesMap.Map.Returns(new Dictionary<string, AuditVariablePlaceholder>()
            {
                {"SampleVariableName", new AuditVariablePlaceholder("documentPath","nodePath",115)}
            });


            _rewriter.Rewrite("projectName", "documentPath", tree.GetRoot(), _auditVariablesMap);

            _auditVariablesMap.Received(2).AddVariable(Arg.Any<AuditVariablePlaceholder>());
        }

        [Test]
        public void Should_AddAuditNodeWithCommentContainingDocumentPath()
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

            _auditVariablesMap.AddVariable(Arg.Any<AuditVariablePlaceholder>()).Returns("SampleVariableName");
            _auditVariablesMap.AuditVariablesClassName.Returns("AuditVariablesClassName");
            _auditVariablesMap.AuditVariablesListName.Returns("AuditVariablesListName");
            _auditVariablesMap.Map.Returns(new Dictionary<string, AuditVariablePlaceholder>()
            {
                {"SampleVariableName", new AuditVariablePlaceholder("documentPath","nodePath",115)}
            });


            var rewrittenNode = _rewriter.Rewrite("projectName", "documentPath", tree.GetRoot(), _auditVariablesMap);

            SyntaxNode auditNode = rewrittenNode.DescendantNodes().OfType<BlockSyntax>().First().ChildNodes().First();

            const string expectedNode = "AuditVariablesClassName.AuditVariablesListName[\"SampleVariableName\"]=true;";
            const string expectedNodeComment = "//documentPath\n";

            Assert.That(auditNode.ToString(), Is.EqualTo(expectedNode));
            Assert.That(auditNode.GetTrailingTrivia().ToString(), Is.EqualTo(expectedNodeComment));
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

            _auditVariablesMap.AddVariable(Arg.Any<AuditVariablePlaceholder>()).Returns("SampleVariableName");
            _auditVariablesMap.Map.Returns(new Dictionary<string, AuditVariablePlaceholder>()
            {
                {"SampleVariableName", new AuditVariablePlaceholder("documentPath","nodePath",115)}
            });


            var rewrittenNode = _rewriter.Rewrite("projectName", "documentPath", tree.GetRoot(), _auditVariablesMap);

            SyntaxNode originalNode = rewrittenNode.DescendantNodes().OfType<BlockSyntax>().First().ChildNodes().Last();

            Assert.That(originalNode.ToString().Trim(), Is.EqualTo("int a=4;"));
        }
    }
}
