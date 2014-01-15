using RestSharp;
using System.Collections.Generic;
using RestSharp.Deserializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using RestSharp.Contrib;
using System.Diagnostics;

namespace Communicator
{
    public class RESTCommunicator
    {
        private string baseURL = "";
        private int _lastPage;
        private bool _hasNextPage = true;

        private RESTCommunicator() { }

        public RESTCommunicator(string baseURL)
        {
            this.baseURL = baseURL;
            _lastPage = 0;
        }

        public bool HasNextPage() 
        {
            return _hasNextPage;
        }

        public List<Entity> GetFromREST(string link, int page = 1, int limit = 25)
        {
            if (link == "" || (!_hasNextPage && page == _lastPage))
            {
                return new List<Entity>();
            }
            IRestResponse<JObject> response = MakeRequest(link, page, limit);

            List<Entity> results = ParseResponse(response.Data);
            return results;
        }

        public void PostToREST(string json, string link)
        {
            if (link.Length == 0 || json.Length == 0)
            {
                return;
            }
            RestClient client = new RestClient(baseURL);
            RestRequest request = new RestRequest(link, Method.POST);
            request.AddParameter("application/json; charset=utf-8", json, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;

            var response = client.Execute(request);

            //Debug.WriteLine(response.Content);
        }

        public int GetEntryCount(string link)
        {
            IRestResponse<JObject> response = MakeRequest(link, 1, 1);
            return response.Data["meta"].Value<int>("total_count");
        }

        public int GetPageCount(string link, int limit = 25)
        {
            if (link == "")
            {
                return 0;
            }
            IRestResponse<JObject> response = MakeRequest(link, 1, limit);
            JObject responseD = response.Data;
            var last = responseD["_links"]["last"];
            if (last != null)
            {
                Uri lastUri = new Uri("http://" + baseURL + "/" + last.Value<string>("href"));
                string amount = HttpUtility.ParseQueryString(lastUri.Query).Get("page");
                return int.Parse(amount);
            }
            return 0;

        }

        private List<Entity> ParseResponse(JObject response)
        {
            List<Entity> result = new List<Entity>();
            List<JObject> t = JsonConvert.DeserializeObject<List<JObject>>(response["_items"].ToString());
            foreach (JObject k in t)
            {
                result.Add(new Entity(k));
            }

            if (response["_links"]["next"] == null)
            {
                _hasNextPage = false;
            }
            
            return result;
        }

        private IRestResponse<JObject> MakeRequest(string link, int page, int limit)
        {
            RestClient client = new RestClient(baseURL);
            client.AddHandler("application/json", new DynamicJsonDeserializer());
            RestRequest request = new RestRequest(link);

            request.AddParameter("page", page);
            request.AddParameter("max_results", limit);
            request.AddHeader("Accept", "application/json");

            IRestResponse<JObject> response = client.Execute<JObject>(request);
            return response;
        }
    }

    public class Entity
    {
        public JObject data { get; set; }

        public Entity(JObject j)
        {
            this.data = j;
        }
    }

    internal class DynamicJsonDeserializer : IDeserializer
    {
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
 
        public T Deserialize<T>(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<dynamic>(response.Content);
        }
    }
}