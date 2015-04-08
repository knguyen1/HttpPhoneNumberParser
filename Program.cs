using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HttpPhoneNumberParser
{
    ///AUTHOR: KYLE NGUYEN
    ///DATE: 2015-03-13
    ///
    ///The purpose of this program is to look for a phone number from a company website.
    ///The program takes two inputs: company name, and the base url.
    ///First, we will visit the homepage and use regex to find all visitable links.
    ///We will then find the phone number on the homepage as well as all other pages using Regex.
    ///
    ///Then, we will hit the OpenCNAM API to verify the numbers.
    ///OpenCNAM is an API that lets you get the name associated with a phone number.
    ///IMPORTANT: YOU CAN ONLY MAKE 10 CALLS PER HOUR.
    ///docs here: https://www.opencnam.com/docs/v2/quickstart
    ///
    ///Finally, we compare the inputted company name and compare it to the name
    ///returned by OpenCNAM API using the Levenstein Edit Distance algorithm.

    class Program
    {
        //look for phone number matching 111-111-1111 or 111.111.1111
        const string phoneRegexPattern = @"[0-9]{3}[-.][0-9]{3}[-.][0-9]{4}";

        //opencnam base uri
        const string callerIdBaseUri = @"https://api.opencnam.com/v2/phone/";

        static void Main(string[] args)
        {
            //keep a running list of phone numbers and the OpenCNAM caller Id
            CallerIdDict phoneNumbers = new CallerIdDict();

            string baseUri = Settings1.Default.website;
            string companyName = Settings1.Default.company;

            if(args.Length > 0)
            {
                baseUri = args[0];
                companyName = args[1];
            }

            //declare html
            string homepageHtml = DownloadWebPage(baseUri);

            //get a collection of all visitable links
            HtmlDocument homepage = new HtmlDocument();
            homepage.LoadHtml(homepageHtml);

            //all nodes containing links
            IEnumerable<string> allLinks = homepage.DocumentNode.Descendants("a")
                .Select(a => a.GetAttributeValue("href", null))
                .Where(u => !String.IsNullOrEmpty(u) && u.Length > 1) //don't get empty url's or the # url
                .Distinct();

            Parallel.ForEach<string>(allLinks,
                new ParallelOptions { MaxDegreeOfParallelism = 3 },
                link =>
            {
                string html = DownloadWebPage(baseUri + link);

                //check for phone numbers on the front page
                Regex rx = new Regex(phoneRegexPattern, RegexOptions.IgnorePatternWhitespace);
                MatchCollection matches = rx.Matches(html);

                foreach (Match match in matches)
                {
                    //strip all non-number characters out of the match
                    string phoneNumber = CleanPhone(match.Value.ToString());

                    //check caller Id API for the name
                    string callerIdName = GetCallerIdName(phoneNumber);

                    //lower case everything
                    string callerIdLower = callerIdName.ToLower();
                    string companyLower = companyName.ToLower();

                    //fuzzy matching between inputed company name and what came back from the API
                    int lev = Levenshtein.EditDistance<Char>(companyLower.ToCharArray(), callerIdLower.ToCharArray());

                    //add to running list, phoneNumbers is a custom dictionary that
                    //takes in (string phoneNumber, string callerIdName, int levenshteinDistance)
                    phoneNumbers.Add(phoneNumber, callerIdName, lev);
                }
            });

            //find good matches, where Levenshtein distance is less than 4
            var results = from p in phoneNumbers
                          where p.Value < 4
                          select p;

            Console.WriteLine("You entered {0} with company name {1}.", baseUri, companyName);
            
            //count the number of results returned
            int resultsCount = results.Count();

            if (resultsCount > 0)
                Console.WriteLine("Found {0} numbers that might match your criteria.", resultsCount);
            else
                Console.WriteLine("Didn't find anything, sorry.  Check your regex pattern.");

            //print the results
            foreach(var result in results)
            {
                Console.WriteLine("NUMBER: {0} /// CALLER ID: {1} /// LEVENSHTEIN DISTANCE: {2}", result.Key.phoneNumber, result.Key.name, result.Value);
            }

            Console.WriteLine("\n\nGoodbye!  Press any key.");
            Console.ReadLine();
        }

        //takes in a Uri string and downloads the webpage, returning the HTML
        public static string DownloadWebPage(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            //always dispose of respources you've finished using
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, ASCIIEncoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        //strips all non-numeric character from a string
        public static string CleanPhone(string phone)
        {
            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(phone, String.Empty);
        }

        //calls the API
        public static string GetCallerIdName(string phone)
        {
            string uri = callerIdBaseUri + "+1" + phone;

            return DownloadWebPage(uri);
        }
    }
}
