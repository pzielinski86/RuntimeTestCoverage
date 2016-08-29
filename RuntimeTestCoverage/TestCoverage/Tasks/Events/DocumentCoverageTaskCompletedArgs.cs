namespace TestCoverage.Tasks.Events
{
    public class DocumentCoverageTaskCompletedArgs : CoverageTaskArgsBase
    {
        public string DocPath { get; }

        public DocumentCoverageTaskCompletedArgs(string docPath)
        {
            DocPath = docPath;
        }
    }
}