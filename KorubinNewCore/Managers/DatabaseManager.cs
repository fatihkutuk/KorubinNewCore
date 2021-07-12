using KorubinNewCore.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace KorubinNewCore.Managers
{
    public class DatabaseManager
    {
        string connectionString;
        public DatabaseManager()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DbStr"].ConnectionString;
        }
        public List<int> GetClients()
        {
            //Console.WriteLine(connectionString);
            List<int> clients = new List<int>();
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new MySqlCommand("Select cd.clientId from channeldevice cd group by cd.clientId;", con);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        clients.Add(reader.GetInt32("clientId"));
                    }
                    con.Close();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return clients;
        }
        public HashSet<OpcInit> GetOpcInit()
        {
            //Console.WriteLine(connectionString);
            HashSet<OpcInit> inits = new HashSet<OpcInit>();
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new MySqlCommand($"CALL `sp_getInit`()", con);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        inits.Add( new OpcInit() { 
                            DeviceId = reader.GetInt32("DeviceId"),
                            ChannelJson = reader.GetString("channelJson"),
                            DeviceJson = reader.GetString("DeviceJson"),
                            ChannelName = reader.GetString("channelName")
                        });
                    }
                    con.Close();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return inits;
        }
        public string GetDeviceTagJsonByDeviceId(int deviceId)
        {

            string deviceTags;
            try
            {

                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new MySqlCommand($"CALL sp_getDeviceTagjSons({deviceId})", con);


                    var reader = cmd.ExecuteScalar();
                    deviceTags = reader.ToString();
                    con.Close();
                }


            }
            catch (Exception)
            {

                throw;
            }
            return deviceTags;

        }
        public string GetDeviceIndividualTagJsonByDeviceId(int deviceId)
        {

            string deviceTags;
            try
            {

                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new MySqlCommand($"CALL sp_getDeviceIndividualTagJsons({deviceId})", con);


                    var reader = cmd.ExecuteScalar();
                    deviceTags = reader.ToString();
                    con.Close();
                }


            }
            catch (Exception)
            {

                throw;
            }
            return deviceTags;

        }

        public HashSet<StatusChangedDevice> GetClientStatusChanged(int statusCode, int clientId)
        {
            HashSet<StatusChangedDevice> statusChangedDevice = new HashSet<StatusChangedDevice>();
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new MySqlCommand($"CALL sp_getDeviceInfoByStatusCode({statusCode},{clientId})", con);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        statusChangedDevice.Add(new StatusChangedDevice
                        {

                            DeviceName = reader.GetInt32("DeviceId"),
                            ChannelJson = reader.GetString("channelJson"),
                            DeviceJson = reader.GetString("DeviceJson"),
                            PoolId = reader.GetInt32("DeviceTypeId"),
                            ChannelName = reader.GetString("ChannelName"),
                            StatusCode = reader.GetInt32("StatusCode")
                        });
                    }
                    con.Close();

                }
            }
            catch (Exception)
            {

                throw;
            }
            return statusChangedDevice;
        }
        public void SetDeviceStatus(int deviceId, int statusCode)
        {
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new MySqlCommand($"Call sp_setDeviceStatusById({statusCode},{deviceId});", con);
                    var res = cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public HashSet<Node> GetNodes(int clientId)
        {
            HashSet<Node> nodes = new HashSet<Node>();
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new MySqlCommand($"Call sp_getClientSubscriptionList({clientId});", con);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        nodes.Add(new Node
                        {
                            ChannelName = reader.GetString("ChannelName"),
                            DeviceName = reader.GetInt32("DeviceName"),
                            TagName = reader.GetString("TagName"),
                            DeviceTagId = reader.GetInt32("DeviceTagId")
                        });
                    }
                    con.Close();

                }
            }
            catch (Exception)
            {

                throw;
            }
            return nodes;
        }
        public HashSet<Node> GetNoErrorNodes(int clientId)
        {
            HashSet<Node> noErrorNodes = new HashSet<Node>();
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new MySqlCommand($"SELECT cd.channelName, cd.id FROM dbkepware.channeldevice cd where cd.clientId = {clientId}", con);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        noErrorNodes.Add(new Node
                        {
                            ChannelName = reader.GetString("channelName"),
                            DeviceName = reader.GetInt32("id"),

                        });
                    }
                    con.Close();

                }
            }
            catch (Exception)
            {

                throw;
            }
            return noErrorNodes;
        }
        public List<WriteNodeValue> TagsToWrite(int clientId)
        {
            List<WriteNodeValue> writeNodeValue = new List<WriteNodeValue>();

            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new MySqlCommand($"Call sp_getTagsToWrite({clientId});", con);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        writeNodeValue.Add(new WriteNodeValue
                        {
                            ChannelName = reader.GetString("ChannelName"),
                            DeviceName = reader.GetInt32("DeviceName"),
                            TagName = reader.GetString("TagName"),
                            TagValue = reader.GetDouble("Value"),
                        });
                    }
                    con.Close();

                }
            }
            catch (Exception)
            {

                throw;
            }
            return writeNodeValue;
        }
        public void SetClientState(int clientId, string clientState)
        {
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new MySqlCommand($"REPLACE INTO dbkepware.service (ClientId,Status) values ({clientId},'{clientState}');", con);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void InsertValues(string values)
        {
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new MySqlCommand(values, con);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
