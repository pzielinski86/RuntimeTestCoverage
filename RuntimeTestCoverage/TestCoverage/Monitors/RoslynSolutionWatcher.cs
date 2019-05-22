using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;
using EnvDTE;
using TestCoverage.Storage;
using TestCoverage.Tasks;
using Document = Microsoft.CodeAnalysis.Document;
using System.Collections.Generic;

namespace TestCoverage.Monitors
{
    public class RoslynSolutionWatcher : ISolutionWatcher
    {
        public event EventHandler<DocumentRemovedEventArgs> DocumentRemoved;
        private readonly DTE _dte;
        private readonly Workspace _workspace;
        private readonly ICoverageStore _coverageStore;
        private readonly IRewrittenDocumentsStorage _rewrittenDocumentsStorage;
        private readonly ITaskCoverageManager _taskCoverageManager;

        public RoslynSolutionWatcher(DTE dte, Workspace workspace, ICoverageStore coverageStore, IRewrittenDocumentsStorage rewrittenDocumentsStorage, ITaskCoverageManager taskCoverageManager)
        {
            _dte = dte;
            _workspace = workspace;
            _coverageStore = coverageStore;
            _rewrittenDocumentsStorage = rewrittenDocumentsStorage;
            _taskCoverageManager = taskCoverageManager;
        }

        public void Start()
        {
            _workspace.WorkspaceChanged += WorkspaceChanged;
        }

        private async void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.Kind == WorkspaceChangeKind.ProjectChanged)
            {
                //TODO: Temporary solution until https://stackoverflow.com/questions/56256559/roslyn-workspacechangekind-documentremoved-never-raised is resolved.
                // Currently there is no easy way to determine if a file was renamed.
                IEnumerable<string> oldDocuments = e.OldSolution.GetProject(e.ProjectId).Documents.Select(doc => doc.FilePath);
                IEnumerable<string> newDocuments = e.NewSolution.GetProject(e.ProjectId).Documents.Select(doc => doc.FilePath);
                if (oldDocuments.Where(doc => !newDocuments.Contains(doc)).Any())
                {
                    _taskCoverageManager.ResyncAll();
                }
            }

            if (e.Kind == WorkspaceChangeKind.DocumentRemoved)
            {
                Document document = e.OldSolution.GetDocument(e.DocumentId);
                OnDocumentRemoved(document.FilePath, document.Project.Name, e.OldSolution.FilePath);
            }
            else if (e.Kind == WorkspaceChangeKind.DocumentChanged)
            {
                await OnDocumentChanged(e);
            }
        }

        private void OnDocumentRemoved(string documentFilePath, string projectName, string solutionPath)
        {
            _coverageStore.RemoveByFile(documentFilePath);
            _coverageStore.RemoveByDocumentTestNodePath(documentFilePath);
            _rewrittenDocumentsStorage.RemoveByDocument(documentFilePath, projectName, solutionPath);

            DocumentRemoved?.Invoke(this, new DocumentRemovedEventArgs(documentFilePath));
        }

        private async System.Threading.Tasks.Task OnDocumentChanged(WorkspaceChangeEventArgs e)
        {
            var originalDoc = e.OldSolution.GetDocument(e.DocumentId);
            var newChangedDoc = e.NewSolution.GetDocument(e.DocumentId);

            IEnumerable<Microsoft.CodeAnalysis.Text.TextChange> changes = await newChangedDoc.GetTextChangesAsync(originalDoc);

            if (changes.Any())
            {
                string activeDocumentPath = _dte.ActiveDocument.FullName;

                if (!string.Equals(newChangedDoc.FilePath, activeDocumentPath, StringComparison.OrdinalIgnoreCase))
                    _taskCoverageManager.ResyncAll();
            }
        }
    }
}