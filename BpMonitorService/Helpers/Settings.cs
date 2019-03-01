
using System.Configuration;


namespace BpMonitorService.Helpers
{
    public static class Settings
    {
        public static int Interval => int.Parse(ConfigurationManager.AppSettings["IntervalMilliSeconds"]);
        public static string ToEmail => ConfigurationManager.AppSettings["ToEmail"];
        public static string ToEmailName => ConfigurationManager.AppSettings["ToEmailName"];
        public static string FromEmail => ConfigurationManager.AppSettings["FromEmail"];
        public static string FromEmailName => ConfigurationManager.AppSettings["FromEmailName"];

    }
}
