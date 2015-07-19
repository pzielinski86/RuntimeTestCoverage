using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.Compilation
{
    [TestFixture]
    public class CompilerTests
    {
        [Test]
        public void Should_CompileSingleType()
        {
            var compiler=new Compiler();

            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo.dll", LanguageNames.CSharp);
            var syntaxTree = CSharpSyntaxTree.ParseText("class TestClass{}");
            var references = new[] {typeof (object).Assembly};
            var auditVariablesMap = new AuditVariablesMap();
            
            var compilationItem = new CompilationItem(project, new[] {syntaxTree});

            Assembly[] result=compiler.Compile(compilationItem, references, auditVariablesMap);

            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0].GetName().Name, Is.EqualTo(project.Name + "_testcoverage.dll"));
            Assert.That(result[0].GetTypes().First().Name, Is.EqualTo("TestClass"));
        }

        [Test]
        public void Should_CompileAuditLib()
        {
            var compiler = new Compiler();

            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo.dll", LanguageNames.CSharp);
            var syntaxTree = CSharpSyntaxTree.ParseText("class TestClass{}");
            var references = new[] { typeof(object).Assembly };
            var auditVariablesMap = new AuditVariablesMap();

            var compilationItem = new CompilationItem(project, new[] { syntaxTree });

            Assembly[] result = compiler.Compile(compilationItem, references, auditVariablesMap);

            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[1].GetName().Name, Is.EqualTo("Audit"));
            Assert.That(result[1].GetTypes().First().Name, Is.EqualTo(auditVariablesMap.AuditVariablesClassName));
        }

    }
}