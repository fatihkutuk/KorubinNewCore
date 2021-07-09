using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace KorubinNewCore.Managers
{
    public class KepwareRestApiManager
    {
        private static readonly string baseUrl = "http://192.168.1.132:57412/config/v1/project/channels";
        private static readonly string cred = Convert.ToBase64String(Encoding.UTF8.GetBytes("administrator:"));
        private static readonly AuthenticationHeaderValue authenticator = new AuthenticationHeaderValue("Basic", cred);
        private static readonly MediaTypeWithQualityHeaderValue mediaTypeHeader = new MediaTypeWithQualityHeaderValue("application/json");
        public KepwareRestApiManager()
        {

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
    }
}
