using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestCoverageVsPlugin
{
    public class TaskCoverageManager : ITaskCoverageManager
    {
        private const int ExecutionDelayInMilliseconds = 2000;
        private readonly ITimer _timer;
        private readonly IVsSolutionTestCoverage _vsSolutionTestCoverage;
        private readonly IDocumentFromTextSnapshotExtractor _documentFromTextSnapshotExtractor;
        private readonly Queue<MethodCoverageTaskInfo> _tasks;

        public event EventHandler<MethodCoverageTaskCompletedArgs> DocumentCoverageTaskCompleted;
        public event EventHandler<MethodCoverageTaskCompletedArgs> DocumentCoverageTaskStarted;
        public bool AreJobsPending => _tasks.Count > 0;
        public bool IsBusy { get; private set; }

        public TaskCoverageManager(ITimer timer, 
            IVsSolutionTestCoverage vsSolutionTestCoverage,
            IDocumentFromTextSnapshotExtractor documentFromTextSnapshotExtractor)
        {
            _timer = timer;
            _tasks = new Queue<MethodCoverageTaskInfo>();
            _vsSolutionTestCoverage = vsSolutionTestCoverage;
            _documentFromTextSnapshotExtractor = documentFromTextSnapshotExtractor;
        }

        public void EnqueueMethodTask(string projectName, int position, ITextSnapshot textSnapshot, string documentPath)
        {
            var existingTask = _tasks.FirstOrDefault(x => x.DocumentPath == documentPath);

            if (existingTask == null)
            {
                var task = new MethodCoverageTaskInfo(projectName, textSnapshot, position,documentPath);
                _tasks.Enqueue(task);
            }
            else
            {
                existingTask.TextSnapshot = textSnapshot;
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

            var rootNode = _documentFromTextSnapshotExtractor.ExtactDocument(taskInfo.TextSnapshot);

            DocumentCoverageTaskStarted?.Invoke(this, new MethodCoverageTaskCompletedArgs(taskInfo.DocumentPath));

            Task task = _vsSolutionTestCoverage.CalculateForSelectedMethodAsync(taskInfo.ProjectName,
                taskInfo.Position,
                rootNode);
            
            task.ContinueWith((x, y) => DocumentCalculationsCompleted(taskInfo.DocumentPath), null, TaskScheduler.FromCurrentSynchronizationContext()).
                ContinueWith((x, y) => ExecuteTask(), null, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DocumentCalculationsCompleted(string documentPath)
        {
            DocumentCoverageTaskCompleted?.Invoke(this, new MethodCoverageTaskCompletedArgs(documentPath));
        }

        class MethodCoverageTaskInfo
        {
            public string ProjectName { get; }
            public ITextSnapshot TextSnapshot { get; set; }
            public int Position { get; }
            public string DocumentPath { get;  }

            public MethodCoverageTaskInfo(string projectName, ITextSnapshot textSnapshot, int position, string documentPath)
            {
                ProjectName = projectName;
                TextSnapshot = textSnapshot;
                Position = position;
                DocumentPath = documentPath;
            }
        }
    }
}