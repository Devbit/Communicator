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
        private int _currentProfilePage = 1;
        private int _currentVacancyPage = 1;
        private int _amountProfile = 25;
        private int _amountVacancy = 25;
        private int _pageCountProfile;
        private int _pageCountVacancy;
        private const string BaseLink = "http://127.0.0.1:5000";
        private const string ProfileLink = "profiles";
        private const string VacancyLink = "vacatures";
        private const int BufferThreadDelay = 15000;
        private Thread _bufferThread = null;
        private bool _bufferThreadAlive = false;
        private readonly ConcurrentQueue<List<Profile>> _wbuffer = new ConcurrentQueue<List<Profile>>();
        private readonly ConcurrentQueue<List<Vacancy>> _vbuffer = new ConcurrentQueue<List<Vacancy>>();

        public Processor(string link = BaseLink, bool backgroundLoading = false)
        {
            rc = new RESTCommunicator(link);
            StartBackgroundBuffering(backgroundLoading);
        }

        public void SetProfilePage(int page)
        {
            _currentProfilePage = page;
        }

        public void SetVacancyPage(int page)
        {
            _currentVacancyPage = page;
        }

        public void SetProfileAmount(int amount)
        {
            _amountProfile = amount;
        }

        public void SetVacancyAmount(int amount)
        {
            _amountVacancy = amount;
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

        public int GetProfileEntryCount()
        {
            return rc.GetEntryCount(ProfileLink);
        }

        public int GetVacancyEntryCount()
        {
            return rc.GetEntryCount(VacancyLink);
        }

        public List<Profile> GetNextProfiles()
        {
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

        public List<Profile> GetProfiles(int page)
        {
            return FetchProfiles(page, _amountProfile);
        }

        public List<Vacancy> GetVacancies(int page)
        {
            return FetchVacancies(page, _amountVacancy);
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
            List<Profile> wl = FetchProfiles(_currentProfilePage, _amountProfile);
            if (wl.Count == 0)
            {
                StopBackgroundBuffering();
                return;
            }
            _currentProfilePage++;
            _wbuffer.Enqueue(wl);
        }

        private void LoadNextVacancyBuffer()
        {
            List<Vacancy> vl = FetchVacancies(_currentVacancyPage, _amountVacancy);
            if (vl.Count == 0)
            {
                return;
            }
            _currentVacancyPage++;
            _vbuffer.Enqueue(vl);
        }

        private List<Vacancy> FetchVacancies(int begin, int amount)
        {
            List<Entity> r = rc.GetFromREST(VacancyLink, begin, amount);
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