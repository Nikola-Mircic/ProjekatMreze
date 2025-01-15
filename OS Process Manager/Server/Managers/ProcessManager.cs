using ProcessOS;
using Server.Schedulers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Managers
{
    internal class ProcessManager
    {
        public double totalCpuUsage { get; private set; }
        public double totalMemoryUsage { get; private set; }
        private IScheduler scheduler;
        private readonly Mutex mutex;

        public ProcessManager(IScheduler scheduler)
        {
            this.scheduler = scheduler;
            totalCpuUsage = 0;
            totalMemoryUsage = 0;
        }

        public bool Add(Process process)
        {
            lock (mutex)
            {
                if(totalCpuUsage + process.CpuUsage <= 100 && totalMemoryUsage + process.MemoryUsage <= 100)
                {
                    totalCpuUsage += process.CpuUsage;
                    totalMemoryUsage += process.MemoryUsage;
                    scheduler.Add(process);
                    return true;
                }

                return false;
            }
        }

        public void ExecuteNext()
        {
            Process process = null;
            int time = 0;
            lock (mutex)
            {
                (process, time) = scheduler.Next();
            }
            Console.WriteLine($"Executing : {process}");
            Thread.Sleep(time);
        }

        public string ActiveProcessesToString() // informaciju o aktivnim procesima za slanje preko udp-a
        {
            string processes = "";
            lock (mutex)
            {
                foreach (Process process in scheduler.GetAll())
                {
                    processes += process.ToString();
                }
            }
            return processes;
        }
    }
}
