using Steeltoe.Messaging;
using Steeltoe.Stream.Attributes;

namespace MultipleSubscribers
{
  public interface IMySubscriber
  {
    [Input]
    ISubscribableChannel EventConsumers { get; }

    [Output]
    IMessageChannel EventProducers { get; }
  }
}
