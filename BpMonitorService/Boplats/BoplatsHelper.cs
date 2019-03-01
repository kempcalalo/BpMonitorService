using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BpMonitorService.Helpers;
using HtmlAgilityPack;

namespace BpMonitorService.Boplats
{
    public static class BoplatsHelper
    {
        private static readonly Dictionary<string, string> Places = new Dictionary<string, string>
        {
            {"Molndal", "508A8CB4FBDC002E000345E7"},
            {"Gothenburg", "508A8CB406FE001F00030A60"},
            {"Harryda", "508A8CB4044A002300035C4C"},
            {"Partille", "508A8CB4FD6E00300003D3DD"}
        };


        private const string BaseUrl = @"https://nya.boplats.se/sok?objecttype=alla&area=";
        private const string SuffixUrl = @"&types=1hand&sortorder=startPublishTime-descending&listtype=imagelist&moveindate=any&images=YES";

        static Dictionary<string, ApartmentDetailsModel> _placesCount = new Dictionary<string, ApartmentDetailsModel>
        {
            {"Molndal", new ApartmentDetailsModel(){ LatestCount = 0, LatestName = null}},
            {"Gothenburg", new ApartmentDetailsModel(){ LatestCount = 0, LatestName = null}},
            {"Harryda", new ApartmentDetailsModel(){ LatestCount = 0, LatestName = null}},
            {"Partille", new ApartmentDetailsModel(){ LatestCount = 0, LatestName = null}}
        };

        private static int LatestSearchCount { get; set; }
        private static string LatestApartmentName { get; set; }

        public static void CheckPlaces()
        {
            foreach (var place in Places)
            {
                var currentUrl = $"{BaseUrl}{place.Value}{SuffixUrl}";
                var htmlString = WebRequestHelper.GetHtmlString(currentUrl);
                SetLatestApartmentCountAndName(htmlString);

                if (_placesCount[place.Key].LatestCount != BoplatsHelper.LatestSearchCount && _placesCount[place.Key].LatestName != BoplatsHelper.LatestApartmentName)
                {
                    var sb = new StringBuilder();

                    sb.Append($"Previous Count: {_placesCount[place.Key].LatestCount} <br /> Latest Count: <strong> {BoplatsHelper.LatestSearchCount} </strong>");
                    sb.Append("<br />");
                    sb.Append($"Previous Apartment: {_placesCount[place.Key].LatestName} <br /> Latest Apartment: <strong> {BoplatsHelper.LatestApartmentName}</strong>");
                    sb.Append("<br /><br />");
                    sb.Append($"Please check {currentUrl}");

                    var mailer = new Mailer();

                    var isSuccessful = mailer.SendEmail(Settings.FromEmail, Settings.FromEmailName, $"Boplats - Changes in {place.Key}'s apartment count",
                        Settings.ToEmail, Settings.ToEmailName, null, sb.ToString());

                    _placesCount[place.Key].LatestCount = BoplatsHelper.LatestSearchCount;
                    _placesCount[place.Key].LatestName = BoplatsHelper.LatestApartmentName;

                    if (!isSuccessful)
                        throw new Exception("Failed to send email");

                }
            }
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
