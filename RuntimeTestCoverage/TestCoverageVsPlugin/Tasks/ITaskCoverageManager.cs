using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using TestCoverageVsPlugin.Annotations;

namespace TestCoverageVsPlugin.Tasks
{
    public interface ITaskCoverageManager
    {
        bool EnqueueMethodTask(string projectName, int position, ITextBuffer textSnapshot, string documentPath);
        void EnqueueDocumentTask(string projectName, ITextBuffer textSnapshot, string documentPath);
        void ReportTaskToRetry(ITaskCoverageInfo task);
        event EventHandler<CoverageTaskArgsBase> CoverageTaskEvent;
        bool AreJobsPending { get; }
        bool IsBusy { get; }

        void RaiseEvent(CoverageTaskArgsBase args);
        List<ITaskCoverageInfo> Tasks { get; }
    }
}