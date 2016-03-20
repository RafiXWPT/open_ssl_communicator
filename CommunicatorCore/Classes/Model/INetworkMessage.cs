using System;

namespace CommunicatorCore.Classes.Model
{
    public interface INetworkMessage
    {
        void LoadJson(string jsonString);

        string GetJsonString();
    }
}