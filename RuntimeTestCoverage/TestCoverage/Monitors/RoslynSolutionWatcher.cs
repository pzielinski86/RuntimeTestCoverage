using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;
using EnvDTE;
using TestCoverage.Storage;
using TestCoverage.Tasks;
using Document = Microsoft.CodeAnalysis.Document;

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
            if (e.Kind == WorkspaceChangeKind.DocumentRemoved)
            {
                var doc = e.OldSolution.GetDocument(e.DocumentId);
                _coverageStore.RemoveByFile(doc.FilePath);
                _coverageStore.RemoveByDocumentTestNodePath(doc.FilePath);
                _rewrittenDocumentsStorage.RemoveByDocument(doc.FilePath, doc.Project.Name, e.NewSolution.FilePath);

                DocumentRemoved?.Invoke(this, new DocumentRemovedEventArgs(doc.FilePath));
            }
            else if (e.Kind == WorkspaceChangeKind.DocumentChanged)
            {               
                var originalDoc = e.OldSolution.GetDocument(e.DocumentId);
                var newChangedDoc = e.NewSolution.GetDocument(e.DocumentId);

                var changes = await newChangedDoc.GetTextChangesAsync(originalDoc);
                
                if (changes.Any())
                {
                    string activeDocumentPath = _dte.ActiveDocument.FullName;

                    if (!string.Equals(newChangedDoc.FilePath,activeDocumentPath,StringComparison.OrdinalIgnoreCase))
                        _taskCoverageManager.ResyncAll();
                }
            }
        }
    }
}