using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using TestCoverage.Rewrite;

namespace TestCoverage.Compilation
{
    public class RoslynCompiler : ICompiler
    {
        public ICompiledItem[] Compile(IEnumerable<CompilationItem> allItems)
        {
            var allItemsArray = allItems.ToArray();

            var compiledItems = new List<RoslynCompiledItem>();
            RoslynCompiledItem roslynCompiledAudit = CompileAudit();

            foreach (var compilationItem in allItemsArray)
            {
                Compile(compilationItem, roslynCompiledAudit, allItemsArray, compiledItems);
            }

            compiledItems.Add(roslynCompiledAudit);

            foreach (var compiledItem in compiledItems)
                compiledItem.EmitAndSave();

            return compiledItems.ToArray();
        }

        public ICompiledItem[] Compile(CompilationItem item, IEnumerable<string> references)
        {
            var compiledItems = new List<RoslynCompiledItem>();
            RoslynCompiledItem roslynCompiledAudit = CompileAudit();

            var requiredReferences =
                item.Project.MetadataReferences.Union(
                    references.Select(r => MetadataReference.CreateFromFile(r))).ToList();
            requiredReferences.Add(roslynCompiledAudit.Compilation.ToMetadataReference());

            string newDllName = PathHelper.GetCoverageDllName(item.Project.Name);
            CSharpCompilation compiledDll = Compile(newDllName, item.SyntaxTrees, requiredReferences.ToArray());

            compiledItems.Add(new RoslynCompiledItem(item.Project, compiledDll));
            compiledItems.Add(roslynCompiledAudit);

            foreach (var compiledItem in compiledItems)
                compiledItem.EmitAndSave();

            return compiledItems.ToArray();
        }

        private RoslynCompiledItem CompileAudit()
        {
            var auditTree = CSharpSyntaxTree.ParseText(AuditVariablesMap.GenerateCode());

            // TODO - remove hardcoded .NET 3.5 dll
            var references = new[] { MetadataReference.CreateFromFile(@"C:\Windows\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll") };

            CSharpCompilation compilation = Compile("Audit", new[] { auditTree }, references);

            return new RoslynCompiledItem(null, compilation);
        }

        private void Compile(CompilationItem item,
            RoslynCompiledItem roslynCompiledAudit,
            IEnumerable<CompilationItem> allItems,
            List<RoslynCompiledItem> currentlyCompiledItems)
        {
            if (currentlyCompiledItems.Any(c => c.Project == item.Project))
                return;

            foreach (ProjectReference projectReference in item.Project.ProjectReferences)
            {
                CompilationItem referencedItem = allItems.Single(i => i.Project.Id == projectReference.ProjectId);
                Compile(referencedItem, roslynCompiledAudit, allItems, currentlyCompiledItems);
            }

            MetadataReference[] projectReferences = GetProjectReferences(item.Project, currentlyCompiledItems);
            MetadataReference[] auditReferences = { roslynCompiledAudit.Compilation.ToMetadataReference() };
            MetadataReference[] requiredReferences = projectReferences.Union(item.Project.MetadataReferences).Union(auditReferences).ToArray();

            string newDllName = PathHelper.GetCoverageDllName(item.Project.Name);
            CSharpCompilation compilation = Compile(newDllName, item.SyntaxTrees, requiredReferences);

            currentlyCompiledItems.Add(new RoslynCompiledItem(item.Project, compilation));
        }

        private MetadataReference[] GetProjectReferences(Project project, List<RoslynCompiledItem> compiledItems)
        {
            var metadataReferences = new List<MetadataReference>();

            foreach (ProjectReference projectReference in project.AllProjectReferences)
            {
                RoslynCompiledItem roslynCompiledItem = compiledItems.Single(i => i.Project.Id == projectReference.ProjectId);

                metadataReferences.Add(roslynCompiledItem.Compilation.ToMetadataReference());
            }

            return metadataReferences.ToArray();
        }

        private static CSharpCompilation Compile(string dllName, SyntaxTree[] allTrees, MetadataReference[] references)
        {
            var settings = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).
                         WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);

            CSharpCompilation compilation = CSharpCompilation.Create(
                dllName,
                allTrees,
                references,
                settings);

            return compilation;
        }
    }
}
