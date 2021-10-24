
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultipleSubscribers;
using Steeltoe.Messaging;
using Steeltoe.Messaging.Core;
using Steeltoe.Stream.Binding;
using Steeltoe.Stream.StreamHost;

var cts = new CancellationTokenSource();
var loggerFactory = LoggerFactory.Create(config =>
{
  config.AddConsole();
  config.AddDebug();
  config.SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger("Program");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build()
  ;

var subscriber1Host = StreamHost
  .CreateDefaultBuilder<Subscriber1>()
  .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
  .ConfigureServices((_, services) =>
  {
    services.AddLogging(builder => { builder.SetMinimumLevel(LogLevel.Information); });
  })
  .UseEnvironment("Development")
  .Build();

var subscriber2Host = StreamHost
  .CreateDefaultBuilder<Subscriber2>()
  .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
  .ConfigureServices((_, services) =>
  {
    services.AddLogging(builder => { builder.SetMinimumLevel(LogLevel.Information); });
  })
  .UseEnvironment("Development")
  .Build();

var channelResolver = subscriber1Host.Services.GetService<IDestinationResolver<IMessageChannel>>() as BinderAwareChannelResolver;
if (channelResolver is null) throw new Exception("Could not find resolver");

logger.LogInformation("Starting subscriber 1");
await subscriber1Host.StartAsync(cts.Token);

logger.LogInformation("Starting subscriber 2");
await subscriber2Host.StartAsync(cts.Token);

logger.LogInformation("Cancelling token in just a few seconds");
cts.CancelAfter(TimeSpan.FromSeconds(200));

var messageChannel = channelResolver.ResolveDestination(IMySubscriberSink.INPUT);

logger.LogInformation("Sending a message");
var outgoingMessage = Message.Create(new DomainEvent("1234", "asdf"), new Dictionary<string, object>(){{"x-request-id",Guid.NewGuid().ToString()}});
await messageChannel.SendAsync(outgoingMessage, cts.Token);

logger.LogInformation("Sending another message");
outgoingMessage = Message.Create(new DomainEvent("6789", "hjkl"), new Dictionary<string, object>(){{"x-request-id",Guid.NewGuid().ToString()}});
await messageChannel.SendAsync(outgoingMessage, cts.Token);

logger.LogInformation("Waiting for token cancellation");
while(!cts.Token.IsCancellationRequested){}

logger.LogInformation("Stopping subscriber 1");
await subscriber1Host.StopAsync(cts.Token);

logger.LogInformation("Stopping subscriber 2");
await subscriber2Host.StopAsync(cts.Token);

subscriber1Host.Dispose();
subscriber2Host.Dispose();
