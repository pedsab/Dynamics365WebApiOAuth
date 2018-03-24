using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics365WebApiOAuth
{
    class Program
    {
        public static string dynamics365_baseurl = "https://labethom.api.crm2.dynamics.com/";
        public static string tenantid = "88c6ab5a-c061-429b-8379-3fc56d15fe40";
        public static string username = "leonardo.castro@labetcrm.onmicrosoft.com";
        public static string password = "labet@18";
        public static string client_id = "af8970da-e117-40cc-9d14-39555c623b7f";
        public static string client_secret = "wMdxTt368Bz8UKNkza1SwFYdz+rANSQR5BMv968q4Rw=";

        static void Main(string[] args)
        {
            Program.RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {
            try
            {
                var accessToken = await GetAccessToken();
                var data = await GetAsync(accessToken, "/api/data/v8.2/accounts?$filter=i9_cnpj eq '01.161.180/0001-56'");
                var jsonObject = JObject.Parse(data);
                var records = jsonObject["value"].ToList();

                records.ForEach(record =>
                {
                    Console.WriteLine($"accountid: {record["accountid"].Value<string>()}");
                    Console.WriteLine($"name: {record["name"].Value<string>()}");
                    Console.WriteLine();
                });

                Console.ReadLine();
            }
            catch (Exception)
            {
                throw;
            }
        }

        static async Task<string> GetAsync(string accessToken, string path)
        {
            string result = null;

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 2, 0);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.BaseAddress = new Uri(dynamics365_baseurl);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage httpResponse = await httpClient.GetAsync(path);

                if (httpResponse.IsSuccessStatusCode)
                {
                    result = await httpResponse.Content.ReadAsStringAsync();

                    return result;
                }
            }

            return string.Empty;
        }

        static async Task<string> GetAccessToken()
        {
            string result = null;

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 2, 0);
                httpClient.BaseAddress = new Uri("https://login.microsoftonline.com");
                httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

                var keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("grant_type", "password"));
                keyValues.Add(new KeyValuePair<string, string>("client_id", client_id));
                keyValues.Add(new KeyValuePair<string, string>("resource", dynamics365_baseurl));
                keyValues.Add(new KeyValuePair<string, string>("username", username));
                keyValues.Add(new KeyValuePair<string, string>("password", password));
                keyValues.Add(new KeyValuePair<string, string>("client_secret", client_secret));

                var formUrlEncodedContent = new FormUrlEncodedContent(keyValues);

                HttpResponseMessage httpResponse = await httpClient.PostAsync($"/{tenantid}/oauth2/token", formUrlEncodedContent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    result = await httpResponse.Content.ReadAsStringAsync();

                    dynamic jsonResponse = JsonConvert.DeserializeObject(result);

                    var accessToken = Convert.ToString(jsonResponse["access_token"]);

                    return accessToken;
                }
            }

            return string.Empty;
        }
    }
}
