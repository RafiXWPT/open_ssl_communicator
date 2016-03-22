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

        public EmailMessage(string subject, string destination)
        {
            this.subject = subject;

            string fullMessage = "Thank you for registering. Your keys was added to attachments:\n\n" +
                                 "Crypto Talk Team.";
            this.message = fullMessage;
            this.destination = destination;
        }

        public void Send(bool withAttachments = false)
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

                if(withAttachments)
                {
                    Attachment attachment;

                    attachment = new Attachment("keys/"+destination+"_Private.pem");
                    attachment.Name = destination + "_Private.pem";
                    messageToSend.Attachments.Add(attachment);

                    attachment = new Attachment("keys/" + destination + "_Public.pem");
                    attachment.Name = destination + "_Public.pem";
                    messageToSend.Attachments.Add(attachment);
                }

                smtp.Send(messageToSend);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
