using Cake.Core.Configuration;

namespace Cake.Module.Shared
{
    public static class Extensions
    {
        public static bool GetConfigFlag(this ICakeConfiguration config, string key) {
            bool fail;
            var configValue = config.GetValue(key);
            return  string.IsNullOrWhiteSpace(configValue)
                ? false
                : bool.TryParse(configValue, out fail)
                    ? fail
                    : false;
        }
    }
}