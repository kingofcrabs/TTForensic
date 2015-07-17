using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTForensic
{
    class PipettingDispatcher
    {
        int pipettingVolume = int.Parse(GlobalVars.Instance["pipettingVolume"]);
        Dictionary<string, int> pcrType_wellID = new Dictionary<string, int>() { };

        public PipettingDispatcher()
        {
            for (int i = 0; i < 4; i++)
                pcrType_wellID.Add((i + 1).ToString(), i + 1);
            pcrType_wellID.Add("+", 5);
            pcrType_wellID.Add("-", 6);
        }
        void PreparePipettingList(ref List<PipettingInfo> pipettingInfoSamples, ref List<PipettingInfo> pipettingInfoPCRs)
        {
            int totalSampleCnt = GlobalVars.Instance.SampleCount;
            int currentSampleIndex = 0;
            foreach(KeyValuePair<int,Dictionary<POSITION,string>> thisPlatePCRSetting in PCRSettings.Instance.Vals)
            {
                PreparePipettingListThisPCRPlate(thisPlatePCRSetting, ref pipettingInfoSamples, ref pipettingInfoPCRs, ref currentSampleIndex);
            }
            
        }

        private void PreparePipettingListThisPCRPlate(KeyValuePair<int, Dictionary<POSITION, string>> thisPlatePCRSetting,
            ref List<PipettingInfo> pipettingInfoSamples,
            ref List<PipettingInfo> pipettingInfoPCRs,
            ref int currentSampleIndex)
        {
            int pcrPlateID = thisPlatePCRSetting.Key;
            string PCRPlateName = string.Format("PCRPlate{0}",pcrPlateID);
            string PCRLabware = "PCR";
            foreach(KeyValuePair<POSITION,string> pair in thisPlatePCRSetting.Value)
            {
                if(IsNormalSample(pair.Value))
                {
                    string sPlate = "";
                    int wellID = 0;
                    GetSamplePosition(currentSampleIndex++, ref sPlate, ref wellID);
                    int volume = GetSampleVolume(pair.Value);
                    pipettingInfoSamples.Add(new PipettingInfo(sPlate, wellID, volume, PCRPlateName, pair.Key.WellID));
                }

                //PCR
                pipettingInfoPCRs.Add(new PipettingInfo(PCRLabware, pcrType_wellID[pair.Value], GetPCRVolume(pair.Value), PCRPlateName, pair.Key.WellID));
            }
        }

        private int GetSampleVolume(string s)
        {
            int val;
            int.TryParse(s, out val);
            return pipettingVolume - val;
        }
        private int GetPCRVolume(string s)
        {
            int val;
            int.TryParse(s, out val);
            return val;
        }
        private bool IsNormalSample(string s)
        {
            int val;
            return int.TryParse(s, out val);
        }

        private void GetSamplePosition(int curSampleIndex, ref string sPlate, ref int wellID)
        {
            int curSampleID = curSampleIndex + 1;
            int smpCntPerPlate = GlobalVars.Instance.SampleCountPerPlate;
            int plateID = (curSampleID + smpCntPerPlate - 1) / smpCntPerPlate;
            wellID = curSampleID - (plateID-1) * smpCntPerPlate;
            sPlate = string.Format("sample{0}", curSampleID);
        }
    }
}
