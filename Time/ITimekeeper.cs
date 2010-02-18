using System;

namespace Phm.Time
{
    public interface ITimekeeper
    {
        Action<Task, Exception> ExceptionHandler { get; set; }
        Action<Task, TaskResult> CompletedHandler { get; set; }
        void ScheduleFor(TimeSpan timeOfDay, Task task);
        void ScheduleForEveryDay(Task task);
        void ScheduleForEveryHour(Task task);
        void Start();
        void Stop();
    }
}


