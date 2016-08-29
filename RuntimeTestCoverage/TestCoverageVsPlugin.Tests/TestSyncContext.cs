using System.Threading;

namespace TestCoverageVsPlugin.Tests
{
    public class TestSyncContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object state)
        {
            d.Invoke(state);
        }
    }
}