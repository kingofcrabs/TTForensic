using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using TTForensic;
using TTForensic.Utility;


namespace TTForensic
{
    class Worklist
    {
        #region scripts
        string wash = "W;";
        public void GenerateSampleScripts(IEnumerable<PipettingInfo> pipettingInfos)
        {
            int tipCountLiha = int.Parse(GlobalVars.Instance["tipCount"]);
            List<string> strs = new List<string>();
            var liquidClass = "";//GlobalVars.Instance["sampleLiquidClass"];
            while(pipettingInfos.Count() > 0)
            {
                var sameBatchPipettingInfos = pipettingInfos.Take(tipCountLiha);
                strs.AddRange(GenerateSampleScriptsSameBatch(sameBatchPipettingInfos,liquidClass));
                pipettingInfos = pipettingInfos.Except(sameBatchPipettingInfos);
            }
            string sFile = Folders.GetOutputFolder() + "samples.gwl";
            File.WriteAllLines(sFile, strs.ToArray());
        }

        private IEnumerable<string> GenerateSampleScriptsSameBatch(IEnumerable<PipettingInfo> sameBatchPipettingInfos,string liquidClass)
        {
            List<string> strs = new List<string>();
            foreach(var pipettingInfo in sameBatchPipettingInfos)
            {
                strs.Add(GetAspirate(pipettingInfo.srcLabware, pipettingInfo.srcWellID, pipettingInfo.volume, liquidClass));
                strs.Add(GetDispense(pipettingInfo.dstLabware, pipettingInfo.dstWellID, pipettingInfo.volume, liquidClass));
                strs.Add(wash);
            }
            return strs;
        }
        #endregion

        #region pcrScripts
        public void GeneratePCRScripts(List<PipettingInfo> pipettingInfos)
        {
            int tipCountLiha = int.Parse(GlobalVars.Instance["pcrTipCount"]);
            int tipMaxVolume = (int)(int.Parse(GlobalVars.Instance["pcrTipVolumeUL"]) * 0.8);
            List<List<PipettingInfo>> groupsList = pipettingInfos.GroupBy(x => x.srcLabware + x.srcWellID).Select(x=>x.ToList()).ToList(); //group by different tips
            List<string> scripts = new List<string>();
            while (groupsList.Count > 0)
            {
                List<List<PipettingInfo>> sameTipGroups = groupsList.Take(tipCountLiha).ToList(); //same batch tips which is the max cnt setting allows
                scripts.AddRange(GenerateSameBatchScripts(sameTipGroups, tipMaxVolume));
                groupsList = groupsList.Skip(tipCountLiha).ToList();
            }
            string sFile = Folders.GetOutputFolder() + "pcr.gwl";
            File.WriteAllLines(sFile, scripts.ToArray());
        }

        private IEnumerable<string> GenerateSameBatchScripts(List<List<PipettingInfo>> sameTipGroups, int tipMaxVolume)
        {
            List<string> strs = new List<string>();
            while (sameTipGroups.Any(x => x.Count() > 0))
            {
                for (int i = 0; i < sameTipGroups.Count; i++) //go through each tip
                {
                    var curTipPipettings = sameTipGroups[i];
                    int cnt = sameTipGroups[i].Count();
                    if (cnt == 0)
                        continue;
                    int maxAllowCnt = tipMaxVolume / (int)curTipPipettings.First().volume;
                    maxAllowCnt = Math.Min(cnt, maxAllowCnt);
                    var multiDispensePipettingInfos = curTipPipettings.Take(maxAllowCnt);
                    strs.AddRange(GenerateMultiDispenseScript(multiDispensePipettingInfos));
                    sameTipGroups[i] = curTipPipettings.Except(multiDispensePipettingInfos).ToList();
                }
            }
            return strs;
        }

        private IEnumerable<string> GenerateMultiDispenseScript(IEnumerable<PipettingInfo> multiDispensePipettingInfos)
        {
            List<string> strs = new List<string>();
            var firstPipettingInfo = multiDispensePipettingInfos.First();
            var liquidClass = "";//GlobalVars.Instance["pcrLiquidClass"];
            strs.Add(GetAspirate(firstPipettingInfo.srcLabware, firstPipettingInfo.srcWellID, firstPipettingInfo.volume, liquidClass));
            foreach(var pipettingInfo in multiDispensePipettingInfos)
                strs.Add(GetDispense(pipettingInfo.dstLabware, pipettingInfo.dstWellID, pipettingInfo.volume, liquidClass));
            strs.Add(wash);
            return strs;
        }
#endregion

        private string GetAspOrDisp(string sLabware, int wellID, double vol, string liquidClass, bool isAsp)
        {

            string str = string.Format("{4};{0};;;{1};;{2};{3};;",
                        sLabware,
                        wellID,
                        vol, liquidClass, isAsp ? 'A' : 'D');
            return str;
        }

        private string GetAspirate(string sLabware, int srcWellID, double vol, string liquidClass)
        {
            return GetAspOrDisp(sLabware, srcWellID, vol, liquidClass, true);
        }

        private string GetDispense(string sLabware, int dstWellID, double vol, string liquidClass)
        {
            return GetAspOrDisp(sLabware, dstWellID, vol, liquidClass, false);
        }
   
    }
}
