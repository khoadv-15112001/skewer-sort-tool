using System;
using UnityEngine;

namespace GrillSort.Schedule
{
    [Serializable]
    public class TimeConfig
    {
        public DayConfig eventStartDay;
        public DayConfig eventEndDay;
        public bool useTimeLocal = true;

        public TimeConfig()
        {
            eventStartDay = new DayConfig();
            eventEndDay = new DayConfig();
        }

        public TimeConfig(TimeConfig other)
        {
            if (other == null)
            {
                eventStartDay = new DayConfig();
                eventEndDay = new DayConfig();
                return;
            }
            eventStartDay = other.eventStartDay != null ? new DayConfig(other.eventStartDay) : new DayConfig();
            eventEndDay = other.eventEndDay != null ? new DayConfig(other.eventEndDay) : new DayConfig();
        }

        public TimeConfig Clone() => new TimeConfig(this);
    }

    [Serializable]
    public class DayConfig
    {
        public DayOfWeek day;
        [Range(0, 23)] public int hour;

        public DayConfig() { }

        public DayConfig(DayConfig other)
        {
            if (other == null) return;
            day = other.day;
            hour = other.hour;
        }
    }
}