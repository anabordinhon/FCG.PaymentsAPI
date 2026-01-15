using FCG.Catalog.Application.Events;
using FCG.Payments.Domain.Events;
using FCG.Payments.Infrastructure.Adapters.Events.Consumers;
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
                cfg.Host(configuration["RabbitMQ:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.Message<PaymentProcessedEvent>(m =>
                {
                    m.SetEntityName("payment-processed");
                });

                cfg.Message<OrderPlacedEvent>(m =>
                {
                    m.SetEntityName("order-placed");
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