using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace BpMonitorService.BpMonitor
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

            SetLatestApartmentCountAndName(htmlString);

        }

        private static void SetLatestApartmentCountAndName(string htmlString)
        {
            var htmlDoc = new HtmlDocument { OptionFixNestedTags = true };
            htmlDoc.LoadHtml(htmlString);
            // ParseErrors is an ArrayList containing any errors from the Load statement
            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
            {
                return;
            }

            var bodyNode = htmlDoc.DocumentNode?.SelectSingleNode("//body");

            if (bodyNode == null) return;

            //Set the latest count
            foreach (var node in bodyNode.SelectNodes("//span[@class='objectcount']"))
            {
                LatestSearchCount = int.Parse(node.InnerText);
            }

            //Set the latest name
            LatestApartmentName = bodyNode.SelectSingleNode("//p[@class='address']")?.InnerHtml;

        }

    }
}
