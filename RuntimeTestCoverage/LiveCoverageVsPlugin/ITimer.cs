using System;

namespace LiveCoverageVsPlugin
{
    public interface ITimer
    {
        void Schedule(int millisecondsFromNow, Action action);
    }
}