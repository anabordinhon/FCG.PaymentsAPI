using FCG.Catalog.Application.Events;
using FCG.Payments.Domain.Events;

namespace FCG.Payments.Application.Common.Ports;

public interface IPaymentservice
{
    Task<PaymentProcessedEvent> ProcessPaymentAsync(OrderPlacedEvent orderEvent);

}
