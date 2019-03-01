using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BpMonitorService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

            if (!Environment.UserInteractive)
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new BostadWatchService()
                };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                string[] args = null;
                var bpm = new BostadWatchService();
                bpm.TestStartupAndStop(args);
            }

        }
    }
}
