using System;
using log4net;

namespace Phm.Time
{
    public class Timekeeper : ITimekeeper
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private TimekeeperWorker _worker;
        private readonly Timetable _timetable = new Timetable();
        private readonly TimeZoneInfo _localTimeZone;

        public Timekeeper()
        {
            _localTimeZone = TimeZoneInfo.Local;
        }

        public Timekeeper(string localTimeZone)
        {
            _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZone);
        }


        public void ScheduleForEveryHour(ITask task)
        {
            if (task == null) throw new ArgumentNullException("task");
            _timetable.ScheduleForEveryHour(task);
            _logger.DebugFormat("Scheduled {0} for every hour.", task.FriendlyName);
        }

        public void ScheduleForEveryDay(ITask task)
        {
            if (task == null) throw new ArgumentNullException("task");
            _timetable.ScheduleForEveryDay(task);
            _logger.DebugFormat("Scheduled {0} for every day.", task.FriendlyName);
        }

        public event Action<ITask, TaskResult> CompletedHandler;
        public event Action<ITask, Exception> ExceptionHandler;

        public void ScheduleFor(TimeSpan timeOfDay, ITask task)
        {
            _timetable.ScheduleFor(timeOfDay, task);
            _logger.DebugFormat("Scheduled {0} for {1}.", task.FriendlyName, timeOfDay);
        }


        public void Start()
        {
            //set the timekeeper's clock to tick in the local time zone
            _logger.InfoFormat("Timekeeper's internal clock set to tick in {0}", _localTimeZone);
            TimekeeperClock.TimeZone = _localTimeZone;
            _worker = new TimekeeperWorker(_timetable);
            _worker.ExceptionHandler += ExceptionHandler;
            _worker.CompletedHandler += CompletedHandler;
            _worker.RunWorkerAsync();
            _logger.Info("Timekeeper started.");
        }

        public void Stop()
        {
            _worker.CancelAsync();
            _worker.ExceptionHandler -= ExceptionHandler;
            _worker.CompletedHandler -= CompletedHandler;
            _logger.Info("Timekeeper stopped.");
        }


    }
}


