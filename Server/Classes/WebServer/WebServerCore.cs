using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class WebServerCore
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly MessageHandler _handler = new MessageHandler();
        private string[] prefixes;

        private readonly UserControll controll = new UserControll();

        public WebServerCore(string[] prefixes)
        {
            this.prefixes = prefixes;

            foreach (string prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }

            _listener.Start();
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            HttpListenerContext ctx = c as HttpListenerContext;
                            _handler.HandleMessage(ctx);
                            
                        }, _listener.GetContext());
                    }
                }
                catch { }
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}