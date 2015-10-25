using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TestCoverage.Compilation
{
    public class CompiledItem
    {

        public Project Project { get; private set; }
        public CSharpCompilation Compilation { get; private set; }
        public bool IsEmitted { get; set; }
        public string DllPath { get; private set; }
        public Assembly Assembly { get; private set; }

        public CompiledItem(Project project, CSharpCompilation
            compilation)
        {
            Project = project;
            Compilation = compilation;
        }

        public Assembly EmitAndSave()
        {
            if (IsEmitted)
                return Assembly;

            string dllName = Compilation.AssemblyName;
            var dllPath = Path.Combine(Directory.GetCurrentDirectory(), dllName);
           
            using (var stream = File.Open(dllPath,FileMode.OpenOrCreate))
            {
                EmitResult emitResult = Compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    throw new TestCoverageCompilationException(
                        emitResult.Diagnostics.Select(d => d.GetMessage()).ToArray());
                }
            }

            Assembly = Assembly.LoadFrom(dllPath);
            DllPath = dllPath;
            IsEmitted = true;

            return Assembly;
        }

        public ISemanticModel GetSemanticModel(SyntaxTree syntaxTree)
        {
            return new RoslynSemanticModel(Compilation.GetSemanticModel(syntaxTree,false));
        }
    }
}