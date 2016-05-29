using System;
using Microsoft.VisualStudio.Text;
using TestCoverageVsPlugin.Annotations;

namespace TestCoverageVsPlugin.Tasks
{
    public interface ITaskCoverageManager
    {
        bool EnqueueMethodTask(string projectName, int position, ITextSnapshot textSnapshot, string documentPath);
        void EnqueueDocumentTask(string projectName, ITextSnapshot textSnapshot, string documentPath);
        event EventHandler<CoverageTaskArgsBase> CoverageTaskEvent;
        bool AreJobsPending { get; }
        bool IsBusy { get; }

        void RaiseEvent(CoverageTaskArgsBase args);
    }
}