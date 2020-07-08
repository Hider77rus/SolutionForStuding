using System;
using System.Threading;
using System.Timers;

namespace DataflowTPL
{
    public class TimerTests
    {
        System.Timers.Timer _timer = new System.Timers.Timer();

        public void Test()
        {
            _timer.Interval = 100;
            _timer.AutoReset = false;
            _timer.Enabled = true;
            _timer.Elapsed += DoSomething;
            _timer.Start();

            while (true)
            {
                var value = Console.ReadKey();
                if(value.KeyChar == 'r')
                    _timer.Start();
            }
        }

        private void DoSomething(object o, ElapsedEventArgs e)
        {
            Thread.Sleep(5000);
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                          e.SignalTime);
            Console.WriteLine($"Thread Id: {Thread.CurrentThread.ManagedThreadId}");
            _timer.Start();
        }
    }
}