using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using TestCoverageVsPlugin.Tasks.Events;

namespace TestCoverageVsPlugin.Tasks
{
    public class DocumentCoverageInfoTaskInfo : ITaskCoverageInfo
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
                    taskCoverageManager.Tasks.RemoveAll(t => t.DocumentPath == DocumentPath);
                else
                    taskCoverageManager.ReportTaskToRetry(this);

                taskCoverageManager.RaiseEvent(new DocumentCoverageTaskCompletedArgs(DocumentPath));

                return finishedTask.Result;
            }, null, TaskSchedulerManager.Current.FromSynchronizationContext());

            return finalTask;
        }
    }
}