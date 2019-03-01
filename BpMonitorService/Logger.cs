using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BpMonitorService
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
