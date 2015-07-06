using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TestCoverage.Rewrite;

namespace TestCoverage.Compilation
{
    internal class Compiler
    {
        public Assembly[] Compile(CompilationItem[] allItems, AuditVariablesMap auditVariablesMap)
        {
            var compiledItems = new List<CompiledItem>();
            CompiledItem compiledAudit = CompileAudit(auditVariablesMap);

            foreach (var compilationItem in allItems)
            {
                Compile(compilationItem, compiledAudit, allItems, compiledItems);
            }

            compiledItems.Add(compiledAudit);

            return compiledItems.Select(x=>x.EmitAndSave()).ToArray();
        }

        public Assembly[] Compile(CompilationItem item, Assembly[] references, AuditVariablesMap auditVariablesMap)
        {
            var compiledItems = new List<CompiledItem>();
            CompiledItem compiledAudit = CompileAudit(auditVariablesMap);

            var requiredReferences =
                item.Project.MetadataReferences.Union(
                    references.Select(r => MetadataReference.CreateFromFile(r.Location))).ToList();
            requiredReferences.Add(compiledAudit.Compilation.ToMetadataReference());

            CSharpCompilation compiledDll = Compile(item.Project.Name, item.SyntaxTrees, requiredReferences.ToArray());

            compiledItems.Add(new CompiledItem(item.Project,compiledDll));
            compiledItems.Add(compiledAudit);

            return compiledItems.Select(x => x.EmitAndSave()).ToArray();
        }

        public CompiledItem CompileAudit(AuditVariablesMap auditVariablesMap)
        {
            var auditTree = CSharpSyntaxTree.ParseText(auditVariablesMap.ToString());

            var references = new[] { MetadataReference.CreateFromFile(Assembly.Load("mscorlib").Location) };

            CSharpCompilation compilation = Compile("Audit", new[] { auditTree }, references);

            return new CompiledItem(null, compilation);
        }

        private void Compile(CompilationItem item, CompiledItem compiledAudit, CompilationItem[] allItems, List<CompiledItem> currentlyCompiledItems)
        {
            if (currentlyCompiledItems.Any(c => c.Project == item.Project))
                return;

            foreach (ProjectReference projectReference in item.Project.ProjectReferences)
            {
                CompilationItem referencedItem = allItems.Single(i => i.Project.Id == projectReference.ProjectId);
                Compile(referencedItem, compiledAudit, allItems, currentlyCompiledItems);
            }

            MetadataReference[] projectReferences = GetProjectReferences(item.Project, currentlyCompiledItems);
            MetadataReference[] auditReferences = { compiledAudit.Compilation.ToMetadataReference() };
            MetadataReference[] requiredReferences = projectReferences.Union(item.Project.MetadataReferences).Union(auditReferences).ToArray();
            CSharpCompilation compilation = Compile(item.Project.Name, item.SyntaxTrees, requiredReferences);

            currentlyCompiledItems.Add(new CompiledItem(item.Project, compilation));
        }

        private MetadataReference[] GetProjectReferences(Project project, List<CompiledItem> compiledItems)
        {
            var metadataReferences = new List<MetadataReference>();

            foreach (ProjectReference projectReference in project.AllProjectReferences)
            {
                CompiledItem compiledItem = compiledItems.Single(i => i.Project.Id == projectReference.ProjectId);

                metadataReferences.Add(compiledItem.Compilation.ToMetadataReference());
            }

            return metadataReferences.ToArray();
        }

        private static CSharpCompilation Compile(string dllName, SyntaxTree[] allTrees, MetadataReference[] references)
        {
            CSharpCompilation compilation = CSharpCompilation.Create(
                dllName,
                allTrees,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


            return compilation;
        }
    }
}
