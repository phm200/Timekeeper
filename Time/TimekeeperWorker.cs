using System;
using System.ComponentModel;
using System.Threading;
using log4net;

namespace Phm.Time
{
    internal class TimekeeperWorker : BackgroundWorker
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Timetable _timetable;
        private TimetableEntry _next;

        internal Action<Task, Exception> ExceptionHandler { get; set; }
        internal Action<Task, TaskResult> CompletedHandler { get; set; }

        internal TimekeeperWorker(Timetable timetable)
        {
            WorkerSupportsCancellation = true;
            _timetable = timetable;
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            _logger.Info("Worker started.");
            DateTime lastTime = TimekeeperClock.Now();
            DateTime timeNow;
            _next = _timetable.GetNextEntry(lastTime);
            _logger.DebugFormat("Next timetable entry is at {0} in {1}", _next.ScheduledTime, _next.TimeUntil);
           
            while (!CancellationPending)
            {
                Thread.Sleep(100);
                timeNow = TimekeeperClock.Now();
                if ( Math.Abs( (timeNow - lastTime).Ticks ) > TimeSpan.FromMinutes(1).Ticks)
                {
                    //if the time changes more then one minute (plus or minus)
                    //after a 1/10 of a second sleep, then
                    //we've had some kind of major clock adjustment (potentially daylight savings time)
                    //so recalculate what the next entry would be
                    //have to back up the time a bit, in case it went from 1:59:59am to 3:00:01am on dst change
                    _logger.InfoFormat("Clock drift detected from {0} to {1} in one cycle.", lastTime, timeNow);
                    DateTime lookBackTime = timeNow.Add(TimeSpan.FromMinutes(-1));
                    _logger.InfoFormat("Recalculating next entry based on a time of {0}", lookBackTime);
                    _next = _timetable.GetNextEntry(lookBackTime);
                }
                if (_next.PastTime)
                {
                    //time to execute
                    _logger.DebugFormat("{0} is after {1}, executing tasks for timetable entry.", TimekeeperClock.Now(), _next.ScheduledTime);
                    ExecuteTasksAsync(_next);
                    _next = _timetable.GetNextEntry(timeNow);
                    _logger.DebugFormat("Next timetable entry is at {0} in {1}", _next.ScheduledTime, _next.TimeUntil);
                }
                //reset last time
                lastTime = timeNow;
            }
            //cancellation pending, so exit out
            e.Cancel = true;
            _logger.Info("Cancellation pending, worker stopped.");
        }

        private void ExecuteTasksAsync(TimetableEntry entry)
        {
            foreach (var task in entry.Tasks)
            {
                //avoid the closure!
                var taskToExecute = task;
                _logger.DebugFormat("Queueing task {0} in thread pool.", taskToExecute.FriendlyName);
                ThreadPool.QueueUserWorkItem(state =>
                                                 {
                                                     try
                                                     {
                                                         var result = taskToExecute.Execute(entry.ScheduledTime);
                                                         var worker = (TimekeeperWorker)state;
                                                         worker.CompletedHandler(taskToExecute, result);
                                                     }
                                                     catch (Exception e)
                                                     {
                                                         //do not reference the scheduler instance to avoid closure
                                                         var worker = (TimekeeperWorker)state;
                                                         if (!worker.ReportException(taskToExecute, e)) throw;
                                                     }
                                                 }, this);
            }
        }

        private bool ReportException(Task task, Exception exception)
        {
            var handler = ExceptionHandler;
            if (handler != null)
            {
                handler(task, exception);
                return true;
            }

            return false;
        }

        private void ReportCompleted(Task task, TaskResult result)
        {
            var handler = CompletedHandler;
            if (handler != null)
            {
                handler(task, result);
            }
        }
        }
    }


