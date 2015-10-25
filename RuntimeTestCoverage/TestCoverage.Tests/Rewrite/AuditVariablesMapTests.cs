using System;
using System.Linq;
using NUnit.Framework;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.Rewrite
{
    [TestFixture]
    public class AuditVariablesMapTests
    {
        private AuditVariablesMap _auditVariablesMap;

        [SetUp]
        public void Setup()
        {
            _auditVariablesMap = new AuditVariablesMap();
        }

        [Test]
        public void Should_NameVariableWithNodePathAndSpan()
        {
            var auditVariablePlaceholder = new AuditVariablePlaceholder("c:\\test\\HelloWorld.cs", "nodePath", 115);

            string variableName = _auditVariablesMap.AddVariable(auditVariablePlaceholder);

            const string expectedNodePath = "nodePath_115";

            Assert.That(variableName, Is.EqualTo(expectedNodePath));
            Assert.That(_auditVariablesMap.Map.Keys.First(), Is.EqualTo(expectedNodePath));
            Assert.That(_auditVariablesMap.Map.Values.First(), Is.EqualTo(auditVariablePlaceholder));
        }

        [Test]
        public void Should_OverrideVariableValue_When_VariableAlreadyExists()
        {
            var auditVariablePlaceholder = new AuditVariablePlaceholder("c:\\test\\HelloWorld.cs", "nodePath", 115);
            _auditVariablesMap.AddVariable(auditVariablePlaceholder);

            var newAuditVariablePlaceholder = new AuditVariablePlaceholder("c:\\test\\HelloWorld.cs", "nodePath", 115);
            _auditVariablesMap.AddVariable(newAuditVariablePlaceholder);

            Assert.That(_auditVariablesMap.Map.Count, Is.EqualTo(1));
            Assert.That(_auditVariablesMap.Map.Values.First(), Is.EqualTo(newAuditVariablePlaceholder));
        }

        [Test]
        public void Should_ReturnValidDictionaryName()
        {
            Assert.That(_auditVariablesMap.AuditVariablesListName, Is.EqualTo("Coverage"));
        }

        [Test]
        public void Should_ReturnValidClassName()
        {
            Assert.That(_auditVariablesMap.AuditVariablesClassName, Is.EqualTo("AuditVariables"));
        }

        [Test]
        public void Should_GenerateValidSourceCode()
        {
            const string expectedSourceCode = "public static class AuditVariables\r\n" +
                                              "{\r\n\tpublic static System.Collections.Generic.Dictionary<string,bool> Coverage = " +
                                              "new  System.Collections.Generic.Dictionary<string,bool>();\r\n" +
                                              "}";

            string classSourceCode = _auditVariablesMap.GenerateSourceCode();

            Assert.That(classSourceCode, Is.EqualTo(expectedSourceCode));
        }

        [Test]
        public void ExtractPathFromVariableName_When_VariableHasNoUnderscores()
        {
            string nodePath = AuditVariablesMap.ExtractPathFromVariableName("a.b.c.d.e.f_115");

            Assert.That(nodePath,Is.EqualTo("a.b.c.d.e.f"));
        }

        [Test]
        public void ExtractPathFromVariableName_When_VariableHasUnderscores()
        {
            string nodePath = AuditVariablesMap.ExtractPathFromVariableName("a_b.b.c_d.d.e.f_115");

            Assert.That(nodePath, Is.EqualTo("a_b.b.c_d.d.e.f"));
        }

        [Test]
        public void ExtractSpanFromVariableName_When_VariableHasNoUnderscores()
        {
            int span = AuditVariablesMap.ExtractSpanFromVariableName("a.b.c.d.e.f_115");

            Assert.That(span, Is.EqualTo(115));
        }

        [Test]
        public void ExtractSpanFromVariableName_When_VariableHasUnderscores()
        {
            int span = AuditVariablesMap.ExtractSpanFromVariableName("a_b.b.c_d.d.e.f_115");

            Assert.That(span, Is.EqualTo(115));
        }
    }
}