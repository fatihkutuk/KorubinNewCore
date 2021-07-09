using System;
using System.Collections.Generic;
using System.Text;

namespace KorubinNewCore.Models
{
    public class WriteNodeValue
    {
        public int DeviceName { get; set; }
        public string ChannelName { get; set; }
        public string TagName { get; set; }
        public double TagValue { get; set; }
    }
}
