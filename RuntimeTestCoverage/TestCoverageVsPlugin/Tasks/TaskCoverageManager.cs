using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestCoverage.Extensions;
using TestCoverage.Tasks;
using TestCoverage.Tasks.Events;

namespace TestCoverageVsPlugin.Tasks
{
    public class TaskCoverageManager : ITaskCoverageManager
    {
        private const int ExecutionDelayInMilliseconds = 2000;
        private readonly ITimer _timer;
        private readonly IDocumentProvider _documentProvider;
        private readonly IVsSolutionTestCoverage _vsSolutionTestCoverage;
        private readonly List<ITaskCoverageInfo> _tasks;
        private readonly Dictionary<string, ITaskCoverageInfo> _unsuccessfulDocumentTasks = new Dictionary<string, ITaskCoverageInfo>();

        public event EventHandler<CoverageTaskArgsBase> CoverageTaskEvent;

        public bool AreJobsPending => _tasks.Count > 0;

        public bool IsBusy { get; private set; }

        public void RaiseEvent(CoverageTaskArgsBase args)
        {
            CoverageTaskEvent?.Invoke(this, args);
        }

        public void ReportTaskToRetry(IDocumentBasedTaskCoverageInfo task)
        {
            _unsuccessfulDocumentTasks[task.DocumentPath]= task;
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

        public List<ITaskCoverageInfo> Tasks => _tasks;
        public void ResyncAll()
        {
            Tasks.Clear();

            var task = new ResyncAllTaskInfo();
            _tasks.Add(task);

            _timer.Schedule(ExecutionDelayInMilliseconds, ExecuteTask);
        }

        public void EnqueueDocumentTask(string projectName, ITextBuffer textBuffer, string documentPath)
        {
            var existingTask = _tasks.OfType<DocumentCoverageInfoTaskInfo>().
                FirstOrDefault(x => x.DocumentPath == documentPath);

            if (existingTask == null)
            {
                var task = new DocumentCoverageInfoTaskInfo(projectName, documentPath, textBuffer);
                _tasks.Add(task);
            }

            _timer.Schedule(ExecutionDelayInMilliseconds, ExecuteTask);
        }

        public bool EnqueueMethodTask(string projectName, int position, ITextBuffer textBuffer, string documentPath)
        {
            if (Path.GetExtension(documentPath) != ".cs")
                return false;

            var root = CSharpSyntaxTree.ParseText(textBuffer.CurrentSnapshot.GetText(), path: documentPath).GetRoot();
            var method = root.GetMethodAt(position);

            if (method == null)
                return false;

            var existingTask = _tasks.OfType<MethodCoverageInfoTaskInfo>().
                FirstOrDefault(x => x.Method.Identifier.ToString() == method.Identifier.ToString());

            if (existingTask == null)
            {
                var task = new MethodCoverageInfoTaskInfo(projectName, method, textBuffer);
                _tasks.Add(task);
            }

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

            task.ContinueWith((finishedTask, y) =>
            {
                if (finishedTask.Result)
                {
                    var documentBasedTask = taskInfo as IDocumentBasedTaskCoverageInfo;

                    if (documentBasedTask != null)
                        RerunFailedDocuments();
                }

                ExecuteTask();
            }, null, TaskSchedulerManager.Current.FromSynchronizationContext());
        }

        private void RerunFailedDocuments()
        {
            foreach (var unsuccessfulDocumentTask in _unsuccessfulDocumentTasks)
            {
                Tasks.Add(_unsuccessfulDocumentTasks[unsuccessfulDocumentTask.Key]);
            }

            _unsuccessfulDocumentTasks.Clear();
        }
    }
}