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
        private const string Auth = "Basic YWRtaW46dHpsVlEzRG4zd1Jwc3JONnNjY1U5b0ZvUENmS21B";
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

        public int GetPageCount(string link, int limit = 25)
        {
            if (link == "")
            {
                return 0;
            }
            IRestResponse<JObject> response = MakeRequest(link, 1, limit);
            JObject responseD = response.Data;
            Links links = new Links();
            links = JsonConvert.DeserializeObject<Links>(responseD["_links"].ToString());
            if (links.last != null)
            {
                Debug.WriteLine((string)links.last["href"]);
                Uri last = new Uri("http://" + (string)links.last["href"]);
                string amount = HttpUtility.ParseQueryString(last.Query).Get("page");
                return int.Parse(amount);
            }
            return 0;

        }

        private List<Entity> ParseResponse(JObject response){
            List<Entity> result = new List<Entity>();
            List<JObject> t = JsonConvert.DeserializeObject<List<JObject>>(response["_items"].ToString());
            foreach (JObject k in t)
            {
                result.Add(new Entity(k));
            }

            Links links = new Links();
            links = JsonConvert.DeserializeObject<Links>(response["_links"].ToString());
            if (links.next == null)
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
            request.AddHeader("Authorization", Auth);

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

    internal class Links
    {
        public JObject self { get; set; }
        public JObject prev { get; set; }
        public JObject next { get; set; }
        public JObject last { get; set; }
        public JObject parent { get; set; }
    }

    internal class InnerLink
    {
        public string href { get; set; }
        public string title { get; set; }
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