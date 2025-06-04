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
            Console.WriteLine($"Got into TopicsService with topic name {topicName}");
            string? nextToken = null;

            do
            {
                try
                {
                    var response = await _snsService.ListTopicsAsync(new ListTopicsRequest
                    {
                        NextToken = nextToken
                    });

                    foreach (var topic in response.Topics)
                    {
                        Console.WriteLine($"Listing topic with topic arn {topic.TopicArn}");
                        if (topic.TopicArn.EndsWith($":{topicName}"))
                        {
                            Console.WriteLine($"Found topic");
                            return topic.TopicArn;
                        }
                    }

                    nextToken = response.NextToken;
                }catch(Exception ex)
                {
                    Console.WriteLine($"Exception happened {ex.Message} with stack: {ex.StackTrace} with inner exception: {ex.InnerException}");
                }

            } while (nextToken != null);

            return null; // Not found
        }
    }
}
