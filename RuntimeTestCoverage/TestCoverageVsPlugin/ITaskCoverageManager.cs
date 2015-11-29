using Microsoft.VisualStudio.Text;
using System;

namespace TestCoverageVsPlugin
{
    public interface ITaskCoverageManager
    {
        void EnqueueMethodTask(string projectName, int position, ITextSnapshot textSnapshot, string documentPath);
        event EventHandler<MethodCoverageTaskArgs> DocumentCoverageTaskCompleted;
        event EventHandler<MethodCoverageTaskArgs> DocumentCoverageTaskStarted;

        bool AreJobsPending { get; }
        bool IsBusy { get; }
    }
}