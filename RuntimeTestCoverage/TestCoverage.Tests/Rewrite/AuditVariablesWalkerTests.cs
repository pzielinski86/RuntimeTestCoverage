using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.Rewrite
{
    [TestFixture]
    public class AuditVariablesWalkerTests
    {
        private const string ProjectName = "Name";
        private const string DocumentPath = "c:\\doc.cs";

        [Test]
        public void Should_InsertAuditVariableBeforeLocalVariable()
        {
            const string expectedNodePath = "Name.doc.SampleNamespace.SampleClass.SampleMethod";
            const string sourceCode=@"namespace SampleNamespace
                                {
                                    class SampleClass
                                    {
                                        public void SampleMethod()
                                        {
                                            int a=4;
                                        }
                                    }
                                }";

            int methodStart = sourceCode.IndexOf("public void");
            int expectedSpanPosition = sourceCode.IndexOf("int a", StringComparison.Ordinal) - methodStart;

            var tree = CSharpSyntaxTree.ParseText(sourceCode);

            IAuditVariablesWalker walker = new AuditVariablesWalker();
            AuditVariablePlaceholder[] insertedAuditVariables=walker.Walk(ProjectName, DocumentPath,tree.GetRoot());


            Assert.That(insertedAuditVariables.Length,Is.EqualTo(1));
            Assert.That(insertedAuditVariables[0].DocumentPath, Is.EqualTo(DocumentPath));
            Assert.That(insertedAuditVariables[0].NodePath, Is.EqualTo(expectedNodePath));
            Assert.That(insertedAuditVariables[0].SpanStart, Is.EqualTo(expectedSpanPosition));
        }
        
        [Test]
        public void Should_AddAuditVariableBeforeIf()
        {
            const string sourceCode = @"class SampleClass
                                    {
                                        public void SampleMethod()
                                        {           
                                            if(a==5){}
                                        }
                                    }";

            AssertAuditVariablesCount(sourceCode, 1);
        }

        [Test]
        public void ShouldNot_AddVariable_BeforeNestedElse()
        {
            const string sourceCode = @"class SampleClass
                                    {
                                        public void SampleMethod()
                                        {           
                                            if(a==5){}
                                            else if(true){}
                                            else if(false){}
                                        }
                                     }";

            AssertAuditVariablesCount(sourceCode, 1);
        }

        [Test]
        public void Should_AddAuditVariables_In_Inline_IfElseStatement()
        {
            const string sourceCode = @"class SampleClass
                                    {
                                        public void SampleMethod()
                                        {           
                                            if(a==5)
                                                int a=4;
                                            else
                                               int b=5;
                                        }
                                    }";

            AssertAuditVariablesCount(sourceCode, 3);
        }

        [Test]
        public void Should_AddAuditVariableBeforeNestedIfsAndLocalVariables()
        {
            const string sourceCode = @"class SampleClass
                                    {
                                        public void SampleMethod()
                                        {           
                                            if(a==5)
                                            {
                                                if(a==6){ int c=5;}
                                            }
                                        }
                                    }";

            AssertAuditVariablesCount(sourceCode, 3);
        }

        [Test]
        public void Should_AddAuditVariableBeforeFor()
        {
            const string sourceCode = @"class SampleClass
                                    {
                                        public void SampleMethod()
                                        {           
                                            for(int i=1;i<5;i++){}
                                        }
                                    }";

            AssertAuditVariablesCount(sourceCode, 1);
        }


        [Test]
        public void Should_AddAuditVariableBeforeWhile()
        {
            const string sourceCode = @"class SampleClass
                                    {
                                        public void SampleMethod()
                                        {           
                                            while(true) {}
                                        }
                                    }";

            AssertAuditVariablesCount(sourceCode, 1);
        }

        [Test]
        public void Should_AddAuditVariables_In_Inline_WhileStatement()
        {
            const string sourceCode = @"class SampleClass
                                    {
                                        public void SampleMethod()
                                        {           
                                            while(true) a++;
                                        }
                                    }";

            AssertAuditVariablesCount(sourceCode, 2);
        }

        [Test]
        public void Should_AddAuditVariables_In_Inline_ForStatement()
        {
            const string sourceCode = @"class SampleClass
                                    {
                                        public void SampleMethod()
                                        {           
                                            for(int i=0;i<10;i++) a++;
                                        }
                                    }";

            AssertAuditVariablesCount(sourceCode, 2);
        }

        [Test]
        public void Should_AddAuditVariables_In_Inline_ForeachStatement()
        {
            const string sourceCode = @"class SampleClass
                                    {
                                        public void SampleMethod()
                                        {           
                                            foreach(int a in data) a++;
                                        }
                                    }";
             
            AssertAuditVariablesCount(sourceCode, 2);
        }

        private static void AssertAuditVariablesCount(string sourceCode, int expectedVariablesCount)
        {
            var tree = CSharpSyntaxTree.ParseText(sourceCode);

            IAuditVariablesWalker walker = new AuditVariablesWalker();
            AuditVariablePlaceholder[] insertedNodes=walker.Walk(ProjectName, DocumentPath,tree.GetRoot());

            Assert.That(insertedNodes.Length, Is.EqualTo(expectedVariablesCount));
        }
    }
}
