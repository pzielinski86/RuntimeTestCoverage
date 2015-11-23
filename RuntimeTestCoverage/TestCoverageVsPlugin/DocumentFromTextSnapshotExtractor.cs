using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;

namespace TestCoverageVsPlugin
{
    sealed class DocumentFromTextSnapshotExtractor : IDocumentFromTextSnapshotExtractor
    {
        public SyntaxNode ExtactDocument(ITextSnapshot snapshot)
        {
            Document document = snapshot.GetOpenDocumentInCurrentContextWithChanges();

            SyntaxNode root;
            if (document.TryGetSyntaxRoot(out root))
                return root;

            return null;
        }
    }
}