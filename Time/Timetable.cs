using System;
using System.Collections.Generic;
using System.Linq;

namespace Phm.Time
{
    internal class Timetable
    {

        private readonly Dictionary<TimeSpan, List<ITask>> _tasksByTimeSlot;
        private bool _hourSlotsCreated = false;
        private bool _daySlotCreated = false;
        private readonly TimeSpan _dayChangeTimeOfDay = TimeSpan.FromHours(0);
        private readonly object _syncObject = new object();

        internal Timetable()
        {
            _tasksByTimeSlot = new Dictionary<TimeSpan, List<ITask>>();
        }



        //the time of day will get passed in
        internal void ScheduleForEveryHour(ITask task)
        {
            lock (_syncObject)
            {
                if (!_hourSlotsCreated) CreateHourSlots();
                ScheduleInEveryHourSlot(task);
            }
        }


        //the new day will get passed in
        internal void ScheduleForEveryDay(ITask task)
        {
            lock (_syncObject)
            {
                if (!_daySlotCreated) CreateDaySlot();
                ScheduleInDayChangeSlot(task);
            }
        }

        internal void ScheduleFor(TimeSpan timeOfDay, ITask task)
        {
            lock (_syncObject)
            {
                ScheduleInSlot(timeOfDay, task);
            }
        }



        internal TimetableEntry GetNextEntry(DateTime time)
        {
            DateTime currentTime = time;
            //find the closest time slot on the schedule
            DateTime? nextScheduledTime = FindNextScheduledTime(currentTime);
            if (nextScheduledTime.HasValue)
            {
                return new TimetableEntry
                           {
                               Tasks = _tasksByTimeSlot[nextScheduledTime.Value.TimeOfDay],
                               ScheduledTime = nextScheduledTime.Value
                           };
            }
            throw new InvalidOperationException("There is no next entry in the timetable! Was the timetable used without adding any tasks?");
        }

        internal TimetableEntry GetNextEntry()
        {
            return GetNextEntry(TimekeeperClock.Now());
        }

        private DateTime? FindNextScheduledTime(DateTime currentTime)
        {
            TimeSpan currentTimeOfDay = currentTime.TimeOfDay;
            DateTime? nextTime = null;
            //first see if there is a time of day bigger then the current time
            var timesYetToOccurToday = _tasksByTimeSlot.Keys.Where(k => k > currentTimeOfDay);
            if (timesYetToOccurToday.Count() > 0)
            {
                TimeSpan closestTimeToday = timesYetToOccurToday.Min();
                nextTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day,
                                        closestTimeToday.Hours, closestTimeToday.Minutes, closestTimeToday.Seconds);
            }
            if (!nextTime.HasValue)
            {
                var timesYetToOccurTomorrow = _tasksByTimeSlot.Keys.Where(k => k <= currentTimeOfDay);
                if (timesYetToOccurTomorrow.Count() > 0)
                {
                    TimeSpan closestTimeTomorrow = timesYetToOccurTomorrow.Min();
                    //bump out the time a day
                    nextTime = currentTime.AddDays(1);
                    nextTime = new DateTime(nextTime.Value.Year, nextTime.Value.Month, nextTime.Value.Day,
                                            closestTimeTomorrow.Hours, closestTimeTomorrow.Minutes, closestTimeTomorrow.Seconds);
                }
            }
            return nextTime;
        }

        private void CreateHourSlots()
        {
            for (int i = 1; i <= 23; i++)
            {
                _tasksByTimeSlot.Add(TimeSpan.FromHours(i), new List<ITask>());
            }
            _hourSlotsCreated = true;
        }

        private void ScheduleInEveryHourSlot(ITask task)
        {
            for (int i = 1; i <= 23; i++)
            {
                _tasksByTimeSlot[TimeSpan.FromHours(i)].Add(task);
            }
        }

        private void CreateDaySlot()
        {
            _tasksByTimeSlot.Add(_dayChangeTimeOfDay, new List<ITask>());
            _daySlotCreated = true;
        }

        private void ScheduleInDayChangeSlot(ITask task)
        {
            _tasksByTimeSlot[_dayChangeTimeOfDay].Add(task);
        }

        private void ScheduleInSlot(TimeSpan timeOfDay, ITask task)
        {
            if (!_tasksByTimeSlot.ContainsKey(timeOfDay))
            {
                _tasksByTimeSlot.Add(timeOfDay, new List<ITask>());
            }
            _tasksByTimeSlot[timeOfDay].Add(task);
        }



    }
}


