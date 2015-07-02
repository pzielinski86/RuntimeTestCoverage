namespace TestCoverage
{
    internal class AuditVariablePlaceholder
    {
        public string Path { get; private set; }
        public int SpanStart { get; private set; }

        public AuditVariablePlaceholder(string path, int spanStart)
        {
            Path = path;
            SpanStart = spanStart;
        }
    }
}