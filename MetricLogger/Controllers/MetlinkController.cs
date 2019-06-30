using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MetricLogger.Controllers
{
    [Route("Metlink")]
    public class MetlinkController : Controller
    {
        private readonly HttpClient _client;

        public MetlinkController()
        {
            _client = new HttpClient();
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]string stop)
        {
            var response = await _client.GetAsync($"https://www.metlink.org.nz/api/v1/StopDepartures/{stop}");

            if (response.IsSuccessStatusCode)
            {
                var resultString = await response.Content.ReadAsStringAsync();

                var metlinkResponse = JsonConvert.DeserializeObject<MetlinkResponse>(resultString);

                var nextOutboundTimeUtc = metlinkResponse.Services
                    .Where(s => s.Direction == "Inbound")
                    .Select(s => s.DisplayDeparture)
                    .First();

                var nzTime = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");

                var nextOutboundTime = TimeZoneInfo.ConvertTime(nextOutboundTimeUtc, nzTime).ToString("HH:mm");

                return Ok(JsonConvert.SerializeObject(new OutboundResponse { DisplayDeparture = nextOutboundTime }));
            }

            return Ok("Dunno");
        }
    }

    public class MetlinkResponse
    {
        [JsonProperty("Services")]
        public List<Service> Services { get; set; }
    }

    public class Service
    {
        [JsonProperty("Direction")]
        public string Direction { get; set; }

        [JsonProperty("DisplayDeparture")]
        public DateTime DisplayDeparture { get; set; }
    }

    public class OutboundResponse
    {
        public string DisplayDeparture { get; set; }
    }
}