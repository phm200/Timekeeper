using System;
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
    }
}


