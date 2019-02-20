using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
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

        public BpMonitorService()
        {
            InitializeComponent();
            timeDelay = new System.Timers.Timer();
            timeDelay.Elapsed += new System.Timers.ElapsedEventHandler(WorkProcess);
            timeDelay.Interval = _interval;
        }

        public void WorkProcess(object sender, System.Timers.ElapsedEventArgs e)
        {
            string process = "Batch number: " + _count;
            CheckPlaces();
            _count++;
            Console.WriteLine(process);
        }

        protected override void OnStart(string[] args)
        {
            timeDelay.Enabled = true;
        }
    

        protected override void OnStop()
        {
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
                    sb.AppendLine();
                    sb.Append($"Old Count: {_placesCount[place.Key]} New Count: BoplatsHelper.LatestSearchCount");
                    var toEmail = ConfigurationManager.AppSettings["ToEmail"];
                    var toEmailName = ConfigurationManager.AppSettings["ToEmailName"];

                    var mailer = new Mailer();
                    mailer.SendEmail("boplats@noreply.com", "Boplats Monitor", $"Changes in {place.Key}'s apartment count",
                        toEmail, toEmailName,sb.ToString());

                    _placesCount[place.Key] = BoplatsHelper.LatestSearchCount;
                }
            }
        }
        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }
    }
}
