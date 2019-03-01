using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceProcess;
using System.Text;
using BpMonitorService.Boplats;
using BpMonitorService.Helpers;

namespace BpMonitorService
{
    public partial class BostadWatchService : ServiceBase
    {

        private readonly System.Timers.Timer _timeDelay;
        private int _count = 1;


        public BostadWatchService()
        {
            InitializeComponent();
            _timeDelay = new System.Timers.Timer();
            _timeDelay.Elapsed += WorkProcess;
            _timeDelay.Interval = Settings.Interval;
        }

        public void WorkProcess(object sender, System.Timers.ElapsedEventArgs e)
        {
            string process = $"Batch number: {_count}";
            _count++;
            
            try
            {
                Logger.WriteToLog($"{DateTime.Now}: {process}");
                BoplatsHelper.CheckPlaces();
            }
            catch(Exception ex)
            {
                Mailer m = new Mailer();
                m.SendEmail(Settings.FromEmail, Settings.FromEmailName, $"Exception Occured in Bostad Watch Service",
                    Settings.ToEmail, Settings.ToEmailName, null, ex.ToString());
                Logger.WriteToLog(ex.ToString());

            }
            
            
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Logger.WriteToLog($"{DateTime.Now}: Service Started.");
                _timeDelay.Enabled = true;
            }
            catch(Exception ex)
            {
                Mailer m = new Mailer();
                m.SendEmail(Settings.FromEmail, Settings.FromEmailName, $"Exception Occured in Bostad Watch Service",
                    Settings.ToEmail, Settings.ToEmailName, null, ex.ToString());
                throw;
            }
        }
    

        protected override void OnStop()
        {
            Logger.WriteToLog($"{DateTime.Now}: Service Stopped.");
            _timeDelay.Enabled = false;
        }


        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }
    }
}
