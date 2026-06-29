using System;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GwCopyPro.Services
{
    /// <summary>
    /// Plays non-blocking PC-speaker beep sequences to provide audio feedback for job events.
    /// Each method fires a <see cref="Task.Run"/> so the UI thread is never blocked.
    /// </summary>
    public static class SoundService
    {
        [DllImport("kernel32.dll")]
        private static extern bool Beep(uint dwFreq, uint dwDuration);

        /// <summary>
        /// Plays three ascending beeps to signal that a job completed successfully.
        /// </summary>
        public static void PlaySuccess()
        {
            Task.Run(() =>
            {
                try
                {
                    Beep(880, 120);
                    System.Threading.Thread.Sleep(60);
                    Beep(1100, 120);
                    System.Threading.Thread.Sleep(60);
                    Beep(1320, 200);
                }
                catch { }
            });
        }

        /// <summary>
        /// Plays three descending beeps to signal a job error.
        /// </summary>
        public static void PlayError()
        {
            Task.Run(() =>
            {
                try
                {
                    Beep(400, 200);
                    System.Threading.Thread.Sleep(80);
                    Beep(300, 200);
                    System.Threading.Thread.Sleep(80);
                    Beep(200, 400);
                }
                catch { }
            });
        }

        /// <summary>
        /// Plays two medium-pitched beeps to signal a warning condition.
        /// </summary>
        public static void PlayWarning()
        {
            Task.Run(() =>
            {
                try
                {
                    Beep(600, 150);
                    System.Threading.Thread.Sleep(60);
                    Beep(600, 150);
                }
                catch { }
            });
        }

        /// <summary>
        /// Plays two ascending beeps to signal that a job has started.
        /// </summary>
        public static void PlayStart()
        {
            Task.Run(() =>
            {
                try
                {
                    Beep(660, 100);
                    System.Threading.Thread.Sleep(40);
                    Beep(880, 100);
                }
                catch { }
            });
        }
    }
}
