using FCG.Payments.Application;
using FCG.Payments.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

// Application Layer (registra services, handlers, etc)
builder.Services.AddApplication();

// Infrastructure Layer (registra MassTransit, Consumers, Repositories, etc)
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
await host.RunAsync();