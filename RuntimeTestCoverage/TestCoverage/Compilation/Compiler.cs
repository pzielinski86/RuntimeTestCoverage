﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TestCoverage.Compilation
{
    public class Compiler
    {
        public CompiledItem[] Compile(CompilationItem[] allItems, AuditVariablesMap auditVariablesMap)
        {
            var compiledItems = new List<CompiledItem>();
            CompiledItem compiledAudit = CompileAudit(auditVariablesMap);

            foreach (var compilationItem in allItems)
            {
                Compile(compilationItem, compiledAudit, allItems, compiledItems);
            }

            compiledItems.Add(compiledAudit);

            return compiledItems.ToArray();
        }

        public CompiledItem[] Compile(CompilationItem item, AuditVariablesMap auditVariablesMap)
        {
            var compiledItems = new List<CompiledItem>();
            CompiledItem compiledAudit = CompileAudit(auditVariablesMap);

            Compile(item, compiledAudit,new[]{item}, compiledItems);

            compiledItems.Add(compiledAudit);

            return compiledItems.ToArray();
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
