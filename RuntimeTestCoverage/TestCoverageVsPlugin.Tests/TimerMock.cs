using System;

namespace TestCoverageVsPlugin.Tests
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
            _action();
        }
    }
}