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
        private readonly Uri uriString = new Uri("http://" + ConnectionInfo.Address + ":" + ConnectionInfo.Port + "/" + ConfigurationHandler.getValueFromKey("DIFFIE_API") + "/");

        public DiffieHellmanTunnelCreator()
        {
            tunnel = new DiffieHellmanTunnel();
        }

        public DiffieHellmanTunnel establishTunnel()
        {
            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                string reply = string.Empty;

                reply = getTemporaryID(client);
                if (reply.Length > 0)
                {
                    ConnectionInfo.Sender = reply;
                }
                else
                {
                    tunnel.Status = DiffieHellmanTunnelStatus.FAILURE;
                }

                reply = exchangePublicKeys(client);
                if(reply != null && reply != string.Empty)
                {
                    tunnel.CreateKey(reply);
                }
                else if(reply == string.Empty)
                {
                    tunnel.Status = DiffieHellmanTunnelStatus.FAILURE;
                }

                reply = exchangeIV(client);
                if (reply != null && reply != "CHECK")
                {
                    tunnel.Status = DiffieHellmanTunnelStatus.FAILURE;
                }

                reply = checkTunnel(client);
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

        string getTemporaryID(WebClient client)
        {
            tunnel.Status = DiffieHellmanTunnelStatus.ASKING_FOR_ID;

            string reply = string.Empty;
            try
            {
                Message message = new Message("UNKNOWN", "REQUEST_FOR_ID", "NO_DESTINATION", "NO_CONTENT");
                reply = NetworkController.Instance.sendMessage(uriString, client, message);
            }
            catch
            {

            }

            return reply;
        }

        string exchangePublicKeys(WebClient client)
        {
            if (tunnel.Status == DiffieHellmanTunnelStatus.FAILURE)
                return null;

            tunnel.Status = DiffieHellmanTunnelStatus.EXCHANGING_PUBLIC_KEYS;

            string reply = string.Empty;

            try
            {
                Message message = new Message(ConnectionInfo.Sender, "PUBLIC_KEY_EXCHANGE", "NO_DESTINTAION", tunnel.getPublicPart());
                reply = NetworkController.Instance.sendMessage(uriString, client, message);
            }
            catch
            {

            }

            return reply;
        }

        string exchangeIV(WebClient client)
        {
            if (tunnel.Status == DiffieHellmanTunnelStatus.FAILURE)
                return null;

            tunnel.Status = DiffieHellmanTunnelStatus.EXCHANGING_IV;

            string reply = string.Empty;
            try
            {
                Message message = new Message(ConnectionInfo.Sender, "IV_EXCHANGE", "NO_DESTINTAION", tunnel.getIV());
                reply = NetworkController.Instance.sendMessage(uriString, client, message);
            }
            catch
            {

            }

            return reply;
        }

        string checkTunnel(WebClient client)
        {
            if (tunnel.Status == DiffieHellmanTunnelStatus.FAILURE)
                return null;

            tunnel.Status = DiffieHellmanTunnelStatus.CHECKING_TUNNEL;

            string reply = string.Empty;
            try
            {
                Message message = new Message(ConnectionInfo.Sender, "CHECK_TUNNEL", "NO_DESTINTAION", tunnel.diffieEncrypt("OK"));
                reply = NetworkController.Instance.sendMessage(uriString, client, message);
            }
            catch
            {

            }

            return reply;
        }
    }
}
