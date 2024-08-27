using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DashMaster.Models
{
    public class TrackStreamerTime
    {
        public async Task<string> StreamerTime(string streamerName)
        {
            string streamerUrl = $"https://decapi.me/twitch/uptime/{streamerName}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(streamerUrl))
                    {
                        using (HttpContent content = res.Content)
                        {
                            var data = await content.ReadAsStringAsync();
                            if (data != null && !data.Contains("Offline"))
                            {
                                return data;
                            }
                            return "Offline";
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex}";
            }
        }
    }
}
