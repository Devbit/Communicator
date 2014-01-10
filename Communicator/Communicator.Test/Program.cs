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
            Processor pc = new Processor("http://127.0.0.1:5000");
            while (true)
            {
                Console.Clear();
                Console.WriteLine(pc.GuessProfileEntryCount());
                //PrintAll(pc);
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
    }
}
