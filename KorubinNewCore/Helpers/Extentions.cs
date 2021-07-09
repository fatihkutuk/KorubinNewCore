using KorubinNewCore.Models;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace KorubinNewCore.Helpers
{
    public static class Extensions
    {
        public static void CreateMonitoredItemList(this HashSet<Node> channels, out HashSet<MonitoredItem> monitoredItems)
        {
            monitoredItems = new HashSet<MonitoredItem>();

            foreach (var item in channels)
            {
                monitoredItems.Add(new MonitoredItem
                {
                    Handle = item,
                    StartNodeId = item.ToString(),

                });
            }
        }
    }
}
