
using System.IO;
using System.Net;
using System.Text;


namespace BpMonitorService.Helpers
{
    public static class WebRequestHelper
    {
        public static string GetHtmlString(string url)
        {
            var urlAddress = url;
            string htmlString = null;

            var request = (HttpWebRequest)WebRequest.Create(urlAddress);
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK) return null;

            var receiveStream = response.GetResponseStream();
            StreamReader readStream = null;

            readStream = response.CharacterSet == null ? new StreamReader(receiveStream) : new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

            htmlString = readStream.ReadToEnd();

            response.Close();
            readStream.Close();

            return htmlString;

        }


    }
}
