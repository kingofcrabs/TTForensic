using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using TTForensic.Utility;

namespace TTForensic
{
    class PCRSettings
    {
        static PCRSettings instance = null;
        int pipettingVolume; 
        public Dictionary<int, Dictionary<POSITION,string>> Vals { get; set; }
        private Dictionary<string, ItemSetting> pcrType_Settings;
        
        public static PCRSettings Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new PCRSettings();
                }
                return instance;
            }
        }

        public Dictionary<string, ItemSetting> PCRType_Settings
        {
            get
            {
                return pcrType_Settings;
            }
        }

        private PCRSettings()
        {
            pipettingVolume = int.Parse(GlobalVars.Instance["pipettingVolume"]);
            Vals = new Dictionary<int, Dictionary<POSITION, string>>();
            pcrType_Settings = ReadSettings();
        }

        private Dictionary<string, ItemSetting> ReadSettings()
        {
            string sFile = Folders.GetDataFolder() + "pcrDef.txt";
            List<string> strs = File.ReadAllLines(sFile).ToList();
            Dictionary<string, ItemSetting> pairs = new Dictionary<string, ItemSetting>();
            for (int i = 1; i < strs.Count(); i++ )
            {
                var pair = ParseItem(strs[i]);
                pairs.Add(pair.Key, pair.Value);
            }
            return pairs;
        }

        private KeyValuePair<string, ItemSetting> ParseItem(string s)
        {
            string[] vol_colors = s.Split('[');
            string[] strs = vol_colors[0].Split(',');
            int i = 0;
            string sType = strs[i++];
            int sampleVol = int.Parse(strs[i++]);
            int pcrVol = int.Parse(strs[i++]);
            Color color = ParseColor(vol_colors[1]);
            return new KeyValuePair<string, ItemSetting>(sType, new ItemSetting(sampleVol, pcrVol, color));
        }

        private Color ParseColor(string sColor)
        {
            sColor = sColor.Replace("[","");
            sColor = sColor.Replace("]","");
            string[] strs = sColor.Split(',');
            int i = 0;
            byte r = byte.Parse(strs[i++]);
            byte g = byte.Parse(strs[i++]);
            byte b = byte.Parse(strs[i++]);
            return Color.FromArgb(255, r, g, b);
        }

        internal void Set(POSITION start, POSITION end)
        {
            Set(start.x, start.y, end.x, end.y);
        }

        private void Set(int startColIndex, int startRowIndex, int endColIndex, int endRowIndex)
        {
            if (!Vals.ContainsKey(GlobalVars.Instance.CurrentPlateID))
                Vals.Add(GlobalVars.Instance.CurrentPlateID, new Dictionary<POSITION, string>());
            Dictionary<POSITION, string> curPlatePosVals = Vals[GlobalVars.Instance.CurrentPlateID];
            for( int x = startColIndex; x< endColIndex; x++)
            {
                for( int y = startRowIndex; y < endRowIndex; y++)
                {
                    POSITION pos = new POSITION(x,y);
                    if (curPlatePosVals.ContainsKey(pos))
                        curPlatePosVals[pos] = GlobalVars.Instance.PCRType;
                    else
                        curPlatePosVals.Add(pos, GlobalVars.Instance.PCRType);
                }
            }
        }

        private Dictionary<string, Color> ReadPCRConfigs()
        {
            string sFile = Folders.GetDataFolder() + "pcrVol_Colors.txt";
            IEnumerable<string> strs = File.ReadAllLines(sFile);
            Dictionary<string, Color> pairs = new Dictionary<string, Color>();
            foreach (string s in strs)
            {
                var pair = ParsePair(s);
                pairs.Add(pair.Key, pair.Value);
            }
            return pairs;
        }

        private KeyValuePair<string, Color> ParsePair(string s)
        {
            string[] strs = s.Split(',');
            int i = 0;
            string desc = strs[i++].Trim();
            //byte a = byte.Parse(strs[i++]);
            byte r = byte.Parse(strs[i++]);
            byte g = byte.Parse(strs[i++]);
            byte b = byte.Parse(strs[i++]);
            return new KeyValuePair<string, Color>(desc, Color.FromArgb(255, r, g, b));
        }

        public int GetSampleVolume(string pcrType)
        {
            string sDigit = Common.GetDigitalStr(pcrType);
            if(sDigit != "")
            {
                return int.Parse(sDigit);
            }
            return pipettingVolume;
        }

        public void CheckValid()
        {
            int totalCnt = Vals.Sum(x => x.Value.Count());
            if(totalCnt < GlobalVars.Instance.SampleCount)
            {
                throw new Exception(string.Format("样品未能全部设置,期望数量为:{0}已设置数量为:{1}！", GlobalVars.Instance.SampleCount, totalCnt));
            }

            for (int i = 0; i < Vals.Count; i++ )
            {
                var dict = Vals.ElementAt(i).Value;
                dict = dict.OrderBy(x => x.Key.WellID).ToDictionary(x => x.Key, x => x.Value);
                Vals[i + 1] = dict;
            }

            int remainingCnt = GlobalVars.Instance.SampleCount;
            foreach (KeyValuePair<int, Dictionary<POSITION, string>> pair in Vals)
            {
                int plateID = pair.Key;
                string wellDesc = "";
                bool isSequential = IsSequential(remainingCnt,pair.Value, ref wellDesc);
                if (!isSequential)
                    throw new Exception(string.Format("{0}号样品板，{1}处未设置！", plateID, wellDesc));
                remainingCnt -= pair.Value.Count;
            }
        }

        private bool IsSequential(int remainingCnt, Dictionary<POSITION, string> dictionary, ref string wellDesc)
        {
            int totalCnt = dictionary.Count;
            int expectedWellID = 1;

            int cntThisPlate = 0;
            foreach (KeyValuePair<POSITION,string> pair in dictionary)
            {
                if (cntThisPlate > remainingCnt)
                    break;
                if (expectedWellID != pair.Key.WellID)
                {
                    wellDesc = Common.GetWellDescription(expectedWellID);
                    return false;
                }
                cntThisPlate++;   
                expectedWellID++;
            }
            return true;
        }
    }


    struct ItemSetting
    {
        public int sampleVol;
        public int pcrVol;
        public Color color;
        public ItemSetting(int sampleVol, int pcrVol, Color color)
        {
            // TODO: Complete member initialization
            this.sampleVol = sampleVol;
            this.pcrVol = pcrVol;
            this.color = color;
        }
    }
}
