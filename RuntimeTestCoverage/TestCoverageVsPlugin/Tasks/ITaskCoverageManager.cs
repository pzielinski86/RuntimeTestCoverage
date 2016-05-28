using System;
using Microsoft.VisualStudio.Text;

namespace TestCoverageVsPlugin.Tasks
{
    public interface ITaskCoverageManager
    {
        void EnqueueMethodTask(string projectName, int position, ITextSnapshot textSnapshot, string documentPath);
        event EventHandler<MethodCoverageTaskArgs> MethodCoverageTaskCompleted;
        event EventHandler<MethodCoverageTaskArgs> MethodCoverageTaskStarted;

        bool AreJobsPending { get; }
        bool IsBusy { get; }
    }
}