using System;

namespace Phm.Time
{
    public interface ITimekeeper
    {
        event Action<ITask, Exception> ExceptionHandler;
        event Action<ITask, TaskResult> CompletedHandler;
        void ScheduleFor(TimeSpan timeOfDay, ITask task);
        void ScheduleForEveryDay(ITask task);
        void ScheduleForEveryHour(ITask task);
        void Start();
        void Stop();
    }
}


