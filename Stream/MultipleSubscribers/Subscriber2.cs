using Microsoft.Extensions.Logging;
using Steeltoe.Stream.Attributes;

namespace MultipleSubscribers
{
  [EnableBinding(typeof(IMySubscriber))]
  public class Subscriber2
  {
    private readonly ILogger _logger;

    public Subscriber2(ILogger<Subscriber2> logger)
    {
      _logger = logger;
    }

    [StreamListener("EventConsumers")]
    public void Handle(DomainEvent domainEvent)
    {
      _logger.LogInformation("Subscriber 2 found event {@Event}", domainEvent);

    }
  }
}
