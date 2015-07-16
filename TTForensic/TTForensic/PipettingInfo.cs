using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTForensic
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
    }

}
