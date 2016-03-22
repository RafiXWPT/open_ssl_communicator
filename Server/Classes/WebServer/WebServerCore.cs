using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server.Classes;

namespace Server
{
    class WebServerCore
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly RequestHandler _handler = new RequestHandler();
        private string[] prefixes;

        private readonly UserControll controll = new UserControll();
        private readonly ContactControl contactControll = new ContactControl();
        private readonly MessageControl messageControll = new MessageControl();

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
                ServerLogger.LogMessage("Webserver Running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            HttpListenerContext ctx = c as HttpListenerContext;
                            _handler.HandleRequest(ctx);
                            
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