﻿using KorubinNewCore.Helpers;
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

namespace KorubinNewCore.Managers
{
    public class ClientManager
    {
        bool _stopped = false;
        int _clientId;
        TimerManager _timerManager;
        Session _session;
        Subscription _subscription;
        HashSet<MonitoredItem> _nodes;
        DatabaseManager _databaseManager;
        public ClientManager(int clientId, HashSet<MonitoredItem> nodes)
        {
            _timerManager = new TimerManager();
            _databaseManager = new DatabaseManager();
            _clientId = clientId;
            _nodes = nodes;

            var opcConfig = new OpcCertification();
            var config = opcConfig.GetConfiguration();
            _session = Session.Create(config,
                new ConfiguredEndpoint(null, new EndpointDescription(ConfigurationManager.ConnectionStrings["OpcStr"].ConnectionString)),
                true,
                "",
                60000,
                null,
                null).Result;
            _subscription = new Subscription(_session.DefaultSubscription)
            {
                PublishingInterval = 1000,
                MaxNotificationsPerPublish = 10000
            };
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

            ExtendedTimer checkReconnect = new ExtendedTimer("checkReconnect" + _clientId, 10000);
            checkReconnect.Elapsed += new ElapsedEventHandler(checkReconnectEvent);

            _timerManager.AddTimer(tagWriteTimer);
            _timerManager.AddTimer(setClientStateTimer);
            _timerManager.AddTimer(getDeviceStatusTimer);
            _timerManager.AddTimer(checkReconnect);
            Run();
            


        }
        public void Run()
        {
            try
            {


                _timerManager.StartAll();

                _subscription.AddItems(_nodes);
                _subscription.FastDataChangeCallback += new FastDataChangeNotificationEventHandler(DataChanged);
                _session.AddSubscription(_subscription);
                _session.OperationTimeout = 3600000;
                _subscription.Create();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message + "--" + _clientId);
            }
        }
        private void checkReconnectEvent(object sender, ElapsedEventArgs e)
        {
            CheckReconnect();
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
        public void CheckReconnect()
        {
            var changedDeviceList = _databaseManager.GetClientStatusChanged(_clientId);
            if (changedDeviceList.Count>0 && _stopped == true)
            {
                Run();
            }
        }
        public void CheckDeviceStatus()
        {
            try
            {
                var changedDeviceList = _databaseManager.GetClientStatusChanged(_clientId);
                if (changedDeviceList.Count > 0)
                {
                    _timerManager.StopAll();
                    _subscription.Delete(true);
                    _session.Close();
                    _stopped = true;
                    _timerManager.StartTimer("checkReconnect" + _clientId);
                }
                else
                {
                    if (_timerManager.IsTimersStopped == true)
                    {
                        _timerManager.StartAll();
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
