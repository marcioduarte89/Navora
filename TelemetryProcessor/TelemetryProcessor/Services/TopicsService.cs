using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace TelemetryProcessor.Services
{
    internal class TopicsService : ITopicsService
    {
        private readonly IAmazonSimpleNotificationService _snsService;

        public TopicsService(IAmazonSimpleNotificationService snsService)
        {
            _snsService = snsService;
        }
        public async Task<string?> GetTopicArnByName(string topicName)
        {
            string? nextToken = null;

            do
            {
                var response = await _snsService.ListTopicsAsync(new ListTopicsRequest
                {
                    NextToken = nextToken
                });

                foreach (var topic in response.Topics)
                {
                    if (topic.TopicArn.EndsWith($":{topicName}"))
                    {
                        return topic.TopicArn;
                    }
                }

                nextToken = response.NextToken;

            } while (nextToken != null);

            return null; // Not found
        }
    }
}
