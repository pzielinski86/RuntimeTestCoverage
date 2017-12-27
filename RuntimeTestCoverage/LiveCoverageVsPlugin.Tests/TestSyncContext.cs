using System.Threading;

namespace LiveCoverageVsPlugin.Tests
{
    public class TestSyncContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object state)
        {
            d.Invoke(state);
        }
    }
}