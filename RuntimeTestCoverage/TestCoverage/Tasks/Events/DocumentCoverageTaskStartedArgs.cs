namespace TestCoverage.Tasks.Events
{
    public class DocumentCoverageTaskStartedArgs : CoverageTaskArgsBase
    {
        public string DocPath { get; }

        public DocumentCoverageTaskStartedArgs(string docPath)
        {
            DocPath = docPath;
        }
    }
}