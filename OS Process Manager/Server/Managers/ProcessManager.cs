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
        private static readonly Mutex mutex = new Mutex();
        private bool running;

        public ProcessManager(IScheduler scheduler)
        {
            this.scheduler = scheduler;
            totalCpuUsage = 0;
            totalMemoryUsage = 0;
            running = false;
        }

        public bool Add(Process process)
        {
            bool success = false;
            try 
            {
                mutex.WaitOne();

                if (totalCpuUsage + process.CpuUsage <= 100 && totalMemoryUsage + process.MemoryUsage <= 100)
                {
                    totalCpuUsage += process.CpuUsage;
                    totalMemoryUsage += process.MemoryUsage;
                    scheduler.Add(process);
                    success = true;
                }
            }
            catch
            {
                Console.WriteLine("Error");
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            if (!running && success) _ = ExecuteNext();
            return success;
        }

        public async Task ExecuteNext()
        {
            running = true;
            Process process = null;
            int time = 0;
            while (scheduler.HasUnfinished())
            {
                try
                {
                    mutex.WaitOne();

                    (process, time) = scheduler.Next();
                    if(process.ExecutionTime - time == 0)
                    {
                        totalCpuUsage -= process.CpuUsage;
                        totalMemoryUsage -= process.MemoryUsage;
                    }
                }
                catch
                {
                    Console.WriteLine("Error");
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
                Console.WriteLine($"Executing : {process}");
                await Task.Delay(time);
            }
            running = false;
        }

        public string ActiveProcessesToString() // informaciju o aktivnim procesima za slanje preko udp-a
        {
            string processes = "";
            try
            {
                mutex.WaitOne();
                foreach (Process process in scheduler.GetAll())
                {
                    processes += process.ToString();
                }
            }
            catch
            {
                Console.WriteLine("Error");
            }
            finally
            {
                mutex.ReleaseMutex(); 
            }
            return processes;
        }
    }
}
