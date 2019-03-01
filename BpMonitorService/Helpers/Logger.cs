using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace BpMonitorService.Helpers
{
    public static class Logger
    {
        public static void WriteToLog(string content)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
            var fs = new FileStream($@"c:\temp\logs\BpMonitorLogs-{DateTime.Now.ToShortDateString()}.txt", FileMode.OpenOrCreate, FileAccess.Write);
            var sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(content);
            sw.Flush();
            sw.Close();
        }
    }
}
