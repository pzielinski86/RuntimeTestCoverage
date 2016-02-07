using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Extensions;
using TestCoverageVsPlugin.Tasks;

namespace TestCoverageVsPlugin
{
    public class TaskCoverageManager : ITaskCoverageManager
    {
        private const int ExecutionDelayInMilliseconds = 2000;
        private readonly ITimer _timer;
        private readonly IVsSolutionTestCoverage _vsSolutionTestCoverage;
        private readonly Queue<MethodCoverageTaskInfo> _tasks;

        public event EventHandler<MethodCoverageTaskArgs> DocumentCoverageTaskCompleted;
        public event EventHandler<MethodCoverageTaskArgs> DocumentCoverageTaskStarted;
        public bool AreJobsPending => _tasks.Count > 0;
        public bool IsBusy { get; private set; }

        public TaskCoverageManager(ITimer timer,
            IVsSolutionTestCoverage vsSolutionTestCoverage)
        {
            _timer = timer;
            _tasks = new Queue<MethodCoverageTaskInfo>();
            _vsSolutionTestCoverage = vsSolutionTestCoverage;
        }

        public void EnqueueMethodTask(string projectName, int position, ITextSnapshot textSnapshot, string documentPath)
        {
            if (Path.GetExtension(documentPath) != ".cs")
                return;

            var document = CSharpSyntaxTree.ParseText(textSnapshot.GetText(), path: documentPath).GetRoot();
            var method = document.GetMethodAt(position);

            if (_tasks.All(x => x.Method.Identifier.ToString() != method.Identifier.ToString()))
            {
                var task = new MethodCoverageTaskInfo(projectName, method);
                _tasks.Enqueue(task);
            }

            IsBusy = true;
            _timer.Schedule(ExecutionDelayInMilliseconds, ExecuteTask);
        }

        private void ExecuteTask()
        {
            if (_tasks.Count == 0)
            {
                IsBusy = false;
                return;
            }

            MethodCoverageTaskInfo taskInfo = _tasks.Dequeue();

            var filePath = taskInfo.Method.SyntaxTree.FilePath;
            DocumentCoverageTaskStarted?.Invoke(this, new MethodCoverageTaskArgs(filePath));

            Task task = _vsSolutionTestCoverage.CalculateForSelectedMethodAsync(taskInfo.ProjectName, taskInfo.Method);

            task.ContinueWith((x, y) =>
            {
                DocumentCalculationsCompleted(filePath);
                ExecuteTask();
            }, null, TaskSchedulerManager.Current.FromSynchronizationContext());
        }

        private void DocumentCalculationsCompleted(string documentPath)
        {
            DocumentCoverageTaskCompleted?.Invoke(this, new MethodCoverageTaskArgs(documentPath));
        }

        class MethodCoverageTaskInfo
        {
            public string ProjectName { get; }
            public MethodDeclarationSyntax Method { get; set; }

            public MethodCoverageTaskInfo(string projectName, MethodDeclarationSyntax method)
            {
                ProjectName = projectName;
                Method = method;
            }
        }
    }
}