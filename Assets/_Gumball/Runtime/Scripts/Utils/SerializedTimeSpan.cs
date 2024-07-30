using System;
using UnityEngine;

namespace Gumball
{
    /// <summary>
    /// Provides an easier way to set time spans in inspector.
    /// </summary>
    [Serializable]
    public struct SerializedTimeSpan
    {
        
        [SerializeField] private long seconds;
        [SerializeField] private long minutes;
        [SerializeField] private long hours;
        [SerializeField] private long days;

        public long Days => days;
        public long Hours => hours;
        public long Minutes => minutes;
        public long Seconds => seconds;

        public SerializedTimeSpan(long hours, long minutes, long seconds)
        {
            this.days = 0;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
        }

        public SerializedTimeSpan(long days, long hours, long minutes, long seconds)
        {
            this.days = days;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
        }

        public static SerializedTimeSpan operator +(SerializedTimeSpan a, SerializedTimeSpan b) => new(a.days + b.days, a.hours + b.hours, a.minutes + b.minutes, a.seconds + b.seconds);
        public static SerializedTimeSpan operator -(SerializedTimeSpan a, SerializedTimeSpan b) => new(a.days - b.days, a.hours - b.hours, a.minutes - b.minutes, a.seconds - b.seconds);

        public static bool operator <(SerializedTimeSpan a, SerializedTimeSpan b) => a.ToSeconds() < b.ToSeconds();
        public static bool operator >(SerializedTimeSpan a, SerializedTimeSpan b) => a.ToSeconds() > b.ToSeconds();

        public static bool operator <=(SerializedTimeSpan a, SerializedTimeSpan b) => a.ToSeconds() <= b.ToSeconds();
        public static bool operator >=(SerializedTimeSpan a, SerializedTimeSpan b) => a.ToSeconds() >= b.ToSeconds();

        public long ToSeconds()
        {
            long daysToSeconds = days * 86400;
            long hoursToSeconds = hours * 3600;
            long minutesToSeconds = minutes * 60;
            return seconds + daysToSeconds + hoursToSeconds + minutesToSeconds;
        }

        public TimeSpan AsTimeSpan()
        {
            return new TimeSpan((int)Days, (int)Hours, (int)Minutes, (int)Seconds);
        }
        
    }
}