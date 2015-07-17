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
    public class CommandProcessor : IRUPCallbackHandler
    {
        PlateViewer plateViewer = null;
        TextBox txtInfo = null;
        public string MethodDistributer(System.Windows.Controls.UserControl ControlUI, string MethodName, string Parameter, Tecan.TouchTools.Interfaces.ITouchToolsServices services)
        {
            if (MethodName.Equals("ShowSettingForm"))
            {
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
                plateViewer = new PlateViewer(new Size(400, 300), new Size(50, 20));
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
            txtInfo.Text = txt;
            txtInfo.Foreground = isError ? Brushes.Red : Brushes.Black;
        }

        private void ClearInfo()
        {
            SetInfo("");
        }


        void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            ClearInfo();
            try
            {
                PCRSettings.Instance.CheckValid();
                //GeneratePipettingInfos();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message);
            }
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
            plateViewer.InvalidateVisual();
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
            var pcrType_ColorDict = PCRSettings.Instance.PCRType_Color;
            foreach(KeyValuePair<string,Color> pair in pcrType_ColorDict)
            {
                ListBoxItem lstItem = new ListBoxItem();
                int result;
                bool isDigital = int.TryParse(pair.Key, out result);
                string suffix = isDigital ? "μl" : "";
                string content = string.Format("{0}{1}",pair.Key,suffix);
                if (content.Length < 3)
                    content = " " + content;
                lstItem.Content = content;
                lstItem.Background = new SolidColorBrush(pair.Value);
                lstPCRType.Items.Add(lstItem);
            }
            lstPCRType.SelectedIndex = 0;
            GlobalVars.Instance.PCRType = ((ListBoxItem)lstPCRType.Items[0]).Content.ToString();
        }
    }
}
