using System;
using System.Globalization;

using Cake.Core.Configuration;

namespace Cake.Module.Shared
{
    /// <summary>
    /// Extensions.
    /// </summary>
    public static class Extensions
    {
        private static readonly string SubSecondDuration = "< 1sec";

        /// <summary>
        /// Renders a <see cref="TimeSpan"/> the way a person reads a stopwatch, as <c>m:ss</c>, or as
        /// <c>h:mm:ss</c> from an hour upwards. Anything below a second becomes <c>&lt; 1sec</c>.
        /// </summary>
        /// <param name="time">The <see cref="TimeSpan"/> to render.</param>
        /// <returns>The humanized duration.</returns>
        public static string Humanize(this TimeSpan time)
        {
            // A tenth of a second means nothing in a build summary, but the difference between "it ran
            // instantly" and "it took a second" does, so anything below a second is called out as such.
            if (time < TimeSpan.FromSeconds(1))
            {
                return SubSecondDuration;
            }

            var rounded = TimeSpan.FromSeconds(Math.Round(time.TotalSeconds, MidpointRounding.AwayFromZero));

            return rounded < TimeSpan.FromHours(1)
                ? string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}", (long)rounded.TotalMinutes, rounded.Seconds)
                : string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}:{2:00}", (long)rounded.TotalHours, rounded.Minutes, rounded.Seconds);
        }

        /// <summary>
        /// Get a config-value as a flag from the <see cref="ICakeConfiguration"/>.
        /// </summary>
        /// <param name="config">The <see cref="ICakeConfiguration"/>.</param>
        /// <param name="key">The config key to get.</param>
        /// <returns><c>true</c>, if the config key exists and equals the text <c>"True"</c>. Otherwise, <c>false</c>.</returns>
        public static bool GetConfigFlag(this ICakeConfiguration config, string key)
        {
            string configValue = config.GetValue(key);
            return string.IsNullOrWhiteSpace(configValue)
                ? false
                : bool.TryParse(configValue, out bool fail)
                    ? fail
                    : false;
        }
    }
}
