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
        private Dictionary<string, Color> pcrType_ColorDict;
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
        public Dictionary<string, Color> PCRType_Color
        {
            get
            {
                return pcrType_ColorDict;
            }
        }

        private PCRSettings()
        {
            pipettingVolume = int.Parse(GlobalVars.Instance["pipettingVolume"]);
            Vals = new Dictionary<int, Dictionary<POSITION, string>>();
            pcrType_ColorDict = ReadPCRConfigs();
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

            foreach (KeyValuePair<int, Dictionary<POSITION, string>> pair in Vals)
            {
                int plateID = pair.Key;
                string wellDesc = "";
                bool isSequential = IsSequential(pair.Value, ref wellDesc);
                if (!isSequential)
                    throw new Exception(string.Format("{0}号样品板，{1}处未设置！", plateID, wellDesc));
            }
        }

        private bool IsSequential(Dictionary<POSITION, string> dictionary, ref string wellDesc)
        {
            int totalCnt = dictionary.Count;
            
            int expectedWellID = 1;
            foreach(KeyValuePair<POSITION,string> pair in dictionary)
            {
                if (expectedWellID != pair.Key.WellID)
                {
                    wellDesc = Common.GetWellDescription(expectedWellID);
                    return false;
                }
                    
                expectedWellID++;
            }
            return true;
        }
    }

}
