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
        private readonly Queue<DocumentCoverageTaskInfo> _tasks;

        public event EventHandler<DocumentCoverageTaskCompletedArgs> DocumentCoverageTaskCompleted;
        public event EventHandler<DocumentCoverageTaskCompletedArgs> DocumentCoverageTaskStarted;
        public bool AreJobsPending => _tasks.Count > 0;
        public bool IsBusy { get; private set; }

        public TaskCoverageManager(ITimer timer, IVsSolutionTestCoverage vsSolutionTestCoverage)
        {
            _timer = timer;
            _tasks = new Queue<DocumentCoverageTaskInfo>();
            _vsSolutionTestCoverage = vsSolutionTestCoverage;
        }

        public void EnqueueDocumentTask(string projectName, string documentPath, string documentContent)
        {
            var existingTask = _tasks.FirstOrDefault(x => x.DocumentPath == documentPath);

            if (existingTask == null)
            {
                var task = new DocumentCoverageTaskInfo(projectName, documentPath, documentContent);
                _tasks.Enqueue(task);
            }
            else
            {
                existingTask.DocumentContent = documentContent;
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

            DocumentCoverageTaskInfo taskInfo = _tasks.Dequeue();
            DocumentCoverageTaskStarted?.Invoke(this,new DocumentCoverageTaskCompletedArgs(taskInfo.DocumentPath));

            Task task = _vsSolutionTestCoverage.CalculateForDocumentAsync(taskInfo.ProjectName,
                taskInfo.DocumentPath,
                taskInfo.DocumentContent);

            string documentPath = taskInfo.DocumentPath;

            task.ContinueWith((x, y) => DocumentCalculationsCompleted(documentPath), null, TaskScheduler.FromCurrentSynchronizationContext())
                .ContinueWith((x, y) => PreCreateAppDomain(),null).
                ContinueWith((x, y) => ExecuteTask(), null, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private Task PreCreateAppDomain()
        {
            return _vsSolutionTestCoverage.InitAsync(false);
        }

        private void DocumentCalculationsCompleted(string documentPath)
        {
            DocumentCoverageTaskCompleted?.Invoke(this, new DocumentCoverageTaskCompletedArgs(documentPath));
        }

        class DocumentCoverageTaskInfo
        {
            public string ProjectName { get; }
            public string DocumentPath { get; }
            public string DocumentContent { get; set; }

            public DocumentCoverageTaskInfo(string projectName, string documentPath, string documentContent)
            {
                ProjectName = projectName;
                DocumentPath = documentPath;
                DocumentContent = documentContent;
            }
        }
    }
}