using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTForensic.Utility
{
    class Common
    {
        static int _col = 12;
        static int _row = 8;
        static public int GetWellID(int x, int y)
        {
            return x * _row + y + 1;
        }
        static public string GetWellDescription(int wellID)
        {
            int x, y;
            Convert(wellID, out x, out y);
            return string.Format("{0}{1}", (char)(y + 'A'), (x + 1));
        }

        public static void Convert(int wellID, out int col, out int row)
        {
            col = (wellID - 1) / _row;
            row = wellID - col * _row - 1;
        }
        
        public static string RemoveUL(string s)
        {
            return s.Replace("μl", "");
        }
        
        public static string GetDigitalStr(string s)
        {
            string sDigit = "";
            foreach (char ch in s)
            {
                if (char.IsDigit(ch))
                {
                    sDigit += ch;
                }
            }
            return sDigit;
        }
    }
}
