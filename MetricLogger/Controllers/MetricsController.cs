using System;
using System.Collections.Generic;
using System.Diagnostics;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NodaTime;

namespace MetricLogger.Controllers
{
    [Route("Metrics")]
    public class MetricsController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return new OkResult();
        }

        [HttpPost]
        public IActionResult Post([FromBody]Metric metric)
        {
            try
            {
                using (var cloudwatch = new AmazonCloudWatchClient(Environment.GetEnvironmentVariable("AWSAccessKey"), Environment.GetEnvironmentVariable("AWSSecret"), RegionEndpoint.USEast1))
                {
                    Console.WriteLine($"Metric received - {metric.Name} : {metric.Value} : {metric.Timestamp}");

                    var timestamp = GetTimestamp(metric);

                    var dataPoint = new MetricDatum
                    {
                        MetricName = metric.Name,
                        Unit = StandardUnit.Count,
                        Value = metric.Value,
                        Timestamp = timestamp,
                        Dimensions = new List<Dimension>(),
                        StatisticValues = new StatisticSet()
                    };

                    var mdr = new PutMetricDataRequest
                    {
                        Namespace = "Environment",
                        MetricData = new List<MetricDatum> { dataPoint }
                    };

                    var resp = cloudwatch.PutMetricDataAsync(mdr).Result;

                    Console.WriteLine(resp.HttpStatusCode);

                    Debug.Assert(resp.HttpStatusCode == System.Net.HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);

                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;

                    Console.WriteLine(ex.Message + ex.StackTrace);
                }

                return new BadRequestResult();
            }

            return new OkResult();
        }

        private static DateTime GetTimestamp(Metric metric)
        {
            var utcTimezone = TimeZoneInfo.Utc;
            var nzTimezone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");

            var timestampAsUtc = TimeZoneInfo.ConvertTime(metric.Timestamp, nzTimezone, utcTimezone);

            Console.WriteLine($"Timestamp as UTC: {timestampAsUtc}");

            return timestampAsUtc;
        }
    }

    public class Metric
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}