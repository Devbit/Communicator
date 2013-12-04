using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Communicator
{
    public class Processor
    {
        private RESTCommunicator rc;
        private int _currentPage = 10375;
        private const int Amount = 25;
        private const string BaseLink = "http://127.0.0.1:5000";
        private const string ProfileLink = "profiles";
        private const string VacatureLink = "vacatures";
        private const int BufferThreadDelay = 15000;
        private Thread _bufferThread = null;
        private bool _bufferThreadAlive = false;
        private readonly ConcurrentQueue<List<Profile>> _wbuffer = new ConcurrentQueue<List<Profile>>();
        private readonly ConcurrentQueue<List<Vacancy>> _vbuffer = new ConcurrentQueue<List<Vacancy>>();

        public Processor()
        {
            rc = new RESTCommunicator(BaseLink);
        }

        public Processor(string link)
        {
            rc = new RESTCommunicator(link);
        }

        public Processor(bool backgroundLoading)
        {
            rc = new RESTCommunicator(BaseLink);
            StartBackgroundBuffering(backgroundLoading);
        }

        public Processor(string link, bool backgroundLoading)
        {
            rc = new RESTCommunicator(link);
            StartBackgroundBuffering(backgroundLoading);
        }

        public void SetPage(int page)
        {
            _currentPage = page;
        }

        private void StartBackgroundBuffering(bool backgroundLoading)
        {
            if (backgroundLoading)
            {
                _bufferThread = new Thread(new ThreadStart(BackgroundBuffering));
                _bufferThreadAlive = true;
                _bufferThread.Start();
            }
        }

        private void BackgroundBuffering()
        {
            while (_bufferThreadAlive)
            {
                LoadNextProfileBuffer();
                Thread.Sleep(BufferThreadDelay);
            }
        }

        public void StopBackgroundBuffering()
        {
            _bufferThreadAlive = false;
        }

        public List<Profile> GetNextProfiles()
        {
            Console.WriteLine(_currentPage);
            if (_wbuffer.Count == 0)
            {
                LoadNextProfileBuffer();
            }
            if (_wbuffer.Count > 0)
            {
                List<Profile> result;
                _wbuffer.TryDequeue(out result);
                return result;
            }
            return new List<Profile>();
            
        }

        public bool HasNextProfiles()
        {
            return (_wbuffer.Count > 0 ? true : false);
        }

        public List<Vacancy> GetNextVacancies()
        {
            if (_vbuffer.Count == 0)
            {
                LoadNextVacancyBuffer();
            }
            if (_vbuffer.Count > 0)
            {
                List<Vacancy> result;
                _vbuffer.TryDequeue(out result);
                return result;
            }
            return new List<Vacancy>();
        }

        public bool HasNextVacancies()
        {
            return (_vbuffer.Count > 0 ? true : false);
        }

        private void LoadNextProfileBuffer()
        {
            List<Profile> wl = FetchProfiles(_currentPage, Amount);
            if (wl.Count == 0)
            {
                StopBackgroundBuffering();
                return;
            }
            _currentPage++;
            _wbuffer.Enqueue(wl);
        }

        private void LoadNextVacancyBuffer()
        {
            List<Vacancy> vl = FetchVacancies(1, 10000);
            if (vl.Count == 0)
            {
                return;
            }
            _vbuffer.Enqueue(vl);
        }

        private List<Vacancy> FetchVacancies(int begin, int amount)
        {
            List<Entity> r = rc.GetFromREST(VacatureLink, begin, amount);
            return ToVacancy(r);
        }

        private List<Profile> FetchProfiles(int begin, int amount)
        {
            List<Entity> r = rc.GetFromREST(ProfileLink, begin, amount);
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