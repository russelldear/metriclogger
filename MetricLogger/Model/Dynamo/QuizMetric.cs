using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;

namespace MetricLogger.Model.Dynamo
{
    [DynamoDBTable("MetricLog")]
    public class QuizMetric
    {
        [DynamoDBHashKey]
        public string MetricId
        {
            get { return $"{Name}"; }
            set { }
        }

        [DynamoDBProperty]
        public string Name { get; set; }

        [DynamoDBProperty]
        public QuizProperties Value { get; set; }

        [DynamoDBRangeKey]
        public DateTime Timestamp { get; set; }
    }

    public class QuizProperties
    {
        [JsonProperty("quizDate")]
        public DateTime QuizDate { get; set; }

        [JsonProperty("participants")]
        public List<Participant> Participants { get; set; }
    }

    public class Participant
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("drinks")]
        public string Drinks { get; set; }
    }
}
