using Cake.Core.Configuration;

namespace Cake.Module.Shared
{
    /// <summary>
    /// Extensions.
    /// </summary>
    public static class Extensions
    {
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
