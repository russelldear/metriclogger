﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using MetricLogger.Model;
using MetricLogger.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetricLogger.Controllers
{
    [Route("Metrics")]
    public class MetricsController : Controller
    {
        private readonly DynamoDbService _dynamoDbService;

        public MetricsController()
        {
            _dynamoDbService = new DynamoDbService();
        }

        [HttpGet]
        public IActionResult Get()
        {
            return new OkResult();
        }

        [HttpPost]
        public IActionResult Post([FromBody]MetricLogs metricContainer)
        {
            foreach (var metric in metricContainer.Metrics)
            {
                Console.WriteLine($"Metric received - {metric.Name} : {metric.Value} : {metric.Timestamp}");

                if (metric.IsCloudWatchable())
                {
                    if (!LogToCloudWatch(metric))
                    {
                        return new BadRequestResult();
                    }
                }

                if (!LogToDynamo(metric))
                {
                    return new BadRequestResult();
                }
            }

            return new OkResult();
        }

        private bool LogToCloudWatch(MetricLog metric)
        {
            try
            {
                using (var cloudwatch = new AmazonCloudWatchClient(Environment.GetEnvironmentVariable("AWSAccessKey"), Environment.GetEnvironmentVariable("AWSSecret"), RegionEndpoint.USEast1))
                {
                    var timestamp = GetTimestamp(metric);

                    var dataPoint = new MetricDatum
                    {
                        MetricName = metric.Name,
                        Unit = StandardUnit.Count,
                        Value = double.Parse(metric.Value),
                        TimestampUtc = timestamp,
                        Dimensions = new List<Dimension>(),
                        StatisticValues = new StatisticSet()
                    };

                    var mdr = new PutMetricDataRequest
                    {
                        Namespace = "Environment",
                        MetricData = new List<MetricDatum> { dataPoint }
                    };

                    var resp = cloudwatch.PutMetricDataAsync(mdr).Result;

                    //Console.WriteLine(resp.HttpStatusCode);

                    Debug.Assert(resp.HttpStatusCode == System.Net.HttpStatusCode.OK);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);

                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;

                    Console.WriteLine(ex.Message + ex.StackTrace);
                }

                return false;
            }
        }

        private bool LogToDynamo(MetricLog metric)
        {
            try
            {
                _dynamoDbService.AddMetric(metric);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);

                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;

                    Console.WriteLine(ex.Message + ex.StackTrace);
                }

                return false;
            }
        }

        private static DateTime GetTimestamp(MetricLog metric)
        {
            var utcTimezone = TimeZoneInfo.Utc;
            var nzTimezone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");

            var timestampAsUtc = TimeZoneInfo.ConvertTime(metric.Timestamp, nzTimezone, utcTimezone);

            //Console.WriteLine($"Timestamp as UTC: {timestampAsUtc}");

            return timestampAsUtc;
        }
    }
}