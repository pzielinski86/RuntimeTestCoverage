namespace TestCoverage.Rewrite
{
    internal class AuditVariablePlaceholder
    {
        public string DocumentPath { get; set; }
        public string NodePath { get; private set; }
        public int SpanStart { get; private set; }

        public AuditVariablePlaceholder(string documentPath,string nodePath, int spanStart)
        {
            DocumentPath = documentPath;
            NodePath = nodePath;
            SpanStart = spanStart;
        }
    }
}