using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace CHStreamingAPI
{
    class Program
    {

        private static readonly CancellationTokenSource canToken = new CancellationTokenSource();

        static async Task Main(string[] args)
        {

            int timepoint = 0;
            bool test = true;

            //Set ability to stop through Ctrl-C in command line
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine("Cancel event triggered");
                canToken.Cancel();
                eventArgs.Cancel = true;
            };

            if (args.Length != 0)
            {
                test = int.TryParse(args[0], out timepoint);
            }

            //Capture non integer timepoint in command line arguments
            if (test)
            {

                Console.WriteLine("Application has started. Ctrl-C to end");
                await ReadStreamingAPI(timepoint);

                //On handled errr or exit
                Console.WriteLine("Now shutting down");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Enter numeric command line argument or leave empty for future events");
                Console.ReadLine();
            }

        }

        async static Task ReadStreamingAPI(int timepoint)
        {

            using HttpClient httpClient = new HttpClient();

            //Set authorization header & indefinite time out
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + EncodeTo64(ConfigurationManager.AppSettings.Get("APIKey") + ":"));
            httpClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

            //If timepoint is non zero add as a query
            string requestUri = "https://stream.companieshouse.gov.uk/companies";
            if (timepoint != 0)
            {
                requestUri = requestUri + "?timepoint=" + timepoint.ToString();
            }

            try
            {
                //Error trapping here for 401(not authorized), 416 (timepoint out of range) etc needs response so EnsureSuccessStatusCode throws an error
                var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);
                while (!reader.EndOfStream && !canToken.IsCancellationRequested)
                {

                    //Read each response, ignore blanks and get timepoint from the JSON
                    var currentLine = reader.ReadLine();
                    if (currentLine != "")
                    {
                        Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(currentLine);
                        timepoint = (int)json.SelectToken("event.timepoint");
                    }
                    Console.WriteLine(currentLine);

                }

                Console.WriteLine("Final timepoint={0}", timepoint);
            }
            catch (HttpRequestException exception)
            {
                Console.WriteLine(exception.ToString());
            }

        }

        static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

    }
}