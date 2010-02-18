using System;

namespace Phm.Time
{
    public class Task
    {
        public Func<DateTime, TaskResult> Execute { get; set; }
        public string FriendlyName { get; set; }
    }
}


