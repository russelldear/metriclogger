using System;
using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;

namespace MetricLogger.Model
{
    [DynamoDBTable("MetricLog")]
    public class MetricLog
    {
        [DynamoDBHashKey]
        [JsonIgnore]
        public string MetricId
        {
            get { return $"{Name}"; }
            set { }
        }

        [DynamoDBProperty]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DynamoDBProperty]
        [JsonProperty("value")]
        public string Value { get; set; }

        [DynamoDBRangeKey]
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}