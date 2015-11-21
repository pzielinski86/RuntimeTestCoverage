using System;

namespace TestCoverage.Rewrite
{
    [Serializable]
    public class AuditVariablePlaceholder
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

        public static string AuditVariableStructureName
        {
            get { return "AuditVariable"; }
        }

        public override string ToString()
        {
            string initVarCode = $"new {AuditVariableStructureName}(){{NodePath=\"{NodePath}\",DocumentPath=@\"{DocumentPath}\",Span={SpanStart}}}";

            return initVarCode;
        }
    }
}