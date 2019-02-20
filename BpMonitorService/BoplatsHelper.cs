using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BpMonitorService
{
    public static class BoplatsHelper
    {
        public static int LatestSearchCount { get; private set; }
        public static string LatestApartmentName { get; set; }


        public static void CheckForNewApartment(string url)
        {
            var urlAddress = url;
            string htmlString = null;

            var request = (HttpWebRequest)WebRequest.Create(urlAddress);
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK) return;

            var receiveStream = response.GetResponseStream();
            StreamReader readStream = null;

            readStream = response.CharacterSet == null ? new StreamReader(receiveStream) : new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

            htmlString = readStream.ReadToEnd();

            response.Close();
            readStream.Close();

            SetLatestSearchCount(htmlString);

        }

        private static void SetLatestSearchCount(string htmlString)
        {
            var htmlDoc = new HtmlDocument { OptionFixNestedTags = true };
            htmlDoc.LoadHtml(htmlString);
            // ParseErrors is an ArrayList containing any errors from the Load statement
            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
            {
                return;
            }

            if (htmlDoc.DocumentNode == null) return;
            var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");

            if (bodyNode == null) return;
            foreach (var node in bodyNode.SelectNodes("//span[@class='objectcount']"))
            {
                LatestSearchCount = int.Parse(node.InnerText);
            }
        }

    }
}
