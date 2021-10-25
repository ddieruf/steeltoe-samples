using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultipleSubscribers;
using Steeltoe.Stream.StreamHost;

const LogLevel logLevel = LogLevel.Information;
var cts = new CancellationTokenSource();
var loggerFactory = LoggerFactory.Create(config =>
{
  config.AddConsole();
  config.AddDebug();
  config.SetMinimumLevel(logLevel);
});

var logger = loggerFactory.CreateLogger("Program");

// Set values that both hosts will use as something portable (like env var)
Environment.SetEnvironmentVariable("spring:cloud:stream:bindings:input:destination","multiple-subscribers-topic");
Environment.SetEnvironmentVariable("spring:cloud:stream:bindings:output:destination","multiple-subscribers-topic");

var subscriber1Host = await CreateAndStartSubscriber1(logger, cts.Token, logLevel);
var subscriber2Host = await CreateAndStartSubscriber2(logger, cts.Token, logLevel);

logger.LogInformation("Cancelling token in just a few seconds");
cts.CancelAfter(TimeSpan.FromSeconds(10));

logger.LogInformation("Waiting for token cancellation");
while(!cts.Token.IsCancellationRequested){}

logger.LogInformation("Stopping hosts");
await StopHosts(logger, cts.Token, subscriber1Host, subscriber2Host);

static async Task<IHost> CreateAndStartSubscriber1(ILogger logger, CancellationToken cancellationToken, LogLevel logLevel)
{
  var subscriber1Host = StreamHost
    .CreateDefaultBuilder<Subscriber1>()
    .ConfigureAppConfiguration(x => x.AddConfiguration(
        new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.Subscriber1.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables()
          .Build()
      )
    )
    .ConfigureServices((_, services) =>
    {
      services.AddLogging(builder => { builder.SetMinimumLevel(logLevel); });
    })
    .UseEnvironment("Development")
    .Build();

  logger.LogInformation("Starting subscriber 1");
  await subscriber1Host.StartAsync(cancellationToken);

  return subscriber1Host;
}

static async Task<IHost> CreateAndStartSubscriber2(ILogger logger, CancellationToken cancellationToken, LogLevel logLevel)
{
  var subscriber2Host = StreamHost
    .CreateDefaultBuilder<Subscriber2>()
    .ConfigureAppConfiguration(x => x.AddConfiguration(
        new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.Subscriber2.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables()
          .Build()
      )
    )
    .ConfigureServices((_, services) =>
    {
      services.AddLogging(builder => { builder.SetMinimumLevel(logLevel); });
    })
    .UseEnvironment("Development")
    .Build();

  logger.LogInformation("Starting subscriber 2");
  await subscriber2Host.StartAsync(cancellationToken);

  return subscriber2Host;
}

static async Task StopHosts(ILogger logger, CancellationToken cancellationToken, params IHost[] hosts)
{
  foreach (var host in hosts)
  {
    await host.StopAsync(cancellationToken);
    host.Dispose();
  }
}
