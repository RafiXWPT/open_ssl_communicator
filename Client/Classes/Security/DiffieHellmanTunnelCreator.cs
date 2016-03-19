using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffieHellman;
using System.Net;
using SystemMessage;
using System.Windows.Controls;
using System.Windows;
using System.Threading;
using SystemSecurity;

namespace Client
{
    public class DiffieHellmanTunnelCreator
    {
        private readonly DiffieHellmanTunnel tunnel;
        private readonly Uri uriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" + ConfigurationHandler.GetValueFromKey("DIFFIE_API") + "/");

        public DiffieHellmanTunnelCreator()
        {
            tunnel = new DiffieHellmanTunnel();
        }

        public DiffieHellmanTunnel EstablishTunnel()
        {
            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                string reply = string.Empty;

                reply = GetTemporaryID(client);
                if (reply.Length > 0)
                {
                    ConnectionInfo.Sender = reply;
                }
                else
                {
                    tunnel.Status = DiffieHellmanTunnelStatus.FAILURE;
                }

                reply = ExchangePublicKeys(client);
                if(reply != null && reply != string.Empty)
                {
                    tunnel.CreateKey(reply);
                }
                else if(reply == string.Empty)
                {
                    tunnel.Status = DiffieHellmanTunnelStatus.FAILURE;
                }

                reply = ExchangeIV(client);
                if (reply != null && reply != "CHECK")
                {
                    tunnel.Status = DiffieHellmanTunnelStatus.FAILURE;
                }

                reply = CheckTunnel(client);
                if (reply != null && reply == "RDY")
                {
                    tunnel.Status = DiffieHellmanTunnelStatus.ESTABLISHED;
                }
                else if(reply == string.Empty || reply != "RDY")
                {
                    tunnel.Status = DiffieHellmanTunnelStatus.FAILURE;
                }
            }

            return tunnel;
        }

        string GetTemporaryID(WebClient client)
        {
            tunnel.Status = DiffieHellmanTunnelStatus.ASKING_FOR_ID;

            string reply = string.Empty;
            try
            {
                Message message = new Message("UNKNOWN", "REQUEST_FOR_ID", "NO_DESTINATION", "NO_CONTENT");
                reply = NetworkController.Instance.SendMessage(uriString, client, message);
            }
            catch
            {

            }

            return reply;
        }

        string ExchangePublicKeys(WebClient client)
        {
            if (tunnel.Status == DiffieHellmanTunnelStatus.FAILURE)
                return null;

            tunnel.Status = DiffieHellmanTunnelStatus.EXCHANGING_PUBLIC_KEYS;

            string reply = string.Empty;

            try
            {
                Message message = new Message(ConnectionInfo.Sender, "PUBLIC_KEY_EXCHANGE", "NO_DESTINTAION", tunnel.GetPublicPart());
                reply = NetworkController.Instance.SendMessage(uriString, client, message);
            }
            catch
            {

            }

            return reply;
        }

        string ExchangeIV(WebClient client)
        {
            if (tunnel.Status == DiffieHellmanTunnelStatus.FAILURE)
                return null;

            tunnel.Status = DiffieHellmanTunnelStatus.EXCHANGING_IV;

            string reply = string.Empty;
            try
            {
                Message message = new Message(ConnectionInfo.Sender, "IV_EXCHANGE", "NO_DESTINTAION", tunnel.GetIV());
                reply = NetworkController.Instance.SendMessage(uriString, client, message);
            }
            catch
            {

            }

            return reply;
        }

        string CheckTunnel(WebClient client)
        {
            if (tunnel.Status == DiffieHellmanTunnelStatus.FAILURE)
                return null;

            tunnel.Status = DiffieHellmanTunnelStatus.CHECKING_TUNNEL;

            string reply = string.Empty;
            try
            {
                Message message = new Message(ConnectionInfo.Sender, "CHECK_TUNNEL", "NO_DESTINTAION", tunnel.DiffieEncrypt("OK"));
                reply = NetworkController.Instance.SendMessage(uriString, client, message);
            }
            catch
            {

            }

            return reply;
        }
    }
}
