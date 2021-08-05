using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;
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
                var clientCount = ConfigurationManager.AppSettings["ClientDeviceCount"].ToString();
                var serviceName = ConfigurationManager.AppSettings["KepwareServiceName"].ToString();

                OnServiceStart onServiceStart = new OnServiceStart(serviceName);
                var result = onServiceStart.Initializer(Convert.ToInt32(clientCount));
                
                DatabaseManager databaseManager = new DatabaseManager();
                var clientList = databaseManager.GetClients();

                

                foreach (var clientId in clientList)
                {
                    

                    Task.Run(() =>
                    {
                        var _client = new ClientManager(clientId);
                        _client.Start();
                    });
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);

            }
            Console.ReadLine();
        }
    }
}
