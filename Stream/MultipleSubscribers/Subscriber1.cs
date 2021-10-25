using System;
using Microsoft.Extensions.Logging;
using Steeltoe.Integration.Attributes;
using Steeltoe.Messaging;
using Steeltoe.Messaging.Handler.Attributes;
using Steeltoe.Stream.Attributes;
using Steeltoe.Stream.Messaging;

namespace MultipleSubscribers
{
  [EnableBinding(typeof(IProcessor))]
  public class Subscriber1
  {
    private readonly ILogger<Subscriber1> _logger;

    public Subscriber1(ILogger<Subscriber1> logger, ISource source)
    {
      _logger = logger;
    }

    [StreamListener(ISink.INPUT)]
    public void Handle(
      DomainEvent domainEvent,
      [Header(Name = "x-request-id", Required = true)] Guid requestId
    )
    {
      _logger.LogInformation("Subscriber 1 request '{RequestId}' with event {@Event}",requestId, domainEvent);

      // Randomly throw an error
      if (requestId == Guid.Empty)
      {
        //throw new Exception("Request id is required to process events");
      }
    }

    // Capture dead letter queue
    [ServiceActivator("multiple-subscribers-topic.subscriber1.dlq")]
    public void DeadLetterQueue(IMessage message)
    {
      _logger.LogError("Subscriber1 dead letter queue: {@Message}", message);
    }

    // Capture errors for the topic individually
    /*[ServiceActivator(ISink.INPUT + ".subscriber1.errors")]
    public void Error(IMessage message)
    {
      _logger.LogError("Subscriber1 encountered an error: {@Message}", message);
    }*/

    // Capture errors for all topics
    [ServiceActivator("errorChannel")]
    public void Error(IMessage message)
    {
      _logger.LogError("An error was encountered: {@Message}", message);
    }
  }
}
