﻿using System;
using System.Linq;
using NUnit.Framework;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.Rewrite
{
    [TestFixture]
    public class AuditVariablesMapTests
    {

        [Test]
        public void Should_ReturnValidDictionaryName()
        {
            Assert.That(AuditVariablesMap.AuditVariablesListName, Is.EqualTo("Coverage"));
        }

        [Test]
        public void Should_ReturnValidClassName()
        {
            Assert.That(AuditVariablesMap.AuditVariablesListClassName, Is.EqualTo("AuditVariables"));
        }

        [Test]
        public void Should_GenerateValidSourceCode_Of_AuditVariable()
        {
            const string expectedSourceCode = "public struct AuditVariable\r\n" +
                                              "{\r\npublic System.String NodePath, DocumentPath;" +
                                              "\r\npublic int Span;\r\n" +
                                              "}\r\n";

            string auditVariableSourceCode = AuditVariablesMap.GenerateAuditVariableSourceCode();

            Assert.That(auditVariableSourceCode, Is.EqualTo(expectedSourceCode));
        }

        [Test]
        public void Should_GenerateValidVariablesListSourceCode()
        {
            const string expectedSourceCode = "public static class AuditVariables\r\n" +
                                              "{\r\n\tpublic static System.Collections.Generic.List<AuditVariable> Coverage = " +
                                              "new  System.Collections.Generic.List<AuditVariable>();\r\n" +
                                              "}";

            string classSourceCode = AuditVariablesMap.GenerateVariablesListSourceCode();

            Assert.That(classSourceCode, Is.EqualTo(expectedSourceCode));
        }
    }
}