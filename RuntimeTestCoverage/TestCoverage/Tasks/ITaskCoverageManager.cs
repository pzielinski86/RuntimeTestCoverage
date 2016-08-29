using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using TestCoverage.Tasks.Events;

namespace TestCoverage.Tasks
{
    public interface ITaskCoverageManager
    {
        bool EnqueueMethodTask(string projectName, int position, ITextBuffer textSnapshot, string documentPath);
        void EnqueueDocumentTask(string projectName, ITextBuffer textSnapshot, string documentPath);
        void ReportTaskToRetry(IDocumentBasedTaskCoverageInfo task);
        event EventHandler<CoverageTaskArgsBase> CoverageTaskEvent;
        bool AreJobsPending { get; }
        bool IsBusy { get; }

        void RaiseEvent(CoverageTaskArgsBase args);
        List<ITaskCoverageInfo> Tasks { get; }
        void ResyncAll();
    }
}