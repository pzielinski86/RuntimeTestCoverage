﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

            int expectedSpanPosition = sourceCode.IndexOf("int a",StringComparison.Ordinal);

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

        private static void AssertAuditVariablesCount(string sourceCode, int expectedVariablesCount)
        {
            var tree = CSharpSyntaxTree.ParseText(sourceCode);

            IAuditVariablesWalker walker = new AuditVariablesWalker();
            AuditVariablePlaceholder[] insertedNodes=walker.Walk(ProjectName, DocumentPath,tree.GetRoot());

            Assert.That(insertedNodes.Length, Is.EqualTo(expectedVariablesCount));
        }
    }
}