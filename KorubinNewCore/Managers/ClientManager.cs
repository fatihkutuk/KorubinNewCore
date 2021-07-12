using KorubinNewCore.Helpers;
using KorubinNewCore.Models;
using Opc.Ua.Client;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.Threading;
using System.Timers;
using System.Threading.Tasks;

namespace KorubinNewCore.Managers
{
    public class ClientManager
    {
        bool _stopped = false;
        int _clientId;
        int[] _statusCodes = { 60,50,40,30,20,10};
        string _restApiResult;
        string _restApiIndividualResult;
        string _deviceTagJson;
        string _deviceIndividualTagJson;

        TimerManager _timerManager;
        Session _session;
        Subscription _subscription;
        HashSet<MonitoredItem> _nodes;
        DatabaseManager _databaseManager;
        ApplicationConfiguration _config;
        KepwareRestApiManager _kepwareRestApiManager;
        HashSet<Models.Node> _subscriptionList;
        HashSet<Models.Node> _noErrorNodes;

        public ClientManager(int clientId)
        {
            _timerManager = new TimerManager();
            _databaseManager = new DatabaseManager();
            _kepwareRestApiManager = new KepwareRestApiManager();
            _clientId = clientId;
            var opcConfig = new OpcCertification();
            _config = opcConfig.GetConfiguration();


        }
        public void Initialize()
        {
            _subscriptionList = _databaseManager.GetNodes(_clientId);
            _noErrorNodes = _databaseManager.GetNoErrorNodes(_clientId);
            _subscriptionList.CreateMonitoredItemList(out HashSet<MonitoredItem> monitoredNodes);
            _noErrorNodes.CreateMonitoredItemList(out HashSet<MonitoredItem> monitoredNoErrorNodes);
            monitoredNodes.UnionWith(monitoredNoErrorNodes);
            _nodes = monitoredNodes;
        }
        public void Start()
        {

            _stopped = false;

            ExtendedTimer tagWriteTimer = new ExtendedTimer("tagWriteTimer" + _clientId, 1000);
            tagWriteTimer.Elapsed += new ElapsedEventHandler(tagWriteTimerEvent);


            ExtendedTimer setClientStateTimer = new ExtendedTimer("setClientStateTimer" + _clientId, 1000);
            setClientStateTimer.Elapsed += new ElapsedEventHandler(setClientStateTimerEvent);


            ExtendedTimer getDeviceStatusTimer = new ExtendedTimer("getDeviceStatusTimer" + _clientId, 10000);
            getDeviceStatusTimer.Elapsed += new ElapsedEventHandler(getDeviceStatusTimerEvent);



            _timerManager.AddTimer(tagWriteTimer);
            _timerManager.AddTimer(setClientStateTimer);
            _timerManager.AddTimer(getDeviceStatusTimer);
            Run();
            


        }
        public void Run()
        {

            try
            {
                Initialize();

                _stopped = false;
                
                _session = Session.Create(_config,
                new ConfiguredEndpoint(null, new EndpointDescription(ConfigurationManager.ConnectionStrings["OpcStr"].ConnectionString)),
                true,
                "",
                60000,
                null,
                null).Result;

                _subscription = new Subscription(_session.DefaultSubscription)
                {
                    PublishingInterval = 500,
                    MaxNotificationsPerPublish = 10000
                };

                _subscription.AddItems(_nodes);
                _subscription.FastDataChangeCallback += new FastDataChangeNotificationEventHandler(DataChanged);
                _session.AddSubscription(_subscription);
                _session.OperationTimeout = 3600000;
                _subscription.Create();
                _timerManager.StartAll();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message + "--" + _clientId);
            }
        }


        private void getDeviceStatusTimerEvent(object sender, ElapsedEventArgs e)
        {
            CheckDeviceStatus();
        }

        private void setClientStateTimerEvent(object sender, ElapsedEventArgs e)
        {
            UpdateClientState();
        }

        private void tagWriteTimerEvent(object sender, ElapsedEventArgs e)
        {
            WriteTagsToServer();
        }

        public void UpdateServer(HashSet<StatusChangedDevice> devices,int statusCode)
        {
            Thread.Sleep(40000);
            foreach (var item in devices)
            {
                switch (statusCode)
                {
                    case 30:
                        _restApiResult = _kepwareRestApiManager.ChannelPut(item.ChannelJson, item.ChannelName);
                        if(_restApiResult != "FAILED")
                        {
                            _databaseManager.SetDeviceStatus(item.DeviceName,31);
                        }
                        else
                        {
                            _databaseManager.SetDeviceStatus(item.DeviceName, 32);
                        }
                        break;
                    case 60:

                        _deviceTagJson = _databaseManager.GetDeviceTagJsonByDeviceId(item.DeviceName);
                        _restApiResult = _kepwareRestApiManager.TagPost(_deviceTagJson.ToString(), item.ChannelName, item.DeviceName.ToString());

                        _deviceIndividualTagJson = _databaseManager.GetDeviceIndividualTagJsonByDeviceId(item.DeviceName);
                        _restApiIndividualResult = _kepwareRestApiManager.TagPost(_deviceIndividualTagJson.ToString(), item.ChannelName, item.DeviceName.ToString());
                        if (_restApiResult != "FAILED" && _restApiIndividualResult != "FAILED")
                        {
                            _databaseManager.SetDeviceStatus(item.DeviceName, 61);
                        }
                        else
                        {
                            _databaseManager.SetDeviceStatus(item.DeviceName, 62);
                        }
                        break;
                    case 10:
                        string channelPost = _kepwareRestApiManager.ChannelPost(item.ChannelJson);
                        string message = _kepwareRestApiManager.DevicePost(item.DeviceJson, item.ChannelName);
                        if ("Created" == message || message == "Exist")
                        {
                            _deviceTagJson = _databaseManager.GetDeviceTagJsonByDeviceId(item.DeviceName);
                            _restApiResult = _kepwareRestApiManager.TagPost(_deviceTagJson.ToString(), item.ChannelName, item.DeviceName.ToString());

                            _deviceIndividualTagJson = _databaseManager.GetDeviceIndividualTagJsonByDeviceId(item.DeviceName);
                            _restApiIndividualResult = _kepwareRestApiManager.TagPost(_deviceIndividualTagJson.ToString(), item.ChannelName, item.DeviceName.ToString());
                            if (_restApiResult != "FAILED" && _restApiIndividualResult != "FAILED")
                            {
                                _databaseManager.SetDeviceStatus(item.DeviceName, 11);
                            }
                            else
                            {
                                _databaseManager.SetDeviceStatus(item.DeviceName, 12);
                            }
                        }
                            break;

                    default:
                        break;
                }
                

            }
            Run();
        }
        public void CheckDeviceStatus()
        {

            try
            {
                foreach (var item in _statusCodes)
                {
                    var changedDeviceList = _databaseManager.GetClientStatusChanged(item,_clientId);
                    if (changedDeviceList.Count > 0)
                    {
                        _timerManager.StopAll();
                        _session.Close();
                        _subscription.Delete(true);
                        _subscription.Dispose();

                        _stopped = true;
                        UpdateServer(changedDeviceList, item);
                    }
                }                
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void UpdateClientState()
        {
            try
            {
                if (_session != null && _session.KeepAliveStopped)
                {
                    _databaseManager.SetClientState(_clientId, "Bad");
                }
                else
                {
                    _databaseManager.SetClientState(_clientId, "Ok");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "--" + _clientId);
            }


        }
        public void WriteTagsToServer()
        {
            try
            {
                WriteValueCollection nodesToWrite = new WriteValueCollection();
                WriteValue intWriteVal = new WriteValue();
                StatusCodeCollection results = null;
                DiagnosticInfoCollection diagnosticInfos;
                var writeNodeValues = _databaseManager.TagsToWrite(_clientId);
                foreach (var item in writeNodeValues)
                {
                    var ns = _session.ReadValue(new NodeId($"ns=2;{item.ChannelName}.{item.DeviceName}.{item.TagName}"));
                    var ns_type = ns.WrappedValue.TypeInfo.ToString();

                    intWriteVal.NodeId = new NodeId($"ns=2;{item.ChannelName}.{item.DeviceName}.{item.TagName}");
                    intWriteVal.AttributeId = Attributes.Value;
                    intWriteVal.Value = new DataValue();
                    var tag_val = ChangeType(ns_type, item.TagValue);
                    intWriteVal.Value.Value = tag_val;
                    nodesToWrite.Add(intWriteVal);

                    _session.Write(null,
                    nodesToWrite,
                    out results,
                    out diagnosticInfos);
              
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "--" + _clientId);
            }

        }
        public object ChangeType(string type, double val)
        {
            object returned_value = 0;
            switch (type)
            {
                case "Int16":
                    returned_value = Convert.ToInt16(val);
                    break;
                case "Int32":
                    returned_value = Convert.ToInt32(val);
                    break;
                case "Double":
                    returned_value = Convert.ToDouble(val);
                    break;
                case "Float":
                    returned_value = (float)(val);
                    break;
                case "Boolean":
                    returned_value = Convert.ToBoolean(val);
                    break;
            }
            return returned_value;
        }

        private void DataChanged(Subscription subscription, DataChangeNotification notification, IList<string> stringTable)
        {
            int c = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("Call sp_setTagValueOnDataChanged(\"");
            try
            {
                foreach (var item in notification.MonitoredItems)
                {
                    var m = subscription.FindItemByClientHandle(item.ClientHandle);
                    Models.Node info = (Models.Node)m.Handle;
                    if (item.Value.StatusCode.ToString() == "Good")
                    {
                        sb.Append($"({info.DeviceName},'{info.TagName}',{Convert.ToDouble(item.Value.Value).ToString("f6", CultureInfo.InvariantCulture)}),");
                        c++;
                    }
                }
                if (c > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("\")");
                    _databaseManager.InsertValues(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataChanged Exception: {0} -- {1}", ex.Message, _clientId);
            }
            sb.Clear();
        }
    }
}
