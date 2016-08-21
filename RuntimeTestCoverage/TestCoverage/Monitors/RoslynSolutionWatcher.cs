using System;
using Microsoft.CodeAnalysis;
using TestCoverage.Storage;

namespace TestCoverage.Monitors
{
    public class RoslynSolutionWatcher:ISolutionWatcher
    {
        public event EventHandler<DocumentRemovedEventArgs> DocumentRemoved;
        private readonly Workspace _workspace;
        private readonly ICoverageStore _coverageStore;
        private readonly IRewrittenDocumentsStorage _rewrittenDocumentsStorage;

        public RoslynSolutionWatcher(Workspace workspace, ICoverageStore coverageStore,IRewrittenDocumentsStorage rewrittenDocumentsStorage)
        {
            _workspace = workspace;
            _coverageStore = coverageStore;
            _rewrittenDocumentsStorage = rewrittenDocumentsStorage;
        }

        public void Start()
        {
            _workspace.WorkspaceChanged += WorkspaceChanged;
        }

        private void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            if (e.Kind == WorkspaceChangeKind.DocumentRemoved)
            {
                var doc = e.OldSolution.GetDocument(e.DocumentId);
                _coverageStore.RemoveByFile(doc.FilePath);
                _coverageStore.RemoveByDocumentTestNodePath(doc.FilePath);
                _rewrittenDocumentsStorage.RemoveByDocument(doc.FilePath,doc.Project.Name,e.NewSolution.FilePath);

                DocumentRemoved?.Invoke(this,new DocumentRemovedEventArgs(doc.FilePath));
            }
        }
    }
}