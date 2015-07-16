using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace TTForensic
{
    class GlobalVars
    {
        private static GlobalVars _instance;
        private int _sampleCnt = 16;
        private int _tipCount = 4;
        AppSettingsSection myDllConfigAppSettings;
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
