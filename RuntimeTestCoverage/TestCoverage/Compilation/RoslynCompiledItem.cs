using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TestCoverage.Compilation
{
    public class RoslynCompiledItem : ICompiledItem
    {

        public Project Project { get; private set; }
        public CSharpCompilation Compilation { get; private set; }
        public bool IsEmitted { get; set; }
        public string DllPath { get; private set; }

        public RoslynCompiledItem(Project project, CSharpCompilation
            compilation)
        {
            Project = project;
            Compilation = compilation;
        }

        public string EmitAndSave()
        {
            if (IsEmitted)
                return DllPath;

            string dllName = Compilation.AssemblyName;
            var dllPath = Path.Combine(Config.WorkingDirectory, dllName);
           
            using (var stream = File.Open(dllPath,FileMode.OpenOrCreate))
            {
                EmitResult emitResult = Compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    throw new TestCoverageCompilationException(
                        emitResult.Diagnostics.Select(d => d.GetMessage()).ToArray());
                }

            }

            DllPath = dllPath;
            IsEmitted = true;

            return DllPath;
        }

        public ISemanticModel GetSemanticModel(SyntaxTree syntaxTree)
        {
            return new RoslynSemanticModel(Compilation.GetSemanticModel(syntaxTree,false));
        }
    }
}