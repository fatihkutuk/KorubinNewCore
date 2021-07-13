using System;
using System.Collections.Generic;
using System.Configuration;
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
                OnServiceStart onServiceStart = new OnServiceStart();
                var clientCount = ConfigurationManager.AppSettings["ClientDeviceCount"].ToString();

                onServiceStart.Initializer(Convert.ToInt32(clientCount));


                var serviceName = ConfigurationManager.AppSettings["KepwareServiceName"].ToString();
                var result = onServiceStart.RestartServiceByServiceName(serviceName);

                

                if (result)
                {
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
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);

            }
            Console.ReadLine();
        }
    }
}
