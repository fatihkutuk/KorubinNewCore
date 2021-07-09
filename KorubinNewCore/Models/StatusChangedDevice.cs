using System;
using System.Collections.Generic;
using System.Text;

namespace KorubinNewCore.Models
{
    public class StatusChangedDevice
    {
        public int DeviceName { get; set; }
        public string ChannelName { get; set; }
        public string ChannelJson { get; set; }
        public string DeviceJson { get; set; }
        public int PoolId { get; set; }
        public int StatusCode { get; set; }

    }
}
