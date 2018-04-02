using System;
using MetricLogger.Model.Dynamo;
using Newtonsoft.Json;

namespace MetricLogger.Model
{
    public static class Converters
    {
        public static QuizMetric ToQuizMetric(this MetricLog source)
        {
            var target = new QuizMetric
            {
                Name = source.Name,
                Timestamp = source.Timestamp
            };

            target.Value = JsonConvert.DeserializeObject<QuizProperties>(source.Value);

            return target;
        }
    }
}
