using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Text;
using System.Linq;
using System.Threading.Tasks;
using TestCoverage.Tasks;
using TestCoverage.Tasks.Events;

namespace TestCoverageVsPlugin.Tasks
{
    public class MethodCoverageInfoTaskInfo : IDocumentBasedTaskCoverageInfo
    {
        public string ProjectName { get; }
        public MethodDeclarationSyntax Method { get; }
        public ITextBuffer TextBuffer { get; set; }
        public string DocumentPath => Method.SyntaxTree.FilePath;
        public string MethodName => Method.Identifier.ValueText;

        public MethodCoverageInfoTaskInfo(string projectName, MethodDeclarationSyntax method, ITextBuffer textBuffer)
        {
            ProjectName = projectName;
            Method = method;
            TextBuffer = textBuffer;
        }

        public Task<bool> ExecuteAsync(ITaskCoverageManager taskCoverageManager, IVsSolutionTestCoverage vsSolutionTestCoverage, IDocumentProvider documentProvider)
        {
          
            string methodName = Method.Identifier.ValueText;
            RaiseTaskStartedEvent(taskCoverageManager);

            var documentSyntaxTree = documentProvider.GetSyntaxNodeFromTextSnapshot(TextBuffer.CurrentSnapshot);

            var methodNode = documentSyntaxTree.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(x => x.Identifier.ValueText == methodName);

            if (methodNode == null)
                return Task.FromResult(false);

            Task<bool> task = vsSolutionTestCoverage.CalculateForSelectedMethodAsync(ProjectName, methodNode);

            var finalTask = task.ContinueWith((x, y) =>
            {
                RaiseTasCompletedEvent(taskCoverageManager);
                return x.Result;

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