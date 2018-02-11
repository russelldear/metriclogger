using System;
using System.Collections.Generic;
using System.Diagnostics;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
                    Console.WriteLine($"Metric received: {metric.Name} : {metric.Value} : {metric.Timestamp}");

                    var timeDifference = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "New Zealand Standard Time") - DateTime.UtcNow;

                    var dataPoint = new MetricDatum
                    {
                        MetricName = metric.Name,
                        Unit = StandardUnit.Count,
                        Value = metric.Value,
                        Timestamp = metric.Timestamp.AddHours(-timeDifference.TotalHours),
                        Dimensions = new List<Dimension>(),
                        StatisticValues = new StatisticSet()
                    };

                    var mdr = new PutMetricDataRequest
                    {
                        Namespace = "Environment",
                        MetricData = new List<MetricDatum>{dataPoint}
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
    }

    public class Metric
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}