namespace TestCoverage.Tasks
{
    public interface IDocumentBasedTaskCoverageInfo : ITaskCoverageInfo
    {
        string DocumentPath { get; }
    }
}