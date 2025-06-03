namespace TelemetryProcessor.Services
{
    public interface ITopicsService
    {
        public Task<string?> GetTopicArnByName(string topicName);
    }
}
