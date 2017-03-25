using System;
using System.Diagnostics;

namespace Cake.MyGet.Module
{
    public static class Extensions
    {
        internal static Stopwatch EnsureStarted(this Stopwatch stopwatch) {
            if (stopwatch == null) {
                stopwatch = new Stopwatch();
            }
            if (stopwatch.IsRunning) {
                stopwatch.Stop();
            }
            stopwatch.Reset();
            stopwatch.Start();
            return stopwatch;
        }
    }
}