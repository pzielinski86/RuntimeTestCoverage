namespace TestCoverageVsPlugin.Tasks.Events
{
    public class DocumentCoverageTaskStartedArgs : CoverageTaskArgsBase
    {
        public DocumentCoverageTaskStartedArgs(string docPath) : base(docPath)
        {
        }
    }
}