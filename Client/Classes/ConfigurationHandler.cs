using System.Configuration;

namespace Client
{
    public static class ConfigurationHandler
    {
        private static readonly Configuration config;

        static ConfigurationHandler()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public static string getValueFromKey(string key)
        {
            try
            {
                if (hasValueOnKey(key))
                    return config.AppSettings.Settings[key].Value;
                else
                    return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void setValueOnKey(string key, string value)
        {
            try
            {
                if (value != string.Empty)
                    config.AppSettings.Settings[key].Value = value;
                else
                    config.AppSettings.Settings[key].Value = "NULL";

                saveConfiguration();
            }
            catch
            {
                return;
            }
        }

        public static bool hasValueOnKey(string key)
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

        public static void saveConfiguration()
        {
            config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
