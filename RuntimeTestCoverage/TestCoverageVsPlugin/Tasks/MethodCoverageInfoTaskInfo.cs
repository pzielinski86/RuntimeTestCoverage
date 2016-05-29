using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Text;
using TestCoverageVsPlugin.Tasks.Events;

namespace TestCoverageVsPlugin.Tasks
{
    class MethodCoverageInfoTaskInfo : ITaskCoverageInfo
    {
        public string ProjectName { get; }
        public MethodDeclarationSyntax Method { get; }
        public ITextSnapshot TextSnapshot { get; set; }
        public string DocumentPath => Method.SyntaxTree.FilePath;
        public string MethodName => Method.Identifier.ValueText;

        public MethodCoverageInfoTaskInfo(string projectName, MethodDeclarationSyntax method, ITextSnapshot textSnapshot)
        {
            ProjectName = projectName;
            Method = method;
            TextSnapshot = textSnapshot;
        }

        public Task ExecuteAsync(ITaskCoverageManager taskCoverageManager, IVsSolutionTestCoverage vsSolutionTestCoverage, IDocumentProvider documentProvider)
        {
          
            string methodName = Method.Identifier.ValueText;
            RaiseTaskStartedEvent(taskCoverageManager);

            var documentSyntaxTree = documentProvider.GetSyntaxNodeFromTextSnapshot(TextSnapshot);

            var methodNode = documentSyntaxTree.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(x => x.Identifier.ValueText == methodName);

            Task task = vsSolutionTestCoverage.CalculateForSelectedMethodAsync(ProjectName, methodNode);

            var finalTask = task.ContinueWith((x, y) =>
            {
                RaiseTasCompletedEvent(taskCoverageManager);
            }, null, TaskSchedulerManager.Current.FromSynchronizationContext());

            return finalTask;
        }

        private void RaiseTaskStartedEvent(ITaskCoverageManager taskCoverageManager)
        {
            var args=new MethodCoverageTaskStartedArgs(DocumentPath, MethodName);

            taskCoverageManager.RaiseEvent(args);
        }

        private void RaiseTasCompletedEvent(ITaskCoverageManager taskCoverageManager)
        {
            var args = new MethodCoverageTaskCompletedArgs(DocumentPath, MethodName);

            taskCoverageManager.RaiseEvent(args);
        }
    }
}