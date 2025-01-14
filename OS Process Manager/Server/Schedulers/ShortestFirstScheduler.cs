using ProcessOS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Schedulers
{
    public class ShortestFirstScheduler : IScheduler
    {
        private List<Process> processes;

        public ShortestFirstScheduler()
        {
            processes = new List<Process> ();
        }

        public void Add(Process process)
        {
            processes.Add(process);

            int i = processes.Count-1;
            while (i > 0) 
            {
                if (processes[i - 1].ExecutionTime <= processes[i].ExecutionTime)
                    break;

                Swap(processes, i - 1, i);
                i--;
            }
        }
        private void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }

        public void AddAll(List<Process> list)
        {
            foreach (Process process in list)
            {
                processes.Add(process);
            }

            processes.Sort((p1, p2) => p1.ExecutionTime.CompareTo(p2.ExecutionTime));
        }

        public List<Process> GetAll()
        {
            return processes;
        }

        public (Process next, int time) Next()
        {
            Process next = processes.First();

            processes.Remove(next);

            return (next, next.ExecutionTime);
        }

        public bool HasUnfinished()
        {
            return processes.Count > 0;
        }
    }
}
