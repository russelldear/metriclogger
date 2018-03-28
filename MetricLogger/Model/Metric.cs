using System;
using Newtonsoft.Json;

namespace MetricLogger.Model
{
    public class MetricLog
    {
        public MetricLog()
        {
            MetricId = Guid.NewGuid();
        }

        public Guid MetricId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}