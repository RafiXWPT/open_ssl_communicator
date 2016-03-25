using Config;

namespace OptionsWindow
{
    public static class LoadOptions
    { 
        public static bool IsIncomingMessageSoundEnabled()
        {
            return IsPropertyEnabled("INCOMING_SOUND");
        }

        public static bool IsOutcomingMessageSoundEnabled()
        {
            return IsPropertyEnabled("OUTCOMING_SOUND");
        }

        public static bool IsBlinkChatEnabled()
        {
            return IsPropertyEnabled("BLINK_CHAT");
        }

        private static bool IsPropertyEnabled(string propertyName)
        {
            return ConfigurationHandler.GetValueFromKey(propertyName) == "True";
        }
    }
}
