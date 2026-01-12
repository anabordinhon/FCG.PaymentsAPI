using FCG.Catalog.Infrastructure.Adapters.Events.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderPlacedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitConfig = configuration.GetSection("RabbitMQ");

                cfg.Host(
                    rabbitConfig["Host"] ?? "localhost",
                    rabbitConfig["VirtualHost"] ?? "/",
                    h =>
                    {
                        h.Username(rabbitConfig["Username"] ?? "guest");
                        h.Password(rabbitConfig["Password"] ?? "guest");
                    });


                cfg.ReceiveEndpoint("order-placed-queue", e =>
                {
                    e.ConfigureConsumer<OrderPlacedConsumer>(context);
                });
            });
        });

        return services;
    }
}