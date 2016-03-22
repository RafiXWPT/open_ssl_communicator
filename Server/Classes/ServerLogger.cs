using System;
using System.IO;

namespace Server
{
    public static class ServerLogger
    {
        static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            if (!file.Exists)
                return false;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return false;
        }

        public static void LogMessage(string msg, bool onlyFile = false)
        {
            Directory.CreateDirectory("logs/" + DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString());

            DateTime date = DateTime.Now;
            string year = date.Year.ToString();
            string month = date.Month.ToString();
            string day = date.Day.ToString();

            try
            {
                string logFile = "logs/" + year + "/" + month + "/" + day + ".txt";
                if(!IsFileLocked(new FileInfo(logFile)))
                {
                    StreamWriter sw = new StreamWriter(logFile, true);
                    sw.WriteLine("[ " + DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss") + " ] >> " + msg);
                    sw.Close();

                    if (!onlyFile)
                    {
                        Console.WriteLine("[ " + DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss") + " ] >> " + msg);
                    }
                }
                else
                {
                    LogMessage(msg, onlyFile);
                }
            }
            catch
            {
                LogMessage(msg, onlyFile);
            }
        }
    }
}
