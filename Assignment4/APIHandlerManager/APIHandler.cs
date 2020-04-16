using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment4.Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace Assignment4.APIHandlerManager
{
    public class APIHandler
    {
        static string BASE_URL = "https://data.cms.gov/resource/97k6-zzx3.json";
        static string API_KEY = "ccaaGn94vH85g31bEwmX61RgRgmNLztKoV84Xayd";

        HttpClient httpClient;

        public APIHandler()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Api-Key", API_KEY);
            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public HospitalData GetHospitals()
        {
            string NATIONAL_PARK_API_PATH = BASE_URL;
            string json = "";
            string finalJson = "";

            HospitalData result = null;

            httpClient.BaseAddress = new Uri(NATIONAL_PARK_API_PATH);

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(NATIONAL_PARK_API_PATH).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }

                if (!json.Equals(""))
                {
                    // Construct proper json, because original json that is returned from the url is an array
                    finalJson = "{\"data\":" + json + "}";
                    result = JsonConvert.DeserializeObject<HospitalData>(finalJson);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }
    }
}
