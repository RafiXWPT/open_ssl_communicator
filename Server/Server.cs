using System;

namespace Server
{
    class Server
    {
        
        public const string Prefix = "http://localhost:11069";

        static void Main(string[] args)
        {
            string[] prefixes = { Prefix + "/connectionCheck/", Prefix + "/diffieTunnel/", Prefix + "/register/", Prefix + "/logIn/", Prefix + "/sendChatMessage/", Prefix + "/contacts/", Prefix + "/history/" };

            WebServerCore WSC = new WebServerCore(prefixes);
            WSC.Run();

            Console.WriteLine("A simple webserver. Press a key to quit.");
            Console.ReadKey();

            WSC.Stop();
        }
    }
}
