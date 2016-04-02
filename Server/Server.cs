using System;
using Server.Classes.DbAccess;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CommunicatorCore.Classes.Model;
using System.Collections.Specialized;
using System.Text;

namespace Server
{
    class Server
    {
        static string GetIPv4Address()
        {
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress i in ips)
            {
                if (i.AddressFamily == AddressFamily.InterNetwork)
                {
                    return i.ToString();
                }
            }
            return "localhost";
        }

        public static string Prefix = string.Empty;

        static void Main(string[] args)
        {
            foreach (string s in args)
            {
                if (s == "-l")
                    Prefix = "http://localhost:11069";
            }
            if(Prefix == string.Empty)
                Prefix = "http://" + GetIPv4Address() + ":11069";
            
            ServerLogger.LogMessage("Loading server prefixes");
            string[] prefixes = { Prefix + "/connectionCheck/", Prefix + "/diffieTunnel/", Prefix + "/register/", Prefix + "/logIn/", Prefix + "/sendChatMessage/", Prefix + "/contacts/", Prefix + "/history/", Prefix + "/password/", Prefix + "/status/" };
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
                    if( x == "go" )
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
            Console.WriteLine("sending MSG from cipa@protonmail.com to rafixwpt@protonmail.com");
            try
            {
                string UID = Guid.NewGuid().ToString();
                string targetIP = "192.168.0.5";
                string target = "mail.rafixwpt@gmail.com";
                string sender = "cipa@protonmail.com";
                string content = "TakiTamRandom";
                Message msg = new Message(UID, sender, target, content);
                CryptoRSA cryptoService = new CryptoRSA();
                cryptoService.LoadRsaFromPublicKey("keys/mail.rafixwpt@gmail.com_Public.pem");
                string encryptedChatMessage = cryptoService.PublicEncrypt(msg.GetJsonString(), cryptoService.PublicRSA);
                ControlMessage message = new ControlMessage("SERVER", "CHAT_MESSAGE", encryptedChatMessage);

                string responseString = string.Empty;
                NameValueCollection headers = new NameValueCollection();
                NameValueCollection data = new NameValueCollection();

                using (var wb = new WebClient())
                {
                    wb.Proxy = null;
                    headers["messageContent"] = message.GetJsonString();
                    wb.Headers.Add(headers);
                    data["DateTime"] = DateTime.Now.ToShortDateString();
                    byte[] responseByte = wb.UploadValues("http://" + targetIP + ":11070/chatMessage/", "POST", data);
                    responseString = Encoding.UTF8.GetString(responseByte);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
