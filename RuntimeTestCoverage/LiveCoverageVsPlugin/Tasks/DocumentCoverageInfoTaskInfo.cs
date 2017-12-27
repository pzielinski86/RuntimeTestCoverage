using Microsoft.VisualStudio.Text;
using System.Threading.Tasks;
using TestCoverage.Tasks;
using TestCoverage.Tasks.Events;

namespace LiveCoverageVsPlugin.Tasks
{
    public class DocumentCoverageInfoTaskInfo : IDocumentBasedTaskCoverageInfo
    {
        public DocumentCoverageInfoTaskInfo(string projectName, string documentPath, ITextBuffer textBuffer)
        {
            ProjectName = projectName;
            DocumentPath = documentPath;
            TextBuffer = textBuffer;
        }

        public string ProjectName { get; }

        public ITextBuffer TextBuffer { get; set; }

        public string DocumentPath { set; get; }

        public Task<bool> ExecuteAsync(ITaskCoverageManager taskCoverageManager, IVsSolutionTestCoverage vsSolutionTestCoverage,
            IDocumentProvider documentProvider)
        {
            taskCoverageManager.RaiseEvent(new DocumentCoverageTaskStartedArgs(DocumentPath));

            string documentContent = TextBuffer.CurrentSnapshot.GetText();
            var task = vsSolutionTestCoverage.CalculateForDocumentAsync(ProjectName, DocumentPath, documentContent);


            var finalTask = task.ContinueWith((finishedTask, y) =>
            {
                if (finishedTask.Result)
                    taskCoverageManager.Tasks.RemoveAll(t => IsTaskInDocument(t, DocumentPath));
                else
                    taskCoverageManager.ReportTaskToRetry(this);

                taskCoverageManager.RaiseEvent(new DocumentCoverageTaskCompletedArgs(DocumentPath));

                return finishedTask.Result;
            }, null, TaskSchedulerManager.Current.FromSynchronizationContext());

            return finalTask;
        }

        private bool IsTaskInDocument(ITaskCoverageInfo taskCoverageInfo, string documentPath)
        {
            if (taskCoverageInfo is IDocumentBasedTaskCoverageInfo)
                return ((IDocumentBasedTaskCoverageInfo)taskCoverageInfo).DocumentPath == documentPath;

            return false;
        }
    }
}