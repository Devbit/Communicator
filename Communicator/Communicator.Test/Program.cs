using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communicator;

namespace Communicator.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //RESTCommunicator db = new RESTCommunicator("http://127.0.0.1:5000");
            //db.GetFromREST("profiles", 1, 1);
            Processor pc = new Processor("http://127.0.0.1:29000");
            while (true)
            {
                Console.Clear();
                //Console.WriteLine(pc.GetProfileEntryCount());
                //PrintAll(pc);
                TryPostMatch(pc);
                Console.ReadKey();
            }

        }

        static void PrintAll(Processor pc)
        {
            foreach (Profile w in pc.GetNextProfiles())
            {
                Console.WriteLine(w.name.firstname);
            }
        }

        static void TryPostMatch(Processor pc)
        {
            List<Profile> profiles = pc.GetNextProfiles();
            List<Vacancy> vacancies = pc.GetNextVacancies();
            Match match = new Match();
            match.factors = new List<MatchFactor>();
            MatchFactor factor = new MatchFactor();
            factor.factor = "Test";
            factor.multiplier = 2;
            factor.strength = 80;
            factor.text = "Text here";
            match.factors.Add(factor);
            match.profile = profiles[0];
            match.vacancy = vacancies[0];
            match.strength = 50;
            pc.SaveMatch(match);
        }
    }
}
