using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProcessOS
{
    [Serializable]
    public class Process
    {
        private static Random rand = new Random();
        public string Name { get; set; }
        public int ExecutionTime { get; set; }
        public int Priority { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }

        public Process(string name)
        {
            Name = name;
            ExecutionTime = rand.Next(1, 5);
            Priority = rand.Next(0,3);
            CpuUsage = rand.NextDouble() * 100;
            MemoryUsage = rand.NextDouble() * 100;
        }

        public override string ToString()
        {
            return $"| Name: {Name, -10} | Execution time: {ExecutionTime + "s", -3} | Priority: {Priority, -2} " +
                $"| CPU usage: {CpuUsage.ToString("F2") + "%",-4} | Memory usage: {MemoryUsage.ToString("F2") + "%",-4} |";
        }
    }
}
