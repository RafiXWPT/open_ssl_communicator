using System;

namespace Server
{
    class Server
    {
        static void Main(string[] args)
        {
            string[] prefixes = { "http://localhost:11069/connectionCheck/", "http://localhost:11069/register/", "http://localhost:11069/logIn/", "http://localhost:11069/sendChatMessage/" };

            WebServerCore WSC = new WebServerCore(prefixes);
            WSC.Run();

            Console.WriteLine("A simple webserver. Press a key to quit.");
            Console.ReadKey();

            WSC.Stop();
        }
    }
}
