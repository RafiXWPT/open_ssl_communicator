using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Controls;
using System.Windows;
using System.Threading;
using SystemSecurity;
using CommunicatorCore.Classes.Model;
using Config;

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

                reply = GetTemporaryId(client);
                if (reply.Length > 0)
                {
                    ConnectionInfo.Sender = reply;
                }
                else
                {
                    tunnel.Status = DiffieHellmanTunnelStatus.FAILURE;
                }

                reply = ExchangePublicKeys(client);
                if( !string.IsNullOrEmpty(reply) )
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

        public bool isTunnelActive(DiffieHellmanTunnel checkingTunnel)
        {
            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                string reply = string.Empty;

                reply = CheckTunnel(client, checkingTunnel);
                if (reply != null && reply == "RDY")
                {
                    return true;
                }
                else if (reply == string.Empty || reply != "RDY")
                {
                    return false;
                }
            }

            return false;
        }

        string GetTemporaryId(WebClient client)
        {
            tunnel.Status = DiffieHellmanTunnelStatus.ASKING_FOR_ID;

            string reply = string.Empty;
            try
            {
                ControlMessage message = new ControlMessage("UNKNOWN", "REQUEST_FOR_ID");
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
                ControlMessage message = new ControlMessage(ConnectionInfo.Sender, "PUBLIC_KEY_EXCHANGE", tunnel.GetPublicPart());
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
                ControlMessage message = new ControlMessage(ConnectionInfo.Sender, "IV_EXCHANGE", tunnel.GetIV());
                reply = NetworkController.Instance.SendMessage(uriString, client, message);
            }
            catch
            {

            }

            return reply;
        }

        string CheckTunnel(WebClient client, DiffieHellmanTunnel checkingTunnel = null)
        {
            DiffieHellmanTunnel tunnelToCheck;
            if (checkingTunnel != null)
                tunnelToCheck = checkingTunnel;
            else
                tunnelToCheck = tunnel;

            if (tunnelToCheck.Status == DiffieHellmanTunnelStatus.FAILURE)
                return null;

            tunnelToCheck.Status = DiffieHellmanTunnelStatus.CHECKING_TUNNEL;

            string reply = string.Empty;
            ControlMessage message = new ControlMessage(ConnectionInfo.Sender, "CHECK_TUNNEL", "OK", tunnelToCheck.DiffieEncrypt("OK"));
            reply = NetworkController.Instance.SendMessage(uriString, client, message);
            
            return reply;
        }
    }
}
