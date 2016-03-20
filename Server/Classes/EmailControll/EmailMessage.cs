using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class EmailMessage
    {
        private readonly MailAddress from = new MailAddress("opensslcommunicator@gmail.com", "OpenSSL Communicator");
        private readonly string fromPassword = "raf109aello";

        private readonly string destination;
        private readonly string message;
        private readonly string subject;

        public EmailMessage(string subject, string message, string destination)
        {
            this.subject = subject;
            this.message = message;
            this.destination = destination;
        }

        public EmailMessage(string subject, string[] keys, string destination)
        {
            this.subject = subject;

            string fullMessage = "Thank you for registering. Below are your keys:\n" +
                                 "Private Key:\n" + keys[0] + "\n" +
                                 "Public Key:\n" + keys[1] + "\n\n\n" +
                                 "OpenSSL Communicator.";
            this.message = fullMessage;
            this.destination = destination;
        }

        public void Send()
        {
            try
            {
                MailAddress to = new MailAddress(destination);
                SmtpClient smtp = new SmtpClient();

                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(from.Address, fromPassword);

                MailMessage messageToSend = new MailMessage(from, to);
                messageToSend.Subject = subject;
                messageToSend.Body = message;

                smtp.Send(messageToSend);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
