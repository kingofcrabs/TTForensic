using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tecan.TouchPad.Shared;
using TTForensic.Utility;

namespace TTForensic
{
    public class CommandProcessor : IRUPCallbackHandler
    {
      
        public string MethodDistributer(System.Windows.Controls.UserControl ControlUI, string MethodName, string Parameter, Tecan.TouchTools.Interfaces.ITouchToolsServices services)
        {
            MessageBox.Show("Enter");
            PlateViewer plateViewer = null;
            TextBox txtInfo = null;
            if (MethodName.Equals("ShowSettingForm"))
            {
                MessageBox.Show("ShowSettingForm");
                return string.Empty;

#if DEBUG
                GlobalVars.Instance.SampleCount = 44;
                GlobalVars.Instance.SampleCountPerPlate = 24;
#endif
                var container = ControlUI.FindName("lstPCRType");
                ListBox lstPCRType = (ListBox)container;
                InitialPCRTypes(lstPCRType);
                lstPCRType.SelectionChanged += lstPCRVolume_SelectionChanged;
                
                container = ControlUI.FindName("lstPlateID");
                ListBox lstPlateID = (ListBox)container;
                InitialPlateIDs(lstPlateID);
                lstPlateID.SelectionChanged += lstPlateID_SelectionChanged;

                //grid
                container = ControlUI.FindName("plateViewer");
                Grid grid = (Grid)container;
                plateViewer = new PlateViewer(new Size(400, 360), new Size(50,40));
                grid.Children.Add(plateViewer);

                //button
                container = ControlUI.FindName("btnConfirm");
                Button btnConfirm = (Button)container;
                btnConfirm.Click += btnConfirm_Click;

                //textbox
                container = ControlUI.FindName("txtInfo");
                txtInfo = (TextBox)container;
                
            }
            return string.Empty;
        }

        void SetInfo(string txt, bool isError = true)
        {
            GlobalVars.Instance.TextInfo.Text = txt;
            GlobalVars.Instance.TextInfo.Foreground = isError ? Brushes.Red : Brushes.Black;
        }

        private void ClearInfo()
        {
            SetInfo("");
        }


        void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            ClearInfo();
#if DEBUG
#else
            try
#endif
            {
                PCRSettings.Instance.CheckValid();
                PipettingDispatcher pipettingDispatcher = new PipettingDispatcher();
                List<PipettingInfo> pipettingInfoSamples = new List<PipettingInfo>();
                List<PipettingInfo> pipettingInfoPCRs = new List<PipettingInfo>();
                pipettingDispatcher.PreparePipettingList(ref pipettingInfoSamples, ref pipettingInfoPCRs);
#if DEBUG
                Write2File(pipettingInfoSamples,"samples");
                Write2File(pipettingInfoPCRs, "PCRs");
#endif
                Worklist wklist = new Worklist();
                wklist.GenerateSampleScripts(pipettingInfoSamples);
                wklist.GeneratePCRScripts(pipettingInfoPCRs);
            }
#if DEBUG
#else

            catch(Exception ex)
            {
                SetInfo(ex.Message);
            }
#endif
            SetInfo("成功生产worklist!",false);
        }

        private void Write2File(List<PipettingInfo> pipettingInfoSamples, string fileName)
        {
            string sFile = Folders.GetOutputFolder() + fileName + ".txt";   
            List<string> strs = new List<string>();
            pipettingInfoSamples.ForEach(x => strs.Add(x.ToString()));
            File.WriteAllLines(sFile, strs.ToArray());
        }

        private void InitialPlateIDs(ListBox lstPlateID)
        {
            int sampleCntPerPlate = GlobalVars.Instance.SampleCountPerPlate;
            int samplePlateCnt = (GlobalVars.Instance.SampleCount + sampleCntPerPlate -1) / sampleCntPerPlate;
            List<int> plateIDs = new List<int>();
            for(int i = 0; i< samplePlateCnt; i++)
            {
                plateIDs.Add(i + 1);
            }
            lstPlateID.ItemsSource = plateIDs;

            //initial select index
            lstPlateID.SelectedIndex = 0;
            GlobalVars.Instance.CurrentPlateID = 1;
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
                string content = string.Format("{0}{1}",pair.Key,suffix);
                if (content.Length < 3)
                    content = " " + content;
                lstItem.Content = content;
                lstItem.Background = new SolidColorBrush(pair.Value.color);
                lstPCRType.Items.Add(lstItem);
            }
            lstPCRType.SelectedIndex = 0;
            GlobalVars.Instance.PCRType = ((ListBoxItem)lstPCRType.Items[0]).Content.ToString();
        }
    }
}
