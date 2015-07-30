using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.IntegrationTests
{
    [TestFixture]
    public class CompilerTests
    {
        [Test, RunInApplicationDomain]
        public void Should_CompileSingleType()
        {
            var compiler = new RoslynCompiler();

            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo.dll", LanguageNames.CSharp);
            var syntaxTree = CSharpSyntaxTree.ParseText("class TestClass{}");
            var references = new[] { typeof(object).Assembly };
            var auditVariablesMap = new AuditVariablesMap();

            var compilationItem = new CompilationItem(project, new[] { syntaxTree });

            Assembly[] result = compiler.Compile(compilationItem, references, auditVariablesMap);

            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0].GetName().Name, Is.StringStarting(project.Name + "_"));
            Assert.That(result[0].GetTypes().First().Name, Is.EqualTo("TestClass"));
        }

        [Test, RunInApplicationDomain]
        public void Should_CompileSingleTypeWithExternalReferences()
        {
            var compiler = new RoslynCompiler();

            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo.dll", LanguageNames.CSharp);

            project = project.AddMetadataReference(MetadataReference.CreateFromFile(typeof(Assert).Assembly.Location));

            var syntaxTree = CSharpSyntaxTree.ParseText("class TestClass{" +
                                                          "private void Test(){" +
                                                                    "NUnit.Framework.Assert.IsTrue(true);}" +
                                                        "}");

            var references = new[] { typeof(object).Assembly };
            var auditVariablesMap = new AuditVariablesMap();

            var compilationItem = new CompilationItem(project, new[] { syntaxTree });

            Assembly[] result = compiler.Compile(compilationItem, references, auditVariablesMap);

            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0].GetName().Name, Is.StringStarting(project.Name + "_"));
            Assert.That(result[0].GetTypes().First().Name, Is.EqualTo("TestClass"));
        }

        [Test, RunInApplicationDomain]
        public void Should_CompileProjectWithDependencyOnAnotherProject()
        {
            var compiler = new RoslynCompiler();

            var workspace = new AdhocWorkspace();
            var project2 = workspace.AddProject("foo2.dll", LanguageNames.CSharp);
            var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);


            project1 = project1.AddProjectReference(new ProjectReference(project2.Id));

            project1 = project1.AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            project2 = project2.AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var project1SyntaxTree = CSharpSyntaxTree.ParseText("class TestClass{" +
                                                          "private void Test(){" +
                                                                    "var test = new Project2.SampleClass();}" +
                                                        "}");

            var project2SyntaxTree = CSharpSyntaxTree.ParseText("namespace Project2 {public class SampleClass{}}");


            var auditVariablesMap = new AuditVariablesMap();

            var compilationItem1 = new CompilationItem(project1, new[] { project1SyntaxTree });
            var compilationItem2 = new CompilationItem(project2, new[] { project2SyntaxTree });

            Assembly[] result = compiler.Compile(new[] { compilationItem1, compilationItem2 }, auditVariablesMap);

            Assert.That(result.Length, Is.EqualTo(3));
            Assert.That(result[1].GetTypes().First().Name, Is.EqualTo("TestClass"));
            Assert.That(result[0].GetTypes().First().Name, Is.EqualTo("SampleClass"));
        }

        [Test, RunInApplicationDomain]
        public void Should_CompileProjectsFromChildrenToRoot_When_Project1DependsOnProject2_And_Project2DependsOnProject3()
        {
            var compiler = new RoslynCompiler();

            var workspace = new AdhocWorkspace();

            var project3 = workspace.AddProject("foo3.dll", LanguageNames.CSharp);
            var project2 = workspace.AddProject("foo2.dll", LanguageNames.CSharp);
            var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);


            project2 = project2.AddProjectReference(new ProjectReference(project3.Id));
            project1 = project1.AddProjectReference(new ProjectReference(project2.Id));

            project1 = project1.AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            project2 = project2.AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            project3 = project3.AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var project1SyntaxTree = CSharpSyntaxTree.ParseText("class TestClass{" +
                                                          "private void Test(){" +
                                                                    "var test = new Project2.SampleClass();}" +
                                                        "}");

            var project2SyntaxTree = CSharpSyntaxTree.ParseText("namespace Project2 {public class SampleClass{" +
                                                            "private void test(){ var a= new Project3.SampleClass2();" +
                                                                    "}}}");
            var project3SyntaxTree = CSharpSyntaxTree.ParseText("namespace Project3 {public class SampleClass2{}}");


            var auditVariablesMap = new AuditVariablesMap();

            var compilationItem1 = new CompilationItem(project1, new[] { project1SyntaxTree });
            var compilationItem2 = new CompilationItem(project2, new[] { project2SyntaxTree });
            var compilationItem3 = new CompilationItem(project3, new[] { project3SyntaxTree });

            Assembly[] result = compiler.Compile(new[] { compilationItem1, compilationItem2, compilationItem3 }, auditVariablesMap);

            Assert.That(result.Length, Is.EqualTo(4));            
        }

        [Test, RunInApplicationDomain]
        public void Should_CompileAuditLib()
        {
            var compiler = new RoslynCompiler();

            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo.dll", LanguageNames.CSharp);
            var auditVariablesMap = new AuditVariablesMap();

            var compilationItem = new CompilationItem(project, new SyntaxTree[0]);

            Assembly[] result = compiler.Compile(compilationItem, new Assembly[0], auditVariablesMap);

            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[1].GetName().Name, Is.StringStarting("Audit"));
            Assert.That(result[1].GetTypes().First().Name, Is.EqualTo(auditVariablesMap.AuditVariablesClassName));
        }

    }
}