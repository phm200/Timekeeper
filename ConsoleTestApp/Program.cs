using System;
using log4net;

namespace Phm.Time.ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            var tk = new Timekeeper("Central Standard Time");
            var selfSatisfiedTask = new Task
                                     {
                                         Execute = dt =>
                                                       {
                                                           var msg = string.Format("Executed at {0}!", dt);
                                                           System.Console.WriteLine("Executed at {0}!", dt);
                                                           return new TaskResult { Message = msg };
                                                       },
                                         FriendlyName = "Self Satisfied Task"
                                     };
            var exceptionThrowingTask = new Task
                                    {
                                        Execute = dt =>
                                                      {
                                                          var msg = string.Format("About to throw an exception at {0}!", dt);
                                                          int i = 0;
                                                          int never = 5/i;
                                                          return new TaskResult { Message = msg };
                                                      },
                                        FriendlyName = "Exception Throwing Task"
                                    };
            tk.ScheduleForEveryHour(selfSatisfiedTask);
            tk.ScheduleForEveryHour(exceptionThrowingTask);
            tk.ExceptionHandler += (task, ex) => System.Console.WriteLine("[Exception Handler] Exception for {0}: {1}", task.FriendlyName, ex);
            tk.CompletedHandler += (task, result) => System.Console.WriteLine("[Completed Handler] {0} completed with a message of {1}", task.FriendlyName,result.Message);
            TimekeeperClock.Now = () => new DateTime(2010,2,17,2,59,59);
            tk.Start();
            System.Console.ReadLine();
            TimekeeperClock.Now = () => new DateTime(2010,2,17,3,0,1);
            System.Console.WriteLine("Press enter to exit.");
            System.Console.ReadLine();
            
        }
    }
}
