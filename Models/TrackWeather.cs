using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using System.Configuration;

namespace DashMaster.Models
{
    public class TrackWeather
    {
        private readonly IMemoryCache _memoryCache;

        public string accessToken = ConfigurationManager.AppSettings["WeatherToken"];
        public string baseUrl = "https://api.mapbox.com/search/geocode/v6/forward?q=";
        public Dictionary<string, string> locationData = new Dictionary<string, string>();

        private static readonly HttpClient client = new HttpClient();

        public WeatherItem weather {  get; set; }

        public TrackWeather(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<Dictionary<string, string>> GetGeoCode(string searchText)
        {

            // URL encode the search text
            string encodedSearchText = Uri.EscapeDataString(searchText);

            weather = new WeatherItem();


            // Build the request URL with parameters
            string requestUrl = $"{baseUrl}?q={encodedSearchText}&access_token={accessToken}&country=dk&limit=1";

            try
            {
                // Send the GET request
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and output the response content
                string responseBody = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(responseBody);
                JsonElement root = doc.RootElement;
                JsonElement features = root.GetProperty("features")[0];
                JsonElement full_adress = features.GetProperty("properties").GetProperty("full_address");
                JsonElement coordinates = features.GetProperty("geometry").GetProperty("coordinates");
                JsonElement locationAddress = features.GetProperty("properties").GetProperty("name");

                if (coordinates.ValueKind == JsonValueKind.Array)
                {
                    string latitude = coordinates[1].ToString();
                    string longitude = coordinates[0].ToString();

                    locationData.Add("Latitude", latitude);
                    locationData.Add("Longitude", longitude);
                    string[] parts = full_adress.ToString().Split(new[] { ',' }, 2);
                    weather.LocationName = parts[0];

                }
                else
                {
                    Console.WriteLine("The 'coordinates' element is not an array.");
                }
                return locationData;
            }
            catch (HttpRequestException e)
            {
                // Handle any errors that occurred during the request
                Console.WriteLine($"Request error: {e.Message}");
                return locationData;
            }
        }


        public async Task<WeatherItem> GetWeatherData(string searchText)
        {
            string cacheKey = $"Weather_{searchText}_{DateTime.Now:yyyyMMdd_HH}";
            string formattedDate = DateTime.Now.ToString("yyyy-MM-ddTHH\\:00");

            // Check if the data is in the cache
            if (!_memoryCache.TryGetValue(cacheKey, out WeatherItem cachedWeather))
            {
                Dictionary<string, string> geoCodeData = await GetGeoCode(searchText);
                if (geoCodeData == null || !geoCodeData.ContainsKey("Latitude") || !geoCodeData.ContainsKey("Longitude"))
                {
                    Console.WriteLine("Failed to retrieve geocode data.");
                    return weather;
                }

                var latitude = geoCodeData["Latitude"];
                var longitude = geoCodeData["Longitude"];
                string requestUrl = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&hourly=temperature_2m&timezone=auto&forecast_days=1";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();

                    using JsonDocument doc = JsonDocument.Parse(responseBody);
                    JsonElement root = doc.RootElement;
                    JsonElement timeArray = root.GetProperty("hourly").GetProperty("time");
                    JsonElement tempArray = root.GetProperty("hourly").GetProperty("temperature_2m");

                    if (timeArray.ValueKind == JsonValueKind.Array)
                    {
                        for (int i = 0; i < timeArray.GetArrayLength(); i++)
                        {
                            if (timeArray[i].ToString() == formattedDate)
                            {
                                Console.WriteLine($"Lige nu er der: {tempArray[i]} grader");
                                weather.LocationTemp = tempArray[i].ToString();
                                break; // Exit the loop once we find the correct time
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("An error occurred with the timearray kind!");
                    }

                    // Cache the weather data, even if the specific time was not found
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromHours(1)); // Cache for 1 hour

                    _memoryCache.Set(cacheKey, weather, cacheEntryOptions);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine(e);
                }
                // Return the weather (which might be incomplete if the time wasn't found)
                return weather;
            }

            // If the data is found in the cache, return it
            Console.WriteLine("Using Cache!");
            return cachedWeather;
        }
    }   
}


