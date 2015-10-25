using System.IO;
using Microsoft.CodeAnalysis;

namespace TestCoverage.Rewrite
{
    public class ContentWriter : IContentWriter
    {
        public void Write(string documentPath, SyntaxTree syntaxTree)
        {
            File.WriteAllText(PathHelper.GetRewrittenFilePath(documentPath), syntaxTree.ToString());
        }
    }
}