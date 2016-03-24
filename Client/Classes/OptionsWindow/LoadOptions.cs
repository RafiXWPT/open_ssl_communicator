using Config;

namespace OptionsWindow
{
    public static class LoadOptions
    { 
        public static bool IsIncomingMessageSoundEnabled()
        {
            if(ConfigurationHandler.GetValueFromKey("INCOMING_SOUND") == "True")
                return true;
            else
                return false;
        }

        public static bool IsOutcomingMessageSoundEnabled()
        {
            if (ConfigurationHandler.GetValueFromKey("OUTCOMING_SOUND") == "True")
                return true;
            else
                return false;
        }

        public static bool IsBlinkChatEnabled()
        {
            if (ConfigurationHandler.GetValueFromKey("BLINK_CHAT") == "True")
                return true;
            else
                return false;
        }
    }
}
