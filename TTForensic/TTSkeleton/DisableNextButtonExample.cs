using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tecan.TouchPad.Shared;
using Tecan.TouchTools.Interfaces;


namespace TTSkeleton
{
    

    /// <summary>
    /// The "DisableNextButtonExample" class implements the "IRUPCallbackHandler" interface to allow interaction between the xaml-file and the touch-tools-suite.
    /// </summary>
    public class DisableNextButtonExample : IRUPCallbackHandler
    {        

        /// <summary>
        /// The MethodDistributer is a reuired Method from the Interface.        
        /// </summary>
        /// <param name="ControlUI">The control UI.</param>
        /// <param name="MethodName">Name of the method.</param>
        /// <param name="Parameter">The parameter.</param>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public string MethodDistributer(System.Windows.Controls.UserControl ControlUI, string MethodName, string Parameter, Tecan.TouchTools.Interfaces.ITouchToolsServices services)
        {
            if (MethodName.Equals("CallDisableEnableNextButtonMethod"))
            {
                //Log.Info(MethodName);
                var canvas = ControlUI.FindName("canvas");
                if (canvas != null)
                {
                    Grid grid = (Grid)canvas;
                    PlateViewer plateViewer = new PlateViewer(new Size(480,360), new Size(20, 20));
                    grid.Children.Add(plateViewer);
                    Log.Info(MethodName);
                }
            }
            
            return string.Empty;
        }
    }
}
