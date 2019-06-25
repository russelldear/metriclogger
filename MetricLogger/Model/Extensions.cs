using MetricLogger.Model;

namespace MetricLogger.Model
{
    public static class Extensions
    {
        public static bool IsCloudWatchable(this MetricLog metric)
        {
            double result;

            return double.TryParse(metric.Value, out result);
        }
    }
}