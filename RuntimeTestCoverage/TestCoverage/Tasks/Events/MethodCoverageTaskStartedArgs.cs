namespace TestCoverage.Tasks.Events
{
    public class MethodCoverageTaskStartedArgs : CoverageTaskArgsBase
    {
        public string DocPath { get; }
        public string MethodName { get; }

        public MethodCoverageTaskStartedArgs(string docPath,string methodName)
        {
            DocPath = docPath;
            MethodName = methodName;
        }
    }
}