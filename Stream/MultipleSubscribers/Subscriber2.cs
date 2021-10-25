using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Steeltoe.Integration.Attributes;
using Steeltoe.Messaging;
using Steeltoe.Messaging.Handler.Attributes;
using Steeltoe.Stream.Attributes;
using Steeltoe.Stream.Messaging;

namespace MultipleSubscribers
{
  [EnableBinding(typeof(IProcessor))]
  public class Subscriber2
  {
    private readonly ILogger<Subscriber2> _logger;

    public Subscriber2(ILogger<Subscriber2> logger, ISource source)
    {
      _logger = logger;

      var i = 0;

      // Wait 1 second to ensure everything is running and then send a message every 2 seconds
      _ = new Timer(_ =>
      {
        _logger.LogInformation("Sending a message from subscriber 2");

        IMessage<DomainEvent> outgoingMessage;
        if (i % 3 == 0)
        {
          // Send an empty guid every 3rd message
          outgoingMessage = Message.Create(
            new DomainEvent("Subscriber2", "asdf"),
            new Dictionary<string, object>(){{"x-request-id",Guid.Empty}}
          );
        }
        else
        {
          outgoingMessage = Message.Create(
            new DomainEvent("Subscriber2", "asdf"),
            new Dictionary<string, object>(){{"x-request-id",Guid.NewGuid().ToString()}}
          );
        }

        source.Output.Send(outgoingMessage);
        i++;
      }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
    }

    [StreamListener(ISink.INPUT)]
    public void Handle(
      [Header(Name = "x-request-id", Required = true)] Guid requestId,
      DomainEvent domainEvent
      )
    {
      _logger.LogInformation("Subscriber 2 request '{RequestId}' with event {@Event}",requestId, domainEvent);
    }

    // Capture dead letter queue
    [ServiceActivator(ISink.INPUT + ".subscriber2.dlq")]
    public void DeadLetterQueue(IMessage message)
    {
      _logger.LogError("Subscriber2 dead letter queue: {@Message}", message);
    }

    // Capture errors for the topic individually
    /*[ServiceActivator(ISink.INPUT + ".subscriber2.errors")]
    public void Error(IMessage message)
    {
      _logger.LogError("Subscriber2 encountered an error: {@Message}", message);
    }*/
  }
}
