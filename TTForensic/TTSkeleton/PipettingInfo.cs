using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTSkeleton
{
    public class PipettingInfo
    {
        public string srcLabware;
        public int srcWellID;
        public double volume;
        public string dstLabware;
        public int dstWellID;

        public PipettingInfo(string srcLabware, int srcWellID, double volume, string dstLabware, int dstWellID)
        {
            this.srcLabware = srcLabware;
            this.srcWellID = srcWellID;
            this.volume = volume;
            this.dstLabware = dstLabware;
            this.dstWellID = dstWellID;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},", srcLabware, srcWellID, volume, dstLabware, dstWellID);
        }
    }
}
