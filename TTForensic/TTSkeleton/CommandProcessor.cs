using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tecan.TouchPad.Shared;
using Tecan.TouchTools.Interfaces;
using TTSkeleton.Utility;


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


#if DEBUG
                GlobalVars.Instance.SampleCount = 44;
                GlobalVars.Instance.SampleCountPerPlate = 24;
#endif
                var container = ControlUI.FindName("lstPCRType");
                if (container != null)
                {
                    ListBox lstPCRType = (ListBox)container;
                    InitialPCRTypes(lstPCRType);
                    lstPCRType.SelectionChanged += lstPCRVolume_SelectionChanged;
                }
             
                //container = ControlUI.FindName("lstPlateID");
                //ListBox lstPlateID = (ListBox)container;
                //InitialPlateIDs(lstPlateID);
                //lstPlateID.SelectionChanged += lstPlateID_SelectionChanged;
            }
            
            return string.Empty;
        }
         void lstPlateID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            int id = (int)e.AddedItems[0];
            GlobalVars.Instance.CurrentPlateID = id;
            GlobalVars.Instance.PlateViewer.InvalidateVisual();
        }

        void lstPCRVolume_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            ListBoxItem itm = (ListBoxItem)e.AddedItems[0];
            GlobalVars.Instance.PCRType = itm.Content.ToString();
        }
        private void InitialPCRTypes(ListBox lstPCRType)
        {
            var pcrType_Settings = PCRSettings.Instance.PCRType_Settings;
            foreach (KeyValuePair<string, ItemSetting> pair in pcrType_Settings)
            {
                ListBoxItem lstItem = new ListBoxItem();
                int result;
                bool isDigital = int.TryParse(pair.Key, out result);
                string suffix = isDigital ? "μl" : "";
                string content = string.Format("{0}{1}", pair.Key, suffix);
                if (content.Length < 3)
                    content = " " + content;
                lstItem.Content = content;
                lstItem.Background = new SolidColorBrush(pair.Value.color);
                lstPCRType.Items.Add(lstItem);
            }
            lstPCRType.SelectedIndex = 0;
            GlobalVars.Instance.PCRType = ((ListBoxItem)lstPCRType.Items[0]).Content.ToString();
        }

        private void InitialPlateIDs(ListBox lstPlateID)
        {
            int sampleCntPerPlate = GlobalVars.Instance.SampleCountPerPlate;
            int samplePlateCnt = (GlobalVars.Instance.SampleCount + sampleCntPerPlate - 1) / sampleCntPerPlate;
            List<int> plateIDs = new List<int>();
            for (int i = 0; i < samplePlateCnt; i++)
            {
                plateIDs.Add(i + 1);
            }
            lstPlateID.ItemsSource = plateIDs;

            //initial select index
            lstPlateID.SelectedIndex = 0;
            GlobalVars.Instance.CurrentPlateID = 1;
        }
    }
}
