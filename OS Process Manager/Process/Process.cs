using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProcessOS
{
    [Serializable]
    public class Process : ICloneable
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
            ExecutionTime = rand.Next(1000, 5000);
            Priority = rand.Next(0,3);
            CpuUsage = rand.NextDouble() * rand.Next(10, 80);
            MemoryUsage = rand.NextDouble() * rand.Next(10, 80);
        }

        private Process(string name, int execTime, int priority, double cpu, double mem)
        {
            this.Name = name;
            this.ExecutionTime = execTime;
            this.Priority = priority;
            this.CpuUsage = cpu;
            this.MemoryUsage = mem;
        }

        public override string ToString()
        {
            return $"| Name: {Name, -15} | Execution time: {ExecutionTime + "ms", -7} | Priority: {Priority, -2} " +
                $"| CPU usage: {CpuUsage.ToString("F2") + "%",-6} | Memory usage: {MemoryUsage.ToString("F2") + "%",-6} |";
        }

        public object Clone()
        {
            return new Process(Name.Clone() as string, ExecutionTime, Priority, CpuUsage, MemoryUsage);
        }

        public static byte[] Serialize(Process process)
        {
            byte[] buffer = new byte[1024];

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, process);
                buffer = ms.ToArray();
            }

            return buffer;
        }

        public static Process Deserialize(byte[] buffer)
        {
            Process process = null;
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                BinaryFormatter bf = new BinaryFormatter();
                process = bf.Deserialize(ms) as Process;
            }

            return process;
        }
    }
}
