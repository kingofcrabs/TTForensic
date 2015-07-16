using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using TTForensic.Utility;

namespace TTForensic
{
    class GlobalVars
    {
        private static GlobalVars _instance;
        AppSettingsSection myDllConfigAppSettings;
        List<Color> sampleColors;
        public GlobalVars()
        {
            Configuration myDllConfig = ConfigurationManager.OpenExeConfiguration(this.GetType().Assembly.Location);
            myDllConfigAppSettings = (AppSettingsSection)myDllConfig.GetSection("appSettings");
            sampleColors = ReadSampleColorConfigs();
        }

        private List<Color> ReadSampleColorConfigs()
        {
            string sFile = Folders.GetDataFolder() + "sampleColors.txt";
            IEnumerable<string> strs = File.ReadAllLines(sFile);
            List<Color> colors = new List<Color>();
            foreach(string s in strs)
            {
                colors.Add(ParseColor(s));
            }
            return colors;
        }

        private Color ParseColor(string s)
        {
            string[] strs = s.Split(',');
            byte a = byte.Parse(strs[0]);
            byte r = byte.Parse(strs[1]);
            byte g = byte.Parse(strs[2]);
            byte b = byte.Parse(strs[3]);
            return Color.FromArgb(a, r, g, b);
        }

        public string this[string key]
        {
            get
            {
                return myDllConfigAppSettings.Settings[key].Value;
            }
        }

        public List<Color> SampleColors
        {
            get
            {
                return sampleColors;
            }
        }

        static public GlobalVars Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalVars();
                }
                return _instance;
            }
        }
   
    }
}
