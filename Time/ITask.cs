using System;

namespace Phm.Time
{
    public interface ITask
    {
        Func<DateTime, TaskResult> Execute { get; set; }
        string FriendlyName { get; set; }
    }
}