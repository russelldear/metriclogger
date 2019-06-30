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
            _client = new HttpClient { BaseAddress = "https://www.metlink.org.nz/api/v1/StopDepartures" };
        }

        [HttpGet]
        public async Task<IActionResult> Get(string stop)
        {
            var response = await _client.GetAsync($"/{stop}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<MetlinkResponse>();

                var nextOutboundTime = result.Services
                    .Where(s => s.Direction == "Inbound")
                    .OrderByDescending(s => s.ExpectedDeparture)
                    .Select(s => s.ExpectedDeparture)
                    .First()
                    .ToString("HH:mm");

                return Ok(nextOutboundTime);
            }

            return Ok("Dunno");
        }
    }

    public partial class MetlinkResponse
    {
        [JsonProperty("Services")]
        public List<Service> Services { get; set; }
    }

    public partial class Service
    {
        [JsonProperty("Direction")]
        public string Direction { get; set; }

        [JsonProperty("ExpectedDeparture")]
        public DateTime ExpectedDeparture { get; set; }
    }
}