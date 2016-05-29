using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using TestCoverageVsPlugin.Tasks.Events;

namespace TestCoverageVsPlugin.Tasks
{
    public class DocumentCoverageInfoTaskInfo:ITaskCoverageInfo
    {
        public DocumentCoverageInfoTaskInfo(string projectName, string documentPath,ITextSnapshot textSnapshot)
        {
            ProjectName = projectName;
            DocumentPath = documentPath;
            TextSnapshot = textSnapshot;
        }

        public string ProjectName { get; }

        public ITextSnapshot TextSnapshot { get; set; }

        public string DocumentPath { set; get; }

        public Task ExecuteAsync(ITaskCoverageManager taskCoverageManager, IVsSolutionTestCoverage vsSolutionTestCoverage,
            IDocumentProvider documentProvider)
        {
            taskCoverageManager.RaiseEvent(new DocumentCoverageTaskStartedArgs(DocumentPath));

            string documentContent = TextSnapshot.GetText();
            var task = vsSolutionTestCoverage.CalculateForDocumentAsync(ProjectName, DocumentPath, documentContent);


            var finalTask = task.ContinueWith((x, y) =>
            {
                taskCoverageManager.RaiseEvent(new DocumentCoverageTaskCompletedArgs(DocumentPath));
            }, null, TaskSchedulerManager.Current.FromSynchronizationContext());

            return finalTask;
        }
    }
}