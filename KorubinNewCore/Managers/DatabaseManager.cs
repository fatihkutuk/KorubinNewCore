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
                    var cmd = new MySqlCommand("Select cd.clientId from channeldevice cd where cd.clientId = 1 group by cd.clientId;", con);
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
        public HashSet<StatusChangedDevice> GetClientStatusChanged(int clientId)
        {
            HashSet<StatusChangedDevice> statusChangedDevice = new HashSet<StatusChangedDevice>();
            try
            {
                using (var con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new MySqlCommand($"select cd.clientId, cd.id, cd.channelJson, cd.deviceJson from dbkepware.channeldevice cd where find_in_set(cd.statusCode, '60,50,40,30,20,10') and cd.clientId = {clientId}", con);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        statusChangedDevice.Add(new StatusChangedDevice
                        {
                        
                            DeviceName = reader.GetInt32("id"),
                            ChannelJson = reader.GetString("channelJson"),
                            DeviceJson = reader.GetString("deviceJson")
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
        public HashSet<Node> GetNodes(int clientId)
        {
            HashSet<Node> nodes = new HashSet<Node>();
            try
            {
                using (var con = new MySqlConnection(connectionString)) 
                {
                    con.Open();
                    var cmd = new MySqlCommand($"Call sp_getClientSubscriptionList({clientId});",con);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        nodes.Add(new Node { 
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
        public void SetClientState(int clientId,string clientState)
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
                    var cmd = new MySqlCommand(values,con);
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
