using System;
using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;

namespace MetricLogger.Model
{
    [DynamoDBTable("Metrics")]
    public class MetricLog
    {
        [DynamoDBHashKey]
        [JsonIgnore]
        public string MetricId
        {
            get { return $"{Name}-{Timestamp:yyyy-MM-dd-hh-mm-ss-fff}"; }
            set { }
        }

        [DynamoDBProperty]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DynamoDBProperty]
        [JsonProperty("value")]
        public double Value { get; set; }

        [DynamoDBProperty]
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}