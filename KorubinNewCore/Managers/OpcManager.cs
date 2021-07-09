using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KorubinNewCore.Helpers;
using Opc.Ua.Client;

namespace KorubinNewCore.Managers
{
    public class OpcManager
    {
        public OpcManager()
        {

        }
        public void Start()
        {
            try
            {
                DatabaseManager databaseManager = new DatabaseManager();
                var clientList = databaseManager.GetClients();

                foreach (var clientId in clientList)
                {
                    Task.Run(() =>
                    {
                        var subscriptionList = databaseManager.GetNodes(clientId);
                        var noErrorNodes = databaseManager.GetNoErrorNodes(clientId);
                        subscriptionList.CreateMonitoredItemList(out HashSet<MonitoredItem> monitoredNodes);
                        noErrorNodes.CreateMonitoredItemList(out HashSet<MonitoredItem> monitoredNoErrorNodes);
                        monitoredNodes.UnionWith(monitoredNoErrorNodes);
                        var _client = new ClientManager(clientId, monitoredNodes);
                        _client.Start();
                });

            }
                Console.ReadLine();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
