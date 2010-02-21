using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Phm.Time.Tests
{
    [TestFixture]
    public class TimekeeperTest
    {
        private Task _doNothingTask = new Task {Execute = dt => new TaskResult {Message = "Did Nothing"}, FriendlyName = "Do Nothing"};

        [Test]
        public void Should_Execute_Hourly_Task_Every_Hour_1_thru_23()
        {
            var tt = new Timekeeper();
            int numberTimesExecuted = 0;
            var incrementTimesExecuted = new Task
                                 {
                                     Execute = dt =>
                                                   {
                                                       numberTimesExecuted += 1;
                                                       return new TaskResult {Message = "Incremented"};
                                                   },
                                     FriendlyName = "Increment Times Executed"
                                 };
            tt.ScheduleForEveryHour(incrementTimesExecuted);
            TimekeeperClock.Now = () => new DateTime(2010, 2, 22, 0, 0, 1);
            tt.Start();
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            for (int i=1; i <24; i++)
            {
                int hour = i;
                TimekeeperClock.Now = () => new DateTime(2010,2,22,hour,0,1);
                System.Threading.Thread.Sleep(200);//let timekeeper tick
                Assert.AreEqual(i, numberTimesExecuted);
            }
            tt.Stop();
        }

        [Test]
        public void Should_Not_Execute_Hourly_Task_On_Hour_0()
        {
            var tt = new Timekeeper();
            int numberTimesExecuted = 0;
            var incrementTimesExecuted = new Task
            {
                Execute = dt =>
                {
                    numberTimesExecuted += 1;
                    return new TaskResult { Message = "Incremented" };
                },
                FriendlyName = "Increment Times Executed"
            };
            tt.ScheduleForEveryHour(incrementTimesExecuted);
            TimekeeperClock.Now = () => new DateTime(2010, 2, 21, 23, 59, 0);
            tt.Start();
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            TimekeeperClock.Now = () => new DateTime(2010, 2, 22, 0, 0, 1);
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            Assert.AreEqual(0,numberTimesExecuted);
            tt.Stop();
        }

        [Test]
        public void Should_Execute_Daily_Task_On_Hour_0()
        {
            var tt = new Timekeeper();
            int numberTimesExecuted = 0;
            var incrementTimesExecuted = new Task
            {
                Execute = dt =>
                {
                    numberTimesExecuted += 1;
                    return new TaskResult { Message = "Incremented" };
                },
                FriendlyName = "Increment Times Executed"
            };
            tt.ScheduleForEveryDay(incrementTimesExecuted);
            TimekeeperClock.Now = () => new DateTime(2010, 2, 21, 23, 59, 0);
            tt.Start();
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            TimekeeperClock.Now = () => new DateTime(2010, 2, 22, 0, 0, 1);
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            Assert.AreEqual(1, numberTimesExecuted);
            tt.Stop();
        }

        [Test]
        public void Should_Execute_Scheduled_Task_At_Scheduled_Time()
        {
            var tt = new Timekeeper();
            int numberTimesExecuted = 0;
            var incrementTimesExecuted = new Task
            {
                Execute = dt =>
                {
                    numberTimesExecuted += 1;
                    return new TaskResult { Message = "Incremented" };
                },
                FriendlyName = "Increment Times Executed"
            };
            tt.ScheduleFor(new TimeSpan(9,22,45), incrementTimesExecuted);
            TimekeeperClock.Now = () => new DateTime(2010, 2, 22, 9, 0, 22);
            tt.Start();
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            TimekeeperClock.Now = () => new DateTime(2010, 2, 22, 9, 23, 1);
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            Assert.AreEqual(1, numberTimesExecuted);
            tt.Stop();
        }

        [Test]
        public void Should_Execute_One_AM_Task_Twice_On_Jump_Back_DST()
        {
            var tt = new Timekeeper();
            int numberTimesExecuted = 0;
            var incrementTimesExecuted = new Task
            {
                Execute = dt =>
                {
                    numberTimesExecuted += 1;
                    return new TaskResult { Message = "Incremented" };
                },
                FriendlyName = "Increment Times Executed"
            };
            tt.ScheduleFor(new TimeSpan(1,0,0), incrementTimesExecuted);
            TimekeeperClock.Now = () => new DateTime(2010, 3, 14, 0, 59, 59);
            tt.Start();
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            TimekeeperClock.Now = () => new DateTime(2010, 3, 14, 1, 0, 01);
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            TimekeeperClock.Now = () => new DateTime(2010, 3, 14, 1, 59, 59);
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            TimekeeperClock.Now = () => new DateTime(2010, 3, 14, 1, 0,01);
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            Assert.AreEqual(2, numberTimesExecuted);
            tt.Stop();
        }

        [Test]
        public void Should_Execute_One_AM_And_3_AM_And_Not_2_AM_Task_On_Jump_Forward_DST()
        {
            var tt = new Timekeeper();
            List<int> hoursExecuted = new List<int>();
            var addHourToExecuted = new Task
                                             {
                                                 Execute = dt =>
                                                               {
                                                                   hoursExecuted.Add(dt.Hour);
                                                                   return new TaskResult();
                                                               }
                                             };

            tt.ScheduleForEveryHour(addHourToExecuted);
            TimekeeperClock.Now = () => new DateTime(2010, 3, 14, 0, 59, 59);
            tt.Start();
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            TimekeeperClock.Now = () => new DateTime(2010, 3, 14, 1, 0, 01);
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            TimekeeperClock.Now = () => new DateTime(2010, 3, 14, 1, 59, 59);
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            TimekeeperClock.Now = () => new DateTime(2010, 3, 14, 3, 0, 01);
            System.Threading.Thread.Sleep(200);//let timekeeper tick
            Assert.Contains(1, hoursExecuted);
            Assert.Contains(3, hoursExecuted);
            Assert.False(hoursExecuted.Contains(2));
            tt.Stop();
        }
    }
}


