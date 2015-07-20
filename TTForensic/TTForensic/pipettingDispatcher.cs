﻿using System;
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
                if (thisPlateFinishedCnt > remainingCnt)
                    break;
                if(IsNormalSample(pair.Value))
                {
                    string sPlate = "";
                    int wellID = 0;
                    GetSamplePosition(currentSampleIndex++, ref sPlate, ref wellID);
                    int volume = GetSampleVolume(pair.Value);
                    pipettingInfoSamples.Add(new PipettingInfo(sPlate, wellID, volume, DstPlateName, pair.Key.WellID));
                }

                //PCR
                string sType = GetTypeFromDescription(pair.Value);
                pipettingInfoPCRs.Add(new PipettingInfo(PCRLabware, pcrType_wellID[sType], GetPCRVolume(pair.Value), DstPlateName, pair.Key.WellID));
                thisPlateFinishedCnt++;
            }
        }
        private string GetTypeFromDescription(string description)
        {
            string s = description.Replace("μl", "");
            return s;
        }
        private int GetSampleVolume(string s)
        {
            int val;
            s = s.Replace("μl", "");
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
            s = s.Replace("μl", "");
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
