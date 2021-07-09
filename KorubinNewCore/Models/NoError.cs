using System;
using System.Collections.Generic;
using System.Text;

namespace KorubinNewCore.Models
{
    public class NoError
    {
        public string ChannelName { get; set; }
        public int DeviceName { get; set; }

        public string TagName { get; set; } = "_NoError";

        public override string ToString()
        {
            return $"ns=2;s={ChannelName}.{DeviceName}._System._NoError";
        }
    }
}
