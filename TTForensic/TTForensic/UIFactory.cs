using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tecan.TouchPad.Shared;

namespace TTForensic
{
    public class UIFactory : IRUPCallbackHandler
    {
        public string MethodDistributer(System.Windows.Controls.UserControl ControlUI, string MethodName, string Parameter, Tecan.TouchTools.Interfaces.ITouchToolsServices services)
        {
            if (MethodName.Equals("ShowSettingForm"))
            {
                var container = ControlUI.FindName("lstPCRVolume");
                ListBox lstPCRVolume = (ListBox)container;
                InitialPCRVolumes(lstPCRVolume);
            }
            return string.Empty;
        }

        private void InitialPCRVolumes(ListBox lstPCRVolume)
        {
            for(int i = 0; i< 4; i++)
            {
                ListBoxItem lstItem = new ListBoxItem();
                lstItem.Content = string.Format("{0}μl",i+1);
                var color = GlobalVars.Instance.SampleColors[i];
                lstItem.Background = new SolidColorBrush(color);
                lstPCRVolume.Items.Add(lstItem);
            }

            ListBoxItem positiveItm = new ListBoxItem();
            positiveItm.Content = " +";
            positiveItm.Background = Brushes.LightBlue;
            lstPCRVolume.Items.Add(positiveItm);
        }
    }
}
