namespace CommunicatorCore.Classes.Model
{
    public static class ContentProvider
    {
        public static string RegistrationEmailContent
        {
            get
            {
                return "Welcome {0}!\n\n" +
                       "Your registration was completed successfully. After login into our application use: \"Menu>File>Import Keys\" to make your account fully working.\n" +
                       "Your unique token for contacts and history access: {1} (do not lose it, there is no way to recover it).\n" +
                       "Your public and private keys are in attachments, keep them safe.\n\n" +
                       "Crypto Talk Team\nMateusz Flis & Rafał Palej";
            }
        }

        public static string ResetPasswordEmailContent
        {
            get
            {
                return "Welcome {0}!\n\n" +
                       "Your password has been restarded. New password can be used to login into accound.\n" +
                       "New password: {1}\n\n" +
                       "Crypto Talk Team\nMateusz Flis & Rafał Palej";
            }
        }

    }
}
