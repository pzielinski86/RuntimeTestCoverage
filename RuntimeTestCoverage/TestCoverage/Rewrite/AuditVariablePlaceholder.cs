using System;

namespace TestCoverage.Rewrite
{
    [Serializable]
    public class AuditVariablePlaceholder
    {
        public string DocumentPath { get; set; }
        public string NodePath { get; private set; }
        public int SpanStart { get; private set; }
        public int ExecutionCounter { get; }

        public AuditVariablePlaceholder(string documentPath,string nodePath, int spanStart):
            this(documentPath,nodePath,spanStart,0)
        {
        }

        public AuditVariablePlaceholder(string documentPath, string nodePath, int spanStart,int executionCounter)
        {
            DocumentPath = documentPath;
            NodePath = nodePath;
            SpanStart = spanStart;
            ExecutionCounter = executionCounter;
        }

        public static string AuditVariableStructureName
        {
            get { return "AuditVariable"; }
        }

        public string GetKey() => $"{DocumentPath}_{SpanStart}";
        public string GetInitializationCode()
        {
            string initVarCode = $"new {AuditVariableStructureName}()" +
                                 $"{{NodePath=\"{NodePath}\"," +
                                 $"DocumentPath=@\"{DocumentPath}\"," +
                                 $"Span={SpanStart}, " +
                                 $"ExecutionCounter={AuditVariablesMap.ExecutionCounterCall}++}}";

            return initVarCode;
        }
    }
}