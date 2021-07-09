using System;
using System.Collections.Generic;
using System.Text;

namespace KorubinNewCore.Models
{
    public class Node
    {
        public int DeviceTagId { get; set; }
        public string ChannelName { get; set; }
        public int DeviceName { get; set; }
        public string TagName { get; set; } = "_NoError";

        public override string ToString()
        {
            if (TagName == "_NoError")
            {
                return $"ns=2;s={ChannelName}.{DeviceName}._System.{TagName}";
            }
            else
            {
                return $"ns=2;s={ChannelName}.{DeviceName}.{TagName}";
            }
        }
    }

}
