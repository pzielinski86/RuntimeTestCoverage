namespace TestCoverageVsPlugin.Tasks.Events
{
    public class MethodCoverageTaskCompletedArgs : CoverageTaskArgsBase
    {
        public string MethodName { get; }

        public MethodCoverageTaskCompletedArgs(string docPath, string methodName) : base(docPath)
        {
            MethodName = methodName;
        }
    }
}