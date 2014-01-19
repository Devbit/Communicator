using System.Security.Cryptography;
using System.Text;
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
        private int _currentMatchPage = 1;
        private int _amountProfile = 25;
        private int _amountVacancy = 25;
        private int _amountMatch = 25;
        private int _pageCountProfile;
        private int _pageCountVacancy;
        private int _pageCountMatch;
        private const string BaseLink = "http://127.0.0.1:29000";
        private const string ProfileLink = "profiles";
        private const string VacancyLink = "vacatures";
        private const string MatchLink = "matches";
        private int _bufferThreadDelay = 15000;
        private Thread _bufferThread = null;
        private bool _bufferThreadAlive = false;
        private readonly ConcurrentQueue<List<Profile>> _wbuffer = new ConcurrentQueue<List<Profile>>();
        private readonly ConcurrentQueue<List<Vacancy>> _vbuffer = new ConcurrentQueue<List<Vacancy>>();
        private readonly ConcurrentQueue<List<JsonMatch>> _mbuffer = new ConcurrentQueue<List<JsonMatch>>();

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

        public void SetMatchPage(int page)
        {
            _currentMatchPage = page;
        }

        public void SetProfileAmount(int amount)
        {
            _amountProfile = amount;
        }

        public void SetVacancyAmount(int amount)
        {
            _amountVacancy = amount;
        }

        public void SetMatchAmount(int amount)
        {
            _amountMatch = amount;
        }

        public void SetBackgroundBufferDelay(int delay)
        {
            _bufferThreadDelay = delay;
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
                Thread.Sleep(_bufferThreadDelay);
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

        public int GetMatchEntryCount()
        {
            return rc.GetEntryCount(MatchLink);
        }

        public int GetProfilePageCount()
        {
            return rc.GetPageCount(ProfileLink, _amountProfile);
        }

        public int GetVacancyPageCount()
        {
            return rc.GetPageCount(VacancyLink, _amountVacancy);
        }

        public int GetMatchPageCount()
        {
            return rc.GetPageCount(MatchLink, _amountMatch);
        }

        public bool InsertDocument(string json, string collectionLink)
        {
            if (json.Length == 0 || collectionLink.Length == 0)
            {
                return false;
            }

            return rc.PostToREST(json, collectionLink);
        }

        public List<Profile> GetProfiles(int page, int amount = _amountProfile, string sort = "", string filter = "")
        {
            return FetchProfiles(page, amount, sort, filter);
        }

        public List<Vacancy> GetVacancies(int page, int amount = _amountVacancy, string sort = "", string filter = "")
        {
            return FetchVacancies(page, amount, sort, filter);
        }

        public List<JsonMatch> GetMatches(int page, int amount = _amountMatch, string sort = "", string filter = "")
        {
            return FetchMatches(page, amount, sort, filter);
        }

        public bool HasNextProfiles()
        {
            return (_wbuffer.Count > 0 ? true : false);
        }

        public bool HasNextVacancies()
        {
            return (_vbuffer.Count > 0 ? true : false);
        }

        public bool HasNextMatches()
        {
            return (_mbuffer.Count > 0 ? true : false);
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

        public List<JsonMatch> GetNextMatches()
        {
            if (_mbuffer.Count == 0)
            {
                LoadNextMatchBuffer();
            }
            if (_mbuffer.Count > 0)
            {
                List<JsonMatch> result;
                _mbuffer.TryDequeue(out result);
                return result;
            }
            return new List<JsonMatch>();
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

        private void LoadNextMatchBuffer()
        {
            List<JsonMatch> ml = FetchMatches(_currentMatchPage, _amountMatch);
            if (ml.Count == 0)
            {
                return;
            }
            _currentMatchPage++;
            _mbuffer.Enqueue(ml);
        }

        private List<Profile> FetchProfiles(int begin, int amount, string sort, string filter)
        {
            List<Entity> r = rc.GetFromREST(ProfileLink, begin, amount, sort, filter);
            return ToProfile(r);
        }

        private List<Vacancy> FetchVacancies(int begin, int amount, string sort, string filter)
        {
            List<Entity> r = rc.GetFromREST(VacancyLink, begin, amount, sort, filter);
            return ToVacancy(r);
        }

        private List<JsonMatch> FetchMatches(int begin, int amount, string sort, string filter)
        {
            List<Entity> r = rc.GetFromREST(MatchLink, begin, amount, sort, filter);
            return ToMatch(r);
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

        private List<JsonMatch> ToMatch(List<Entity> result)
        {
            List<JsonMatch> ws = new List<JsonMatch>();
            foreach (Entity j in result)
            {
                JsonMatch w = JsonConvert.DeserializeObject<JsonMatch>(j.data.ToString());
                ws.Add(w);
            }
            return ws;
        }
    }
}