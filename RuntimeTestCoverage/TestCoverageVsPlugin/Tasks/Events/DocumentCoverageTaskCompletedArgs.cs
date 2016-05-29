namespace TestCoverageVsPlugin.Tasks.Events
{
    public class DocumentCoverageTaskCompletedArgs : CoverageTaskArgsBase
    {
        public DocumentCoverageTaskCompletedArgs(string docPath) : base(docPath)
        {
        }
    }
}