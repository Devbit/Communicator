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

        private RESTCommunicator() { }

        public RESTCommunicator(string baseURL)
        {
            this.baseURL = baseURL;
        }

        public List<Entity> GetFromREST(string link, int page = 1, int limit = 25)
        {
            if (link == "")
            {
                return new List<Entity>();
            }
            IRestResponse<JObject> response = MakeRequest(link, page, limit);

            List<Entity> results = ParseResponse(response.Data);
            return results;
        }

        public int GetEntryCount(string link)
        {
            if (link == "")
            {
                return 0;
            }
            IRestResponse<JObject> response = MakeRequest(link, 1, 1);
            return response.Data["meta"].Value<int>("total_count");
        }

        private List<Entity> ParseResponse(JObject response){
            List<Entity> result = new List<Entity>();
            List<JObject> t = JsonConvert.DeserializeObject<List<JObject>>(response["objects"].ToString());
            foreach (JObject k in t)
            {
                result.Add(new Entity(k));
            }

            return result;
        }

        private IRestResponse<JObject> MakeRequest(string link, int page, int limit)
        {
            RestClient client = new RestClient(baseURL);
            client.AddHandler("application/json", new DynamicJsonDeserializer());
            RestRequest request = new RestRequest(link);
            int offset = (page - 1) * limit;
            request.AddParameter("offset", offset);
            request.AddParameter("limit", limit);
            request.AddHeader("Accept", "application/json");
            //request.AddHeader("Authorization", Auth);
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

    internal class Meta
    {
        public int total_count { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
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