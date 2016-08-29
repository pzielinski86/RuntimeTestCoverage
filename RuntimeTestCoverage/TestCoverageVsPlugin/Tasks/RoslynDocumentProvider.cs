using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using TestCoverage.Tasks;

namespace TestCoverageVsPlugin.Tasks
{
    public class RoslynDocumentProvider : IDocumentProvider
    {
        public SyntaxNode GetSyntaxNodeFromTextSnapshot(ITextSnapshot textSnapshot)
        {
            return textSnapshot.GetOpenDocumentInCurrentContextWithChanges().GetSyntaxRootAsync().Result;
        } 
    }
}