using Steeltoe.Messaging;
using Steeltoe.Stream.Attributes;

namespace MultipleSubscribers
{
  public interface IMySubscriber : IMySubscriberSink
  {

  }

  public interface IMySubscriberSink
  {
    const string INPUT = "input";

    [Input(INPUT)]
    ISubscribableChannel Input { get; }
  }
}
