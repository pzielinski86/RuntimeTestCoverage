using System;

namespace TestCoverageVsPlugin
{
    public interface ITimer
    {
        void Schedule(int millisecondsFromNow, Action action);
    }
}