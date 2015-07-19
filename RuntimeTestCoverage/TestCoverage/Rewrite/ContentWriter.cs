using System.IO;

namespace TestCoverage.Rewrite
{
    public class ContentWriter : IContentWriter
    {
        public void Write(RewrittenItemInfo item)
        {
            File.WriteAllText(PathHelper.GetRewrittenFilePath(item.DocumentPath), item.SyntaxTree.ToString());
        }
    }
}