using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Process
{
    [Serializable]
    public class Process
    {
        public string Name { get; set; }
        public int ExecutionTime { get; set; }
        public int Priority { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }

        public Process(string name, int executionTime, int priority, double cpuUsage, double memoryUsage)
        {
            Name = name;
            ExecutionTime = executionTime;
            Priority = priority;
            CpuUsage = cpuUsage;
            MemoryUsage = memoryUsage;
        }

        public override string ToString()
        {
            return $"| Name: {Name, -10} | Execution time: {ExecutionTime + "s", -3} | Priority: {Priority, -2} " +
                $"| CPU usage: {CpuUsage + "%",-4} | Memory usage: {MemoryUsage + "%",-4} |";
        }
    }
}
