using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Client
{
    public static class ConnectionInfo
    {
        public static string Sender { get; set; }
        public static int Port { get; set; }
        public static bool isLogged { get; set; }
        public static string Address { get; set; }
        public static bool Connected { get; set; }
        public static double Latency { get; set; }

        static List<double> ticks;

        static ConnectionInfo()
        {
            ResetInfo();
        }

        public static void UpdateConnection(double connectionTick, bool lostConnection = false)
        {
            try
            {
                if (ticks.Count > 10)
                    ticks.RemoveAt(0);
                ticks.Add(connectionTick);
                Latency = ticks.Average();

                if (Latency > 1024 || lostConnection)
                {
                    Connected = false;
                }
                else
                {
                    Connected = true;
                }
            }
            catch
            {
                Latency = 0;
            }

            UpdateMainWindowConnectionImage();
        }

        public static void ResetInfo()
        {
            Sender = "UNKNOWN";
            Port = Convert.ToInt32(ConfigurationHandler.GetValueFromKey("PORT"));
            Address = ConfigurationHandler.GetValueFromKey("ADDRESS");
            Connected = false;
            isLogged = false;
            Latency = 9999;
            ticks = new List<double>();
        }

        static void UpdateMainWindowConnectionImage()
        {
             Application.Current.Dispatcher.Invoke(new Action(() => updateMainWindowConnectionImage()));
        }

        static void updateMainWindowConnectionImage()
        {
            if (MainWindow.Instance == null)
                return;

            string uri = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\Images\";
            if (!Connected)
            {
                uri += "redbtn.png";
                Latency = 0;
            }
            else if (Latency < 64)
            {
                uri += "greenbtn.png";
            }
            else if (Latency < 128)
            {
                uri += "lightgreenbtn.png";
            }
            else if (Latency < 256)
            {
                uri += "yellowbtn.png";
            }
            else if (Latency < 512)
            {
                uri += "darkorangebtn.png";
            }
            else if (Latency >= 512)
            {
                uri += "darkorangebtn.png";
            }

            BitmapImage image = new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));
            MainWindow.Instance.UpdateLatency(Math.Round(Latency,2), image);
        }
    }
}
