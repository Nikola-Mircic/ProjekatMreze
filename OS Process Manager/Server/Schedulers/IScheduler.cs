using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProcessOS;

namespace Server.Schedulers
{
    internal interface IScheduler
    {
        /*
            Funkcija za odabir sledeceg procesa.
            Vraca:
              - process koji treba da se izvrsava i 
              - vreme koje je dodeljeno tom procesu
         */
        (Process next, int time) Next();

        void Add(Process process);

        void AddAll(List<Process> list);
        List<Process> GetAll();
    }
}
