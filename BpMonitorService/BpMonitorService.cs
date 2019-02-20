using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BpMonitorService
{
    public partial class BpMonitorService : ServiceBase
    {
        static Dictionary<string, string> _places = new Dictionary<string, string>
        {
            {"Molndal", "508A8CB4FBDC002E000345E7"},
            {"Gothenburg", "508A8CB406FE001F00030A60"},
            {"Harryda", "508A8CB4044A002300035C4C"},
            {"Partille", "508A8CB4FD6E00300003D3DD"}
        };

        public const string BaseUrl = @"https://nya.boplats.se/sok?objecttype=alla&area=";
        public const string SuffixUrl = @"&types=1hand&sortorder=startPublishTime-descending&listtype=imagelist&moveindate=any&images=YES";

        static Dictionary<string, int> _placesCount = new Dictionary<string, int>
        {
            {"Molndal", 0},
            {"Gothenburg", 0},
            {"Harryda", 0},
            {"Partille", 0}
        };

        readonly System.Timers.Timer timeDelay;
        int _count;
        public int _interval = int.Parse(ConfigurationManager.AppSettings["IntervalMilliSeconds"].ToString());
        static string toEmail = ConfigurationManager.AppSettings["ToEmail"];
        static string toEmailName = ConfigurationManager.AppSettings["ToEmailName"];
        static string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
        static string fromEmailName = ConfigurationManager.AppSettings["FromEmailName"];

        public BpMonitorService()
        {
            InitializeComponent();
            timeDelay = new System.Timers.Timer();
            timeDelay.Elapsed += new System.Timers.ElapsedEventHandler(WorkProcess);
            timeDelay.Interval = _interval;
        }

        public void WorkProcess(object sender, System.Timers.ElapsedEventArgs e)
        {
            string process = $"Batch number: {_count}";
            _count++;
            
            try
            {
                LogService($"{DateTime.Now}: {process}");
                CheckPlaces();
            }
            catch(Exception ex)
            {
                Mailer m = new Mailer();
                m.SendEmail(fromEmail, fromEmailName, $"Exception Occured in BP Monitor Service",
                        toEmail, toEmailName, "eom", null);
                LogService(ex.Message + ex.InnerException.StackTrace);

            }
            
            
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                LogService($"{DateTime.Now}: Service Started.");
                timeDelay.Enabled = true;
            }
            catch(Exception ex)
            {
                Mailer m = new Mailer();
                m.SendEmail(fromEmail, fromEmailName, $"Exception Occured in BP Monitor Service",
                        toEmail, toEmailName, ex.Message, null);
                throw;
            }
        }
    

        protected override void OnStop()
        {
            LogService($"{DateTime.Now}: Service Stopped.");
            timeDelay.Enabled = false;
        }

        public static void CheckPlaces()
        {
            foreach (var place in _places)
            {
                var currentUrl = $"{BaseUrl}{place.Value}{SuffixUrl}";
                BoplatsHelper.CheckForNewApartment(currentUrl);
                if (_placesCount[place.Key] != BoplatsHelper.LatestSearchCount)
                {
                    var sb = new StringBuilder();

                    sb.Append($"Please check {currentUrl}");
                    sb.Append("<br /><br />");
                    sb.Append($"Old Count: {_placesCount[place.Key]} <br /> New Count: {BoplatsHelper.LatestSearchCount}");


                    var mailer = new Mailer();

                    var isSuccessful = mailer.SendEmail(fromEmail, fromEmailName, $"Changes in {place.Key}'s apartment count",
                        toEmail, toEmailName, null, sb.ToString());

                    _placesCount[place.Key] = BoplatsHelper.LatestSearchCount;
                }
            }
        }

        private void LogService(string content)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
            var fs = new FileStream($@"c:\temp\logs\BpMonitorLogs-{DateTime.Now.ToShortDateString()}.txt", FileMode.OpenOrCreate, FileAccess.Write);
            var sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(content);
            sw.Flush();
            sw.Close();
        }

        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }
    }
}
