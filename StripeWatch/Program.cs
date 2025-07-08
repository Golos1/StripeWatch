using StripeWatch;
var builder = Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
{
    services.AddHostedService<StripeMonitor>(); // Register your Worker Service
});
var host = builder.Build();
host.Run();