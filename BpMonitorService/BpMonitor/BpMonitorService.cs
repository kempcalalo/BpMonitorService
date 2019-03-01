using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace BpMonitorService.BpMonitor
{
    public partial class BpMonitorService : ServiceBase
    {
        private static readonly Dictionary<string, string> Places = new Dictionary<string, string>
        {
            {"Molndal", "508A8CB4FBDC002E000345E7"},
            {"Gothenburg", "508A8CB406FE001F00030A60"},
            {"Harryda", "508A8CB4044A002300035C4C"},
            {"Partille", "508A8CB4FD6E00300003D3DD"}
        };


        public const string BaseUrl = @"https://nya.boplats.se/sok?objecttype=alla&area=";
        public const string SuffixUrl = @"&types=1hand&sortorder=startPublishTime-descending&listtype=imagelist&moveindate=any&images=YES";

        static Dictionary<string, ApartmentDetailsModel> _placesCount = new Dictionary<string, ApartmentDetailsModel>
        {
            {"Molndal", new ApartmentDetailsModel(){ LatestCount = 0, LatestName = null}},
            {"Gothenburg", new ApartmentDetailsModel(){ LatestCount = 0, LatestName = null}},
            {"Harryda", new ApartmentDetailsModel(){ LatestCount = 0, LatestName = null}},
            {"Partille", new ApartmentDetailsModel(){ LatestCount = 0, LatestName = null}}
        };

        private readonly System.Timers.Timer _timeDelay;
        private int _count = 1;
        public int Interval = int.Parse(ConfigurationManager.AppSettings["IntervalMilliSeconds"]);
        private static readonly string ToEmail = ConfigurationManager.AppSettings["ToEmail"];
        private static readonly string ToEmailName = ConfigurationManager.AppSettings["ToEmailName"];
        private static readonly string FromEmail = ConfigurationManager.AppSettings["FromEmail"];
        private static readonly string FromEmailName = ConfigurationManager.AppSettings["FromEmailName"];

        public BpMonitorService()
        {
            InitializeComponent();
            _timeDelay = new System.Timers.Timer();
            _timeDelay.Elapsed += WorkProcess;
            _timeDelay.Interval = Interval;
        }

        public void WorkProcess(object sender, System.Timers.ElapsedEventArgs e)
        {
            string process = $"Batch number: {_count}";
            _count++;
            
            try
            {
                Logger.WriteToLog($"{DateTime.Now}: {process}");
                CheckPlaces();
            }
            catch(Exception ex)
            {
                Mailer m = new Mailer();
                m.SendEmail(FromEmail, FromEmailName, $"Exception Occured in BP Monitor Service",
                        ToEmail, ToEmailName, null, ex.Message + ex.InnerException);
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
                m.SendEmail(FromEmail, FromEmailName, $"Exception Occured in BP Monitor Service",
                        ToEmail, ToEmailName, ex.Message, null);
                throw;
            }
        }
    

        protected override void OnStop()
        {
            Logger.WriteToLog($"{DateTime.Now}: Service Stopped.");
            _timeDelay.Enabled = false;
        }

        public static void CheckPlaces()
        {
            foreach (var place in Places)
            {
                var currentUrl = $"{BaseUrl}{place.Value}{SuffixUrl}";
                BoplatsHelper.CheckForNewApartment(currentUrl);
                if (_placesCount[place.Key].LatestCount != BoplatsHelper.LatestSearchCount && _placesCount[place.Key].LatestName != BoplatsHelper.LatestApartmentName)
                {
                    var sb = new StringBuilder();
          
                    sb.Append($"Previous Count: {_placesCount[place.Key].LatestCount} <br /> Latest Count: <strong> {BoplatsHelper.LatestSearchCount} </strong>");
                    sb.Append("<br />");
                    sb.Append($"Previous Apartment: {_placesCount[place.Key].LatestName} <br /> Latest Apartment: <strong> {BoplatsHelper.LatestApartmentName}</strong>");
                    sb.Append("<br /><br />");
                    sb.Append($"Please check {currentUrl}");

                    var mailer = new Mailer();

                    var isSuccessful = mailer.SendEmail(FromEmail, FromEmailName, $"Changes in {place.Key}'s apartment count",
                        ToEmail, ToEmailName, null, sb.ToString());

                    _placesCount[place.Key].LatestCount = BoplatsHelper.LatestSearchCount;
                    _placesCount[place.Key].LatestName= BoplatsHelper.LatestApartmentName;

                    if (!isSuccessful)
                        throw new Exception("Failed to send email");

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
