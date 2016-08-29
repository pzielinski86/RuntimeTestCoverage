namespace TestCoverage.Tasks.Events
{
    public class MethodCoverageTaskCompletedArgs : CoverageTaskArgsBase
    {
        public string DocPath { get; }
        public string MethodName { get; }

        public MethodCoverageTaskCompletedArgs(string docPath, string methodName)
        {
            DocPath = docPath;
            MethodName = methodName;
        }
    }
}