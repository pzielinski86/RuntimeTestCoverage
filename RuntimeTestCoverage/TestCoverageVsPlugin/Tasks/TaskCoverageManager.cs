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
        private readonly Queue<MethodCoverageTaskInfo> _tasks;

        public event EventHandler<MethodCoverageTaskArgs> MethodCoverageTaskCompleted;
        public event EventHandler<MethodCoverageTaskArgs> MethodCoverageTaskStarted;
        public bool AreJobsPending => _tasks.Count > 0;
        public bool IsBusy { get; private set; }

        public TaskCoverageManager(ITimer timer,
            IDocumentProvider documentProvider,
            IVsSolutionTestCoverage vsSolutionTestCoverage)
        {
            _timer = timer;
            _documentProvider = documentProvider;
            _tasks = new Queue<MethodCoverageTaskInfo>();
            _vsSolutionTestCoverage = vsSolutionTestCoverage;
        }

        public void EnqueueMethodTask(string projectName, int position, ITextSnapshot textSnapshot, string documentPath)
        {
            if (Path.GetExtension(documentPath) != ".cs")
                return;

            var root = CSharpSyntaxTree.ParseText(textSnapshot.GetText(), path: documentPath).GetRoot();
            var method = root.GetMethodAt(position);

            if (method == null)
                return;

            var existingTask = _tasks.FirstOrDefault(x => x.Method.Identifier.ToString() == method.Identifier.ToString());

            if (existingTask == null)
            {
                var task = new MethodCoverageTaskInfo(projectName,method, textSnapshot);
                _tasks.Enqueue(task);
            }
            else
                existingTask.TextSnapshot = textSnapshot;

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
            string methodName = taskInfo.Method.Identifier.ValueText;
            MethodCoverageTaskStarted?.Invoke(this, new MethodCoverageTaskArgs(filePath, methodName));

            var documentSyntaxTree = _documentProvider.GetSyntaxNodeFromTextSnapshot(taskInfo.TextSnapshot);
            
            var methodNode = documentSyntaxTree.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(x => x.Identifier.ValueText == methodName);

            Task task = _vsSolutionTestCoverage.CalculateForSelectedMethodAsync(taskInfo.ProjectName, methodNode);

            task.ContinueWith((x, y) =>
            {
                MethodCalculationsCompleted(filePath, methodName);
                ExecuteTask();
            }, null, TaskSchedulerManager.Current.FromSynchronizationContext());
        }

        private void MethodCalculationsCompleted(string documentPath, string methodName)
        {
            MethodCoverageTaskCompleted?.Invoke(this, new MethodCoverageTaskArgs(documentPath, methodName));
        }

        class MethodCoverageTaskInfo
        {
            public string ProjectName { get; }
            public MethodDeclarationSyntax Method { get; }
            public ITextSnapshot TextSnapshot { get; set; }

            public MethodCoverageTaskInfo(string projectName, MethodDeclarationSyntax method, ITextSnapshot textSnapshot)
            {
                ProjectName = projectName;
                Method = method;
                TextSnapshot = textSnapshot;
            }
        }
    }
}