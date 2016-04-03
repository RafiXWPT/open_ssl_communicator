using System;
using Server.Classes.DbAccess;
using System.Net;
using System.Threading;
using CommunicatorCore.Classes.Model;
using System.Collections.Specialized;
using System.Text;

namespace Server
{
    class Server
    {
        public static string Prefix = string.Empty;

        static void Main(string[] args)
        {
            foreach (string s in args)
            {
                if (s.Contains("--address"))
                {
                    Prefix = "http://" + s.Split(':')[1] + ":11069";
                }
            }
            if(Prefix == string.Empty)
                Prefix = "http://localhost:11069";
            
            ServerLogger.LogMessage("Loading server prefixes");
            string[] prefixes = { Prefix + "/connectionCheck/", Prefix + "/diffieTunnel/", Prefix + "/register/", Prefix + "/logIn/", Prefix + "/sendChatMessage/", Prefix + "/contacts/", Prefix + "/messageHistory/", Prefix + "/password/", Prefix + "/status/" };
            foreach (string prefix in prefixes) { 
                Console.WriteLine(prefix);
            }

            try
            {
                ServerLogger.LogMessage("Initializing Server Core");
                WebServerCore WSC = new WebServerCore(prefixes);


                ServerLogger.LogMessage("Checking DB connection");
                if (!MongoDbAccess.IsServerAlive())
                {
                    ServerLogger.LogMessage("Database connection unavailable. Exiting...");
	                Console.ReadLine();
                    Environment.Exit(1);
                }
                ServerLogger.LogMessage("DB Connection acquired");

                ServerLogger.LogMessage("Starting Server on: " + Prefix);
                WSC.Run();

                ServerLogger.LogMessage("Server Started");
                while(true)
                {
                    string x = Console.ReadLine();
                    if( x == "test" )
                    {
                        Test();
                    }
                    else if( x == "q" )
                    {
                        break;
                    }
                }

                WSC.Stop();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }

        }

        static void Test()
        {
            Thread t = new Thread(TestT);
            t.Start();
        }

        static void TestT()
        {
            Console.WriteLine("Test Method, do here whatever you want.");

            try
            { 
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
