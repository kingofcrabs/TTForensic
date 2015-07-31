using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using TTSkeleton.Utility;

namespace TTSkeleton
{
    class Log
    {
        public static void Info(string s)
        {
            SendCommand(s);
        }

        public static void InfoFormat(string sFormat, string[] sContents)
        {
            string s = string.Format(sFormat, sContents);
            SendCommand(s);
        }



        private static void SendCommand(string sContent)
        {
            Console.WriteLine("Attempting to connect to pipe...");
            try
            {
                GlobalVars.Instance.PipeClient.Connect(1000);

            }
            catch
            {
                Console.WriteLine("The Pipe server must be started in order to send data to it.");
                return;
            }
            Console.WriteLine("Connected to pipe.");

            using (StreamWriter sw = new StreamWriter(GlobalVars.Instance.PipeClient))
            {
                sw.Write(sContent);
            }
        }
    }
}
