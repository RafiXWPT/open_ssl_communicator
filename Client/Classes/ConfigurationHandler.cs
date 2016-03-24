using System.Configuration;

namespace Config
{
    public static class ConfigurationHandler
    {
        private static readonly Configuration config;

        static ConfigurationHandler()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public static string GetValueFromKey(string key)
        {
            try
            {
                if (HasValueOnKey(key))
                    return config.AppSettings.Settings[key].Value;
                else
                    return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void SetValueOnKey(string key, string value)
        {
            try
            {
                if (value != string.Empty)
                    config.AppSettings.Settings[key].Value = value;
                else
                    config.AppSettings.Settings[key].Value = "NULL";

                SaveConfiguration();
            }
            catch
            {
                return;
            }
        }

        public static bool HasValueOnKey(string key)
        {
            try
            {
                if (config.AppSettings.Settings[key].Value == "NULL" || config.AppSettings.Settings[key].Value == string.Empty)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SaveConfiguration()
        {
            config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
