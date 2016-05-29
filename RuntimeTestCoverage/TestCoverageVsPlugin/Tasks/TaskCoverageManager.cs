using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using TestCoverage.Extensions;

namespace TestCoverageVsPlugin.Tasks
{
    public class TaskCoverageManager : ITaskCoverageManager
    {
        private const int ExecutionDelayInMilliseconds = 2000;
        private readonly ITimer _timer;
        private readonly IDocumentProvider _documentProvider;
        private readonly IVsSolutionTestCoverage _vsSolutionTestCoverage;
        private readonly List<ITaskCoverageInfo> _tasks;

        public event EventHandler<CoverageTaskArgsBase> CoverageTaskEvent;

        public bool AreJobsPending => _tasks.Count > 0;

        public bool IsBusy { get; private set; }
        public void RaiseEvent(CoverageTaskArgsBase args)
        {
            CoverageTaskEvent?.Invoke(this, args);
        }

        public TaskCoverageManager(ITimer timer,
            IDocumentProvider documentProvider,
            IVsSolutionTestCoverage vsSolutionTestCoverage)
        {
            _timer = timer;
            _documentProvider = documentProvider;
            _tasks = new List<ITaskCoverageInfo>();
            _vsSolutionTestCoverage = vsSolutionTestCoverage;
        }

        public void EnqueueDocumentTask(string projectName, ITextSnapshot textSnapshot, string documentPath)
        {
            _tasks.RemoveAll(x => x.DocumentPath == documentPath);
            
            var existingTask = _tasks.OfType<DocumentCoverageInfoTaskInfo>().
                FirstOrDefault(x => x.DocumentPath == documentPath);
            
            if (existingTask == null)
            {
                var task = new DocumentCoverageInfoTaskInfo(projectName,documentPath, textSnapshot);
                _tasks.Add(task);
            }

            _timer.Schedule(ExecutionDelayInMilliseconds, ExecuteTask);
        }

        public bool EnqueueMethodTask(string projectName, int position, ITextSnapshot textSnapshot, string documentPath)
        {
            if (Path.GetExtension(documentPath) != ".cs")
                return false;

            var root = CSharpSyntaxTree.ParseText(textSnapshot.GetText(), path: documentPath).GetRoot();
            var method = root.GetMethodAt(position);

            if (method == null)
                return false;

            var existingTask = _tasks.OfType<MethodCoverageInfoTaskInfo>().
                FirstOrDefault(x => x.Method.Identifier.ToString() == method.Identifier.ToString());

            if (existingTask == null)
            {
                var task = new MethodCoverageInfoTaskInfo(projectName, method, textSnapshot);
                _tasks.Add(task);
            }
            else
                existingTask.TextSnapshot = textSnapshot;

            IsBusy = true;
            _timer.Schedule(ExecutionDelayInMilliseconds, ExecuteTask);
            return true;
        }

        private void ExecuteTask()
        {
            if (_tasks.Count == 0)
            {
                IsBusy = false;
                return;
            }

            ITaskCoverageInfo taskInfo = _tasks[0];
            _tasks.RemoveAt(0);

            var task = taskInfo.ExecuteAsync(this, _vsSolutionTestCoverage, _documentProvider);

            task.ContinueWith((x, y) =>
            {
                ExecuteTask();
            }, null, TaskSchedulerManager.Current.FromSynchronizationContext());
        }
    }
}