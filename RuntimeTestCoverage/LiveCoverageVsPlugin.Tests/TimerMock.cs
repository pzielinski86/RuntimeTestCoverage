using System;
using System.Threading;

namespace LiveCoverageVsPlugin.Tests
{
    class TimerMock : ITimer
    {
        private Action _action;

        public void Schedule(int millisecondsFromNow, Action action)
        {
            _action = action;
        }

        public void ExecuteNow()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            _action();
        }
    }
}