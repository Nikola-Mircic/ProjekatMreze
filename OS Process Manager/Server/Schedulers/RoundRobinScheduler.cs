using ProcessOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Schedulers
{
    internal class RoundRobinScheduler : IScheduler
    {
        private List<Process> processes;

        private readonly int period;
        private int nextProcess = -1;

        public RoundRobinScheduler(int period) 
        {
            this.processes = new List<Process>();
            this.period = period;
        }

        public void Add(Process process)
        {
            processes.Add(process);

            if(nextProcess == -1)
                nextProcess = 0;
        }

        public void AddAll(List<Process> list)
        {
            foreach (Process process in list)
                processes.Add(process);

            if (nextProcess == -1)
                nextProcess = 0;
        }

        public List<Process> GetAll()
        {
            return processes;
        }

        public (Process next, int time) Next()
        {
            Process next = processes[nextProcess].Clone() as Process;
            int time = Math.Min(period, next.ExecutionTime);

            processes[nextProcess].ExecutionTime -= time;

            if (processes[nextProcess].ExecutionTime == 0)
                processes.RemoveAt(nextProcess);

            nextProcess = (nextProcess + 1) % processes.Count;

            return (next, time);
        }
    }
}
