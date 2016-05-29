namespace TestCoverageVsPlugin.Tasks.Events
{
    public class MethodCoverageTaskStartedArgs : CoverageTaskArgsBase
    {
        public string MethodName { get; }

        public MethodCoverageTaskStartedArgs(string docPath,string methodName):base(docPath)
        {
            MethodName = methodName;
        }
    }
}