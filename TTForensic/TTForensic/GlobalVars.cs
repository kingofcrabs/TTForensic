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
    
  
        public string PCRType { get; set; }
        public int CurrentPlateID { get; set; }
        public int SampleCount { get; set; }
        public int SampleCountPerPlate { get; set; }
        public GlobalVars()
        {
            Configuration myDllConfig = ConfigurationManager.OpenExeConfiguration(this.GetType().Assembly.Location);
            myDllConfigAppSettings = (AppSettingsSection)myDllConfig.GetSection("appSettings");

        }

      

        public string this[string key]
        {
            get
            {
                return myDllConfigAppSettings.Settings[key].Value;
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
