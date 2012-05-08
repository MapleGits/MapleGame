﻿using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Loki.Threading
{
    public class Delay
    {
        public static void Execute(double delay, ThreadStart action)
        {
            Delay.Execute((int)delay, action);
        }

        public static void Execute(int delay, ThreadStart action)
        {
            Timer t = new Timer(delay);

            t.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs e)
                {
                    t.Stop();
                    action();
                    t.Dispose();
                    t = null;
                });

            t.Start();
        }

        private Timer t;

        public Delay(int delay, ThreadStart action)
        {
            t = new Timer(delay);

            t.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs e)
            {
                if (t != null)
                {
                    t.Stop();
                    action();
                    t.Dispose();
                }

                t = null;
            });
        }

        public void Execute()
        {
            t.Start();
        }

        public void Cancel()
        {
            if (t != null)
            {
                t.Stop();
                t.Dispose();
            }

            t = null;
        }
    }
}
