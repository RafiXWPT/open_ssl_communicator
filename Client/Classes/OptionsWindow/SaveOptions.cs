using Config;

namespace OptionsWindow
{
    public static class SaveOptions
    {
        public static void Save(bool inComingMessageSound, bool outComingMessageSound, bool blinkChatWindow)
        {
            ConfigurationHandler.SetValueOnKey("INCOMING_SOUND", inComingMessageSound.ToString());
            ConfigurationHandler.SetValueOnKey("OUTCOMING_SOUND", outComingMessageSound.ToString());
            ConfigurationHandler.SetValueOnKey("BLINK_CHAT", blinkChatWindow.ToString());
        }
    }
}
