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

        private double maxCpuUsage = 0;
        private double maxMemUsage = 0;
        private Process minExecProcess = null;

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
                    if(totalCpuUsage > maxCpuUsage)
                    {
                        maxCpuUsage = totalCpuUsage;
                    }
                    if(totalMemoryUsage > maxMemUsage)
                    {
                        maxMemUsage = totalMemoryUsage;
                    }
                    if(minExecProcess == null || (minExecProcess != null && minExecProcess.ExecutionTime > process.ExecutionTime))
                    {
                        minExecProcess = new Process(process);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[Error]:\n{ex.Message}");
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
                catch(Exception ex)
                {
                    Console.WriteLine($"[Error]:\n{ex.Message}");
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
                Console.WriteLine($"[Executing]:\n{process}");
                await Task.Delay(time * 1000);
                if (process.ExecutionTime - time == 0)
                {
                    process.ExecutionTime = 0;
                    Console.WriteLine($"[Completed]:\n{process}");
                }
            }
            running = false;
        }

        public void ServerInfo()
        {
            Console.WriteLine("[ServerInfo]:");
            Console.WriteLine($"Max CPU usage: {maxCpuUsage.ToString("F2")}%");
            Console.WriteLine($"Max Memory usage: {maxMemUsage.ToString("F2")}%");
            if(minExecProcess != null)
            {
                Console.WriteLine("Process with min execution time:");
                Console.WriteLine(minExecProcess);
            }
        }
    }
}
