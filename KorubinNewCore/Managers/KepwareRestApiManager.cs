using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace KorubinNewCore.Managers
{
    public class KepwareRestApiManager
    {
        private static readonly string baseUrl = ConfigurationManager.ConnectionStrings["KepwareRestApiStr"].ConnectionString;
        private static readonly string cred = Convert.ToBase64String(Encoding.UTF8.GetBytes("administrator:"));
        private static readonly AuthenticationHeaderValue authenticator = new AuthenticationHeaderValue("Basic", cred);
        private static readonly MediaTypeWithQualityHeaderValue mediaTypeHeader = new MediaTypeWithQualityHeaderValue("application/json");
        public KepwareRestApiManager()
        {

        }
        public string ChannelPost(string jSon)
        {
            string _message = string.Empty;
            try
            {
                using (HttpClient cl = new HttpClient())
                {
                    cl.BaseAddress = new Uri(baseUrl);
                    cl.DefaultRequestHeaders.Authorization = authenticator;
                    cl.DefaultRequestHeaders.Accept.Add(mediaTypeHeader);

                    HttpContent content = new StringContent(jSon, Encoding.UTF8, "application/json");
                    HttpResponseMessage message = cl.PostAsync(baseUrl, content).Result;
                    Thread.Sleep(25);
                    _message = message.ReasonPhrase;
                }
            }
            catch (Exception ex)
            {
                _message = "FAILED";
            }
            return _message;
        }
        public string DevicePost(string jSon, string channelName)
        {
            //Debug.WriteLine($"{restHelper["KepwareUserName"]}:{restHelper["KepwarePassword"]}");
            JObject devJson = JObject.Parse(jSon);
            string _message = string.Empty;
            try
            {
                using (HttpClient cl = new HttpClient())
                {
                    cl.BaseAddress = new Uri(baseUrl + "/" + channelName + "/devices");
                    cl.DefaultRequestHeaders.Authorization = authenticator;
                    cl.DefaultRequestHeaders.Accept.Add(mediaTypeHeader);
                    string _tempurl = baseUrl + "/" + channelName + "/devices";
                    HttpContent content = new StringContent(jSon, Encoding.UTF8, "application/json");
                    HttpResponseMessage message = cl.PostAsync(_tempurl, content).Result;
                    string s = message.Content.ReadAsStringAsync().Result;
                    if (message.IsSuccessStatusCode)
                    {
                        _message = "Success";
                    }
                    else
                    {
                        if (s.Contains("Validation failed on property common.ALLTYPES_NAME"))
                        { _message = "Exist"; }
                        else
                        { _message = message.ReasonPhrase; }

                    }
                    Thread.Sleep(25);
                }
            }
            catch (Exception ex)
            {
                _message = "FAILED";
            }
            return _message;
        }
        public string TagPut(string jSon, string channelName, string deviceJson, string deviceName = "1")
        {
            string _message = string.Empty;

            var resDeviceDelete = DeviceDelete(channelName, deviceName);
            if(resDeviceDelete == "Success")
            {
                var resDeviceAdd = DevicePost(deviceJson,channelName);
                if (resDeviceAdd == "Success")
                {
                    try
                    {
                        using (HttpClient cl = new HttpClient())
                        {
                            cl.BaseAddress = new Uri(baseUrl);
                            cl.DefaultRequestHeaders.Authorization = authenticator;
                            cl.DefaultRequestHeaders.Accept.Add(mediaTypeHeader);
                            string _tempurl = baseUrl + "/" + channelName + "/devices/" + deviceName + "/tags/";
                            HttpContent content = new StringContent(jSon, Encoding.UTF8, "application/json");
                            HttpResponseMessage message = cl.PostAsync(_tempurl, content).Result;
                            Thread.Sleep(50);
                            _message = message.ReasonPhrase;
                        }
                    }
                    catch (Exception ex)
                    {
                        _message = "FAILED";
                    }

                }
                else
                {
                    _message = "FAILED";
                }
            }
            else
            {
                _message = "FAILED";

            }

            return _message;
        }
        public string TagPost(string jSon, string channelName, string tagName, string deviceName = "1")
        {
            string _message = string.Empty;
            using (HttpClient cl = new HttpClient())
            {
                try
                {
                    cl.BaseAddress = new Uri(baseUrl);
                    cl.DefaultRequestHeaders.Authorization = authenticator;
                    cl.DefaultRequestHeaders.Accept.Add(mediaTypeHeader);
                    string _tempurl = baseUrl + "/" + channelName + "/devices/" + deviceName + "/tags/" + tagName;
                    HttpContent content = new StringContent(jSon, Encoding.UTF8, "application/json");
                    HttpResponseMessage message = cl.PutAsync(_tempurl, content).Result;
                    if (message.IsSuccessStatusCode)
                    {
                        string s = message.Content.ReadAsStringAsync().Result;
                        if (s != string.Empty)
                        {
                            _message = s;
                        }
                    }
                    else
                    {
                        _message = message.ReasonPhrase;
                    }
                }
                catch (Exception ex)
                {
                    _message = "FAILED";
                }
            }
            return _message;
        }
        public string ChannelPut(string jSon, string channelName)
        {
            string _message = string.Empty;
            using (HttpClient cl = new HttpClient())
            {
                try
                {
                    cl.BaseAddress = new Uri(baseUrl);
                    cl.DefaultRequestHeaders.Authorization = authenticator;
                    cl.DefaultRequestHeaders.Accept.Add(mediaTypeHeader);
                    string _tempurl = baseUrl + "/" + channelName;
                    JObject channelJson = JObject.Parse(jSon);
                    channelJson.Add("FORCE_UPDATE", JToken.Parse("true"));
                    HttpContent content = new StringContent(channelJson.ToString(), Encoding.UTF8, "application/json");
                    HttpResponseMessage message = cl.PutAsync(_tempurl, content).Result;
                    Thread.Sleep(50);
                    if (message.IsSuccessStatusCode)
                    {
                        string s = message.Content.ReadAsStringAsync().Result;
                        if (s != string.Empty)
                        {
                            _message = s;
                        }
                    }
                    else
                    {
                        _message = message.ReasonPhrase;
                    }
                }
                catch (Exception ex)
                {
                    _message = "FAILED";
                }
            }
            return _message;
        }
        public string DevicePut(string jSon, string channelName, string deviceName = "1")
        {
            string _message = string.Empty;
            using (HttpClient cl = new HttpClient())
            {
                try
                {
                    cl.BaseAddress = new Uri(baseUrl);
                    cl.DefaultRequestHeaders.Authorization = authenticator;
                    cl.DefaultRequestHeaders.Accept.Add(mediaTypeHeader);
                    string _tempurl = baseUrl + "/" + channelName + "/devices/" + deviceName;
                    JObject deviceJson = JObject.Parse(jSon);
                    deviceJson.Add("FORCE_UPDATE", JToken.Parse("true"));
                    HttpContent content = new StringContent(deviceJson.ToString(), Encoding.UTF8, "application/json");
                    HttpResponseMessage message = cl.PutAsync(_tempurl, content).Result;
                    Thread.Sleep(50);
                    if (message.IsSuccessStatusCode)
                    {
                        string s = message.Content.ReadAsStringAsync().Result;
                        if (s != string.Empty)
                        {
                            _message = "Success";
                        }
                        else
                        {
                            _message = "Success";
                        }
                    }
                    else
                    {
                        _message = message.ReasonPhrase;
                    }
                }
                catch (Exception ex)
                {
                    _message = "FAILED";
                }
            }
            return _message;
        }
        public string DeviceDelete(string channelName, string deviceName)
        {
            string _message = string.Empty;
            using (HttpClient cl = new HttpClient())
            {
                try
                {
                    cl.BaseAddress = new Uri(baseUrl);
                    cl.DefaultRequestHeaders.Authorization = authenticator;
                    cl.DefaultRequestHeaders.Accept.Add(mediaTypeHeader);
                    string _tempurl = baseUrl + "/" + channelName + "/devices/" + deviceName;
                    HttpResponseMessage message = cl.DeleteAsync(_tempurl).Result;
                    Thread.Sleep(300);
                    //string s = message.Content.ReadAsStringAsync().Result;
                    if (message.IsSuccessStatusCode)
                    {
                        _message = "Success";
                    }
                    else
                    {
                        _message = message.ReasonPhrase;
                    }
                }
                catch (Exception ex)
                {
                    _message = "FAILED";
                }
            }
            return _message;
        }
    }
}
