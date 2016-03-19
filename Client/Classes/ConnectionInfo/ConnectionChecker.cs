using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using CommunicatorCore.Classes.Model;

namespace Client
{
    public class ConnectionChecker
    {
        public bool CheckConnection { get; set; }
        ManualResetEvent checkConnection_event;
        Thread checkConnectionThread;

        public ConnectionChecker()
        {
            CheckConnection = false;
        }

        public void StartCheckConnection()
        {
            checkConnection_event = new ManualResetEvent(false);
            checkConnectionThread = new Thread(CheckConnection_thread);
            checkConnectionThread.IsBackground = true;
            CheckConnection = true;
            checkConnectionThread.Start();
        }

        public void StopCheckConnection()
        {
            CheckConnection = false;
            checkConnection_event.Set();
        }

        void CheckConnection_thread()
        {
            CHECK_CONNECTION:
            try
            {
                while (CheckConnection)
                {
                    Message message = new Message(ConnectionInfo.Sender, "CHECK_CONNECTION", "NO_DESTINATION", "CONN_CHECK");
                    Stopwatch watch = new Stopwatch();

                    using (WebClient client = new WebClient())
                    {
                        client.Proxy = null;

                        Uri uriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" + ConfigurationHandler.GetValueFromKey("CHECK_CONNECTION_API") + "/");

                        watch.Restart();
                        string response = NetworkController.Instance.SendMessage(uriString, client, message);
                        watch.Stop();

                        if (response == "CONN_AVAIL")
                        {
                            ConnectionInfo.UpdateConnection(Convert.ToDouble(watch.ElapsedMilliseconds));
                        }
                        else
                        {
                            ConnectionInfo.UpdateConnection(9999, true);
                            CheckConnection = !checkConnection_event.WaitOne(TimeSpan.FromSeconds(10));
                        }
                    }

                    CheckConnection = !checkConnection_event.WaitOne(TimeSpan.FromSeconds(1));
                }
            }
            catch
            {
                ConnectionInfo.UpdateConnection(9999, true);
                CheckConnection = !checkConnection_event.WaitOne(TimeSpan.FromSeconds(10));
                goto CHECK_CONNECTION;
            } 
        }
    }
}
