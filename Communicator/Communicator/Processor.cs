using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Communicator
{
    public class Processor
    {
        private RESTCommunicator rc;
        private int currentPage = 10375;
        private int AMOUNT = 25;
        private string BASE_LINK = "http://127.0.0.1:5000";
        private string PROFILE_LINK = "profiles";
        private string VACATURE_LINK = "vacatures";
        private Thread buffer_thread = null;
        private int buffer_thread_delay = 3000;
        private bool buffer_thread_alive = false;
        private ConcurrentQueue<List<Profile>> wbuffer = new ConcurrentQueue<List<Profile>>();
        private ConcurrentQueue<List<Vacancy>> vbuffer = new ConcurrentQueue<List<Vacancy>>();

        public Processor()
        {
            rc = new RESTCommunicator(BASE_LINK);
        }

        public Processor(string link)
        {
            rc = new RESTCommunicator(link);
        }

        public Processor(bool backgroundLoading)
        {
            rc = new RESTCommunicator(BASE_LINK);
            StartBackgroundBuffering(backgroundLoading);
        }

        public Processor(string link, bool backgroundLoading)
        {
            rc = new RESTCommunicator(link);
            StartBackgroundBuffering(backgroundLoading);
        }

        public void SetPage(int page)
        {
            currentPage = page;
        }

        private void StartBackgroundBuffering(bool backgroundLoading)
        {
            if (backgroundLoading)
            {
                buffer_thread = new Thread(new ThreadStart(BackgroundBuffering));
                buffer_thread_alive = true;
                buffer_thread.Start();
            }
        }

        private void BackgroundBuffering()
        {
            while (buffer_thread_alive)
            {
                LoadNextProfileBuffer();
                Thread.Sleep(buffer_thread_delay);
            }
        }

        public void StopBackgroundBuffering()
        {
            buffer_thread_alive = false;
        }

        public List<Profile> GetNextProfiles()
        {
            Console.WriteLine(currentPage);
            if (wbuffer.Count == 0)
            {
                LoadNextProfileBuffer();
            }
            if (wbuffer.Count > 0)
            {
                List<Profile> result;
                wbuffer.TryDequeue(out result);
                return result;
            }
            return new List<Profile>();
            
        }

        public bool HasNextProfiles()
        {
            return (wbuffer.Count > 0 ? true : false);
        }

        public List<Vacancy> GetNextVacancies()
        {
            if (vbuffer.Count == 0)
            {
                LoadNextVacancyBuffer();
            }
            if (vbuffer.Count > 0)
            {
                List<Vacancy> result;
                vbuffer.TryDequeue(out result);
                return result;
            }
            return new List<Vacancy>();
        }

        public bool HasNextVacancies()
        {
            return (vbuffer.Count > 0 ? true : false);
        }

        private void LoadNextProfileBuffer()
        {
            List<Profile> wl = FetchProfiles(currentPage, AMOUNT);
            if (wl.Count == 0)
            {
                StopBackgroundBuffering();
                return;
            }
            currentPage++;
            wbuffer.Enqueue(wl);
        }

        private void LoadNextVacancyBuffer()
        {
            List<Vacancy> vl = FetchVacancies(1, 10000);
            if (vl.Count == 0)
            {
                return;
            }
            vbuffer.Enqueue(vl);
        }

        private List<Vacancy> FetchVacancies(int begin, int amount)
        {
            List<Entity> r = rc.GetFromREST(VACATURE_LINK, begin, amount);
            return ToVacancy(r);
        }

        private List<Profile> FetchProfiles(int begin, int amount)
        {
            List<Entity> r = rc.GetFromREST(PROFILE_LINK, begin, amount);
            return ToProfile(r);
        }

        private List<Profile> ToProfile(List<Entity> result)
        {
            List<Profile> ws = new List<Profile>();
            foreach (Entity j in result)
            {
                Profile w = JsonConvert.DeserializeObject<Profile>(j.data.ToString());
                ws.Add(w);
            }
            return ws;
        }

        private List<Vacancy> ToVacancy(List<Entity> result)
        {
            List<Vacancy> ws = new List<Vacancy>();
            foreach (Entity j in result)
            {
                Vacancy w = JsonConvert.DeserializeObject<Vacancy>(j.data.ToString());
                ws.Add(w);
            }
            return ws;
        }
    }
}