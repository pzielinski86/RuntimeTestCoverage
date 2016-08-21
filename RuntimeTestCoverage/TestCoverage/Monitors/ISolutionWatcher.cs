using System;

namespace TestCoverage.Monitors
{
    public interface ISolutionWatcher
    {
        event EventHandler<DocumentRemovedEventArgs> DocumentRemoved;
        void Start();
    }
}