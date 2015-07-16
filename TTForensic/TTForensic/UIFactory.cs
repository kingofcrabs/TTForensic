using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Tecan.TouchPad.Shared;

namespace TTForensic
{
    public class UIFactory : IRUPCallbackHandler
    {
        public string MethodDistributer(System.Windows.Controls.UserControl ControlUI, string MethodName, string Parameter, Tecan.TouchTools.Interfaces.ITouchToolsServices services)
        {
            if (MethodName.Equals("ShowSettingForm"))
            {
                var grid = ControlUI.FindName("papaGrid");
                string find = grid == null ? "not found" : "found";
                MessageBox.Show(find);
            }
            return string.Empty;
        }
    }
}
