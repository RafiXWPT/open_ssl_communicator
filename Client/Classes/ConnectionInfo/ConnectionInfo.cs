using System;
using System.Collections.Generic;
using System.Linq;
using Config;

namespace Client
{
    public static class ConnectionInfo
    {
        public static string Sender { get; set; }
        public static int Port { get; }
        public static string Address { get; }
        public static bool Connected { get; set; }
        public static double Latency { get; set; }

        static List<double> ticks = new List<double>();

        static ConnectionInfo()
        {
            Sender = "UNKNOWN";
            Port = Convert.ToInt32(ConfigurationHandler.GetValueFromKey("PORT"));
            Address = ConfigurationHandler.GetValueFromKey("ADDRESS");
            Connected = false;
            Latency = 9999;
        }

        public static void UpdateConnection(double connectionTick, bool lostConnection = false)
        {
            if (ticks.Count > 10)
                ticks.RemoveAt(0);
            ticks.Add(connectionTick);
            Latency = ticks.Average();

            if(Latency > 1000 || lostConnection)
            {
                Connected = false;
            }
            else
            {
                Connected = true;
            }
        }
    }
}
