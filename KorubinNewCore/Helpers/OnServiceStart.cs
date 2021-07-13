using KorubinNewCore.Managers;
using KorubinNewCore.Models;
using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace KorubinNewCore.Helpers
{

    public class OnServiceStart
    {
        OpcCertification _opcConfig;
        ApplicationConfiguration _config;
        Session _session;
        ReferenceDescriptionCollection refs;
        byte[] cp;
        KepwareRestApiManager _kepwareRestApiManager;
        DatabaseManager _databaseManager = new DatabaseManager();
        HashSet<string> _channelList = new HashSet<string>();
        public OnServiceStart()
        {
            _kepwareRestApiManager = new KepwareRestApiManager();
            _opcConfig = new OpcCertification();
            _config = _opcConfig.GetConfiguration();



            _session = Session.Create(_config,
            new ConfiguredEndpoint(null, new EndpointDescription(ConfigurationManager.ConnectionStrings["OpcStr"].ConnectionString)),
            true,
            "",
            60000,
            null,
            null).Result;


        }

        public bool RestartServiceByServiceName(string ServiceName)
        {
            bool _serviceStatus = false;

            ServiceController kepwareService = new ServiceController
            {
                ServiceName = ServiceName,
            };

            switch (kepwareService.Status)
            {
                case ServiceControllerStatus.Running:

                    kepwareService.Stop();
                    kepwareService.WaitForStatus(ServiceControllerStatus.Stopped);
                    kepwareService.Start();
                    kepwareService.WaitForStatus(ServiceControllerStatus.Running);
                    _serviceStatus = true;
                    break;
                case ServiceControllerStatus.StartPending:
                    kepwareService.WaitForStatus(ServiceControllerStatus.Running);
                    _serviceStatus = true;
                    break;
                case ServiceControllerStatus.Stopped:
                    kepwareService.Start();
                    kepwareService.WaitForStatus(ServiceControllerStatus.Running);
                    _serviceStatus = true;
                    break;
                case ServiceControllerStatus.StopPending:
                    kepwareService.WaitForStatus(ServiceControllerStatus.Stopped);
                    kepwareService.Start();
                    kepwareService.WaitForStatus(ServiceControllerStatus.Running);
                    _serviceStatus = true;

                    break;
            }
            return _serviceStatus;
        }

        public bool Initializer(int deviceCountInClient)
        {
            GetKepwareChannelList();
            HashSet<OpcInit> devices = _databaseManager.GetOpcInit();
            devices = devices.OrderBy(a => a.DeviceId).ToHashSet();



            foreach (var device in devices)
            {
                if (_channelList.Any(a => a.StartsWith(device.ChannelName)) == false)
                {
                    _kepwareRestApiManager.ChannelPost(device.ChannelJson);
                }

                if (_channelList.Any(a => a.Equals($"{device.ChannelName}.{device.DeviceId}")) == false)
                {
                    _kepwareRestApiManager.DevicePost(device.DeviceJson, device.ChannelName);
                    string individualTagList = _databaseManager.GetDeviceIndividualTagJsonByDeviceId(device.DeviceId);
                    _kepwareRestApiManager.TagPost(individualTagList, device.ChannelName, device.DeviceId.ToString());

                    string tagJsonList = _databaseManager.GetDeviceTagJsonByDeviceId(device.DeviceId);
                    _kepwareRestApiManager.TagPost(tagJsonList, device.ChannelName, device.DeviceId.ToString());
                }
            }

            double deviceCount = devices.Count;
            int clientCount = (int)Math.Ceiling(deviceCount / deviceCountInClient);
            string sql = string.Empty;
            for (int i = 0; i < clientCount; i++)
            {
                sql = string.Empty;
                devices.Skip(i * deviceCountInClient).Take(deviceCountInClient).ToList().ForEach(a => sql += a.DeviceId + ",");
               _databaseManager.setClientToDevices(i + 1, sql.Remove(sql.Length - 1, 1));
            }

            return true;
        }
        public void GetKepwareChannelList(string channelName="")
        {

            _session.Browse(
                null,
                null,
                channelName =="" ? ObjectIds.ObjectsFolder: new NodeId(channelName),
                0u,
                BrowseDirection.Forward,
                ReferenceTypeIds.HierarchicalReferences,
                true,
                (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                out cp,
                out refs);

            foreach (var element in refs)
            {
                if (!element.DisplayName.Text.StartsWith("_") && element.DisplayName.Text != "Server")
                {
                    _channelList.Add(element.NodeId.Identifier.ToString());
                    if (element.NodeId.ToString().Split(".").Length!=2)
                    {
                        GetKepwareChannelList(element.NodeId.ToString()); //Search other elements
                    }
                }
            }
        }
    }
}
