using System;
using Server.Classes.WebServer;

namespace Server
{
    class Server
    {
        
        public const string Prefix = "http://localhost:11069";

        static void Main(string[] args)
        {
            string[] prefixes = { Prefix + "/connectionCheck/", Prefix + "/register/", Prefix + "/logIn/", Prefix + "/sendChatMessage/" };

            WebServerCore WSC = new WebServerCore(prefixes);
            WSC.Run();

            Console.WriteLine("A simple webserver. Press a key to quit.");
            Console.ReadKey();

            WSC.Stop();
        }
    }
}
