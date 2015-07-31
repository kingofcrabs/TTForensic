using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace TTSkeleton.Utility
{
    class GlobalVars
    {
        private static GlobalVars _instance;
        AppSettingsSection myDllConfigAppSettings;


        public string PCRType { get; set; }
        public int CurrentPlateID { get; set; }
        public int SampleCount { get; set; }
        public int SampleCountPerPlate { get; set; }
        public NamedPipeClientStream PipeClient { get; set; }
        public GlobalVars()
        {
            Configuration myDllConfig = ConfigurationManager.OpenExeConfiguration(this.GetType().Assembly.Location);
            myDllConfigAppSettings = (AppSettingsSection)myDllConfig.GetSection("appSettings");
            PipeClient = new NamedPipeClientStream(".", "TT", PipeDirection.Out, PipeOptions.None);
        }
        
        public string this[string key]
        {
            get
            {
                return myDllConfigAppSettings.Settings[key].Value;
            }
        }


        public PlateViewer PlateViewer { get; set; }
        public TextBox TextInfo { get; set; }

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
