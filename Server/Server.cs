using System;

namespace Server
{
    class Server
    {
        
        public const string Prefix = "http://localhost:11069";

        static void Main(string[] args)
        {
            ServerLogger.LogMessage("Loading server prefixes");
            string[] prefixes = { Prefix + "/connectionCheck/", Prefix + "/diffieTunnel/", Prefix + "/register/", Prefix + "/logIn/", Prefix + "/sendChatMessage/", Prefix + "/contacts/", Prefix + "/history/" };

            ServerLogger.LogMessage("Initializing Server Core");
            WebServerCore WSC = new WebServerCore(prefixes);
            ServerLogger.LogMessage("Starting Server");
            WSC.Run();

            ServerLogger.LogMessage("Server Started");
            Console.ReadLine();

            WSC.Stop();
        }
    }
}
