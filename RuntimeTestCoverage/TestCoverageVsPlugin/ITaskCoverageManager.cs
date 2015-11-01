using System;

namespace TestCoverageVsPlugin
{
    public interface ITaskCoverageManager
    {
        void EnqueueDocumentTask(string projectName, string documentPath, string documentContent);
        event EventHandler<DocumentCoverageTaskCompletedArgs> DocumentCoverageTaskCompleted;
        event EventHandler<DocumentCoverageTaskCompletedArgs> DocumentCoverageTaskStarted;

        bool AreJobsPending { get; }
        bool IsBusy { get; }
    }
}