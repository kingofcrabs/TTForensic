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
        private static void Write(string s)
        {
            s += "\r\n";
            File.AppendAllText(@"C:\TouchToolsData\log.txt",s);
        }

        public static void Info(string s)
        {
            Write(s);
        }

        public static void InfoFormat(string sFormat, string[] sContents)
        {
            string s = string.Format(sFormat, sContents);
            Write(s);
        }



        private static void SendCommand(string sContent)
        {
            //Console.WriteLine("Attempting to connect to pipe...");
            //try
            //{
            //    GlobalVars.Instance.PipeClient.Connect(1000);

            //}
            //catch
            //{
            //    Console.WriteLine("The Pipe server must be started in order to send data to it.");
            //    return;
            //}
            //Console.WriteLine("Connected to pipe.");

            //using (StreamWriter sw = new StreamWriter(GlobalVars.Instance.PipeClient))
            //{
            //    sw.Write(sContent);
            //}
        }
    }
}
