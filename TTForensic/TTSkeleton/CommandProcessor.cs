using System;
using System.Collections.Generic;
using System.IO;
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
    public class SampleSettingControl : IRUPCallbackHandler
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
                Log.Info("find txtInfo");

                //first, we prepare the txtInfo
                var container = ControlUI.FindName("txtInfo");
                GlobalVars.Instance.TextInfo = (TextBox)container;

                try
                {
                    GlobalVars.Instance.SampleCount = ReadSampleCount(ControlUI);
                    GlobalVars.Instance.SampleCountPerPlate = int.Parse(GlobalVars.Instance["sampleCountPerPlate"]);
                }
                catch(Exception ex)
                {
                    SetInfo(ex.Message);
                    return "";
                }
                
                var canvas = ControlUI.FindName("canvas");
                if (canvas != null)
                {
                    Grid grid = (Grid)canvas;
                    grid.Children.Clear();
                    Log.Info("CallDisableEnableNextButtonMethod & canvas found!");
                    PlateViewer plateViewer = new PlateViewer(new Size(480,360), new Size(20, 20));
                    GlobalVars.Instance.PlateViewer = plateViewer;
                    grid.Children.Add(plateViewer);
                    Log.Info("canvas not null");
                }

                Log.Info("set inputGrid visibility");
                container = ControlUI.FindName("inputGrid");
                if(container != null)
                {
                    Grid inputGrid = (Grid)container;
                    inputGrid.Visibility = Visibility.Visible;
                }

                container = ControlUI.FindName("lstPCRType");
                if (container != null)
                {
                    ListBox lstPCRType = (ListBox)container;
                    InitialPCRTypes(lstPCRType);
                    lstPCRType.SelectionChanged += lstPCRVolume_SelectionChanged;
                }

                container = ControlUI.FindName("lstPlateID");
                ListBox lstPlateID = (ListBox)container;
                InitialPlateIDs(lstPlateID);
                lstPlateID.SelectionChanged += lstPlateID_SelectionChanged;

                //button
                container = ControlUI.FindName("btnConfirm");
                Button btnConfirm = (Button)container;
                btnConfirm.Click += btnConfirm_Click;
            }
            //here we disable next button

            GlobalVars.Instance.Service = services;
            EnableNextButton(false);
            return string.Empty;
        }

        private void EnableNextButton(bool bEnable)
        {
            if(GlobalVars.Instance.Service != null)
                GlobalVars.Instance.Service.TouchToolsGuiServices.GetButtonService(Tecan.TouchTools.Interfaces.RUP.TouchToolsButtons.Button4).IsEnabled = bEnable;
        }

        private int ReadSampleCount(UserControl ControlUI)
        {
            var container = ControlUI.FindName("txtSampleCnt");
            TextBox txtSampleCnt = (TextBox)container;
            if (txtSampleCnt == null)
                throw new Exception("无法找到样本数textbox！");
            if (txtSampleCnt.Text == "")
            {
                throw new Exception("请先设置样本数！");
            }
            int val = int.Parse(txtSampleCnt.Text);
            if (val <= 0)
            {
                throw new Exception("样本数必须大于0！");
            }
            return val;
        }

        private int ReadSampleCountPerPlate()
        {
            string sCntPerPlateFile = Folders.GetDataFolder() + "countPerPlate.txt";
            return int.Parse(File.ReadAllText(sCntPerPlateFile));
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
                Write2File(pipettingInfoSamples, "samples");
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
                SetInfo(ex.Message+ex.StackTrace);
                return;
            }
#endif
            SetInfo("成功生产worklist!", false);
            EnableNextButton(true);
        }

        void SetInfo(string txt, bool isError = true)
        {
            GlobalVars.Instance.TextInfo.Text = txt;
            GlobalVars.Instance.TextInfo.Foreground = isError ? Brushes.Red : Brushes.Black;
        }


        private void Write2File(List<PipettingInfo> pipettingInfoSamples, string fileName)
        {
            string sFile = Folders.GetOutputFolder() + fileName + ".txt";
            List<string> strs = new List<string>();
            pipettingInfoSamples.ForEach(x => strs.Add(x.ToString()));
            File.WriteAllLines(sFile, strs.ToArray());
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
