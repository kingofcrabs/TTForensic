using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using TTForensic;


namespace SaintX.Utility
{
    class worklist
    {
        #region scripts
        string wash = "W;";
        public void GenerateSampleScripts(IEnumerable<PipettingInfo> pipettingInfos)
        {
            int tipCountLiha = int.Parse(GlobalVars.Instance["tipCount"]);
            List<string> strs = new List<string>();
            var liquidClass = GlobalVars.Instance["sampleLiquidClass"];
            while(pipettingInfos.Count() > 0)
            {
                var sameBatchPipettingInfos = pipettingInfos.Take(tipCountLiha);
                strs.AddRange(GenerateSampleScriptsSameBatch(sameBatchPipettingInfos,liquidClass));
                pipettingInfos = pipettingInfos.Except(sameBatchPipettingInfos);
            }
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
            var groups = pipettingInfos.GroupBy(x => x.srcLabware + x.srcWellID); //group by different tips
            List<string> scripts = new List<string>();
            while(groups.Count() > 0)
            {
                var sameTipGroups = groups.Take(tipCountLiha); //same batch tips which is the max cnt setting allows
                scripts.AddRange(GenerateSameBatchScripts(sameTipGroups, tipMaxVolume));
                groups = groups.Except(sameTipGroups);
            }
        }

        private IEnumerable<string> GenerateSameBatchScripts(IEnumerable<IGrouping<string, PipettingInfo>> sameTipGroups,int maxTipVolume)
        {
            List<string> strs = new List<string>();
            List<List<PipettingInfo>> groups = new List<List<PipettingInfo>>();
            foreach(var group in sameTipGroups)
            {
                groups.Add(group.ToList());
            }
            while (groups.Any(x => x.Count() > 0))
            {
                for (int i = 0; i < groups.Count; i++) //go through each group
                {
                    var curGroup = groups[i];
                    int cnt = curGroup.Count();
                    if( cnt == 0)
                        continue;
                    int maxAllowCnt = maxTipVolume/ (int)curGroup.First().volume;
                    maxAllowCnt = Math.Min(cnt, maxAllowCnt);
                    var multiDispensePipettingInfos = curGroup.Take(maxAllowCnt);
                    strs.AddRange(GenerateMultiDispenseScript(multiDispensePipettingInfos));
                    curGroup = curGroup.Except(multiDispensePipettingInfos).ToList();
                }
            }
            return strs;
        }

        private IEnumerable<string> GenerateMultiDispenseScript(IEnumerable<PipettingInfo> multiDispensePipettingInfos)
        {
            List<string> strs = new List<string>();
            var firstPipettingInfo = multiDispensePipettingInfos.First();
            var liquidClass = GlobalVars.Instance["pcrLiquidClass"];
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
