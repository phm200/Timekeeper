using System;
using System.Collections.Generic;

namespace Phm.Time
{
    internal class TimetableEntry
    {
        private readonly TimeSpan _zero = TimeSpan.FromTicks(0);

        public DateTime ScheduledTime { get; set; }

        public TimeSpan TimeUntil
        {
            get { return ScheduledTime - TimekeeperClock.Now(); }
        }

        public bool PastTime
        {
            get { return TimeUntil < _zero; }
        }

        public List<Task> Tasks = new List<Task>();

        public TimetableEntry NextOccurance
        {
            get
            {
                return new TimetableEntry { Tasks = Tasks, ScheduledTime = ScheduledTime.AddDays(1) };
            }
        }

    }
}


