using System.Diagnostics;

namespace Cake.MyGet.Module
{
    /// <summary>
    /// Extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Ensures a <see cref="Stopwatch"/> is started.
        /// </summary>
        /// <param name="stopwatch">The <see cref="Stopwatch"/>.</param>
        /// <returns>The Stopwatch.</returns>
        internal static Stopwatch EnsureStarted(this Stopwatch stopwatch)
        {
            if (stopwatch == null)
            {
                stopwatch = new Stopwatch();
            }

            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
            }

            stopwatch.Reset();
            stopwatch.Start();
            return stopwatch;
        }
    }
}
