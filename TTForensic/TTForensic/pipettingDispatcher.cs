using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTForensic
{
    class PipettingDispatcher
    {
        Dictionary<string, int> pcrType_wellID = new Dictionary<string, int>() { };
        public PipettingDispatcher()
        {
            AllocatePCRWellIDs();
        }

        private void AllocatePCRWellIDs()
        {
            for (int i = 0; i < 4; i++)
                pcrType_wellID.Add((i + 1).ToString(), i + 1);
            pcrType_wellID.Add("+", 5);
            pcrType_wellID.Add("-", 6);
        }

        public void PreparePipettingList(ref List<PipettingInfo> pipettingInfoSamples, ref List<PipettingInfo> pipettingInfoPCRs)
        {
            int totalSampleCnt = GlobalVars.Instance.SampleCount;
            int currentSampleIndex = 0;
            int remainingCnt = totalSampleCnt;
            foreach(KeyValuePair<int,Dictionary<POSITION,string>> thisPlatePCRSetting in PCRSettings.Instance.Vals)
            {
                PreparePipettingListThisPCRPlate(thisPlatePCRSetting, remainingCnt, ref pipettingInfoSamples, ref pipettingInfoPCRs, ref currentSampleIndex);
                remainingCnt -= thisPlatePCRSetting.Value.Count;
                if (remainingCnt <= 0)
                    break;
            }
            
        }

        private void PreparePipettingListThisPCRPlate(KeyValuePair<int, Dictionary<POSITION, string>> thisPlatePCRSetting,
            int remainingCnt,
            ref List<PipettingInfo> pipettingInfoSamples,
            ref List<PipettingInfo> pipettingInfoPCRs,
            ref int currentSampleIndex)
        {
            int pcrPlateID = thisPlatePCRSetting.Key;
            string DstPlateName = string.Format("dst{0}",pcrPlateID);
            string PCRLabware = "PCR";
            int thisPlateFinishedCnt = 0;
            foreach(KeyValuePair<POSITION,string> pair in thisPlatePCRSetting.Value)
            {
                if (thisPlateFinishedCnt >= remainingCnt)
                    break;
                string sType = GetTypeFromDescription(pair.Value);
                if(IsNormalSample(pair.Value))
                {
                    string sPlate = "";
                    int wellID = 0;
                    GetSamplePosition(currentSampleIndex++, ref sPlate, ref wellID);
                    int volume = PCRSettings.Instance.PCRType_Settings[sType].sampleVol;
                    pipettingInfoSamples.Add(new PipettingInfo(sPlate, wellID, volume, DstPlateName, pair.Key.WellID));
                }
                int pcrVol = PCRSettings.Instance.PCRType_Settings[sType].pcrVol;
                pipettingInfoPCRs.Add(new PipettingInfo(PCRLabware, pcrType_wellID[sType], pcrVol, DstPlateName, pair.Key.WellID));
                thisPlateFinishedCnt++;
            }
        }
        private string GetTypeFromDescription(string description)
        {
            string s = description.Replace("μl", "");
            return s.Trim();
        }
    
        private bool IsNormalSample(string s)
        {
            int val;
            s = s.Replace("μl", "");
            return int.TryParse(s, out val);
        }

        private void GetSamplePosition(int curSampleIndex, ref string sPlate, ref int wellID)
        {
            int curSampleID = curSampleIndex + 1;
            int smpCntPerPlate = GlobalVars.Instance.SampleCountPerPlate;
            int plateID = (curSampleID + smpCntPerPlate - 1) / smpCntPerPlate;
            wellID = curSampleID - (plateID-1) * smpCntPerPlate;
            sPlate = string.Format("sample{0}", plateID);
        }
    }
}
