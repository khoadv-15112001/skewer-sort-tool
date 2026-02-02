using System;
using System.Collections;

namespace SonatFramework.Systems.TimeManagement
{
    public abstract class TimeService : SonatServiceSo
    {
        public abstract DateTime GetCurrentTime(bool force = true);
        public abstract long GetUnixTimeSeconds(bool force = true);

        public abstract int GetDaysPassed(DateTime timeStart, DateTime timeEnd);

        public abstract IEnumerator DoActionRealtime(long sec, Action<long> action, bool force = true);
    }
}