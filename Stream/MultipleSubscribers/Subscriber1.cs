using Microsoft.Extensions.Logging;
using Steeltoe.Stream.Attributes;

namespace MultipleSubscribers
{
  [EnableBinding(typeof(IMySubscriber))]
  public class Subscriber1
  {
    private readonly ILogger _logger;

    public Subscriber1(ILogger<Subscriber1> logger)
    {
      _logger = logger;
    }

    [StreamListener(IMySubscriberSink.INPUT)]
    public void Handle(DomainEvent domainEvent)
    {
      _logger.LogInformation("Subscriber 1 found event {@Event}", domainEvent);

    }
  }
}
