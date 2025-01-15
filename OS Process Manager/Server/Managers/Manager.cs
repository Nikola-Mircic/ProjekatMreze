using Server.Schedulers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Managers
{
    internal class Manager
    {
        private static ProcessManager processManager;

        public static bool Init(IScheduler scheduler)
        {
            if (processManager == null)
            {
                processManager = new ProcessManager(scheduler);
                return true;
            }

            return false;
        }

        public static ProcessManager Get()
        {
            return processManager;
        }
    }
}
