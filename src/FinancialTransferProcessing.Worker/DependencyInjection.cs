using FinancialTransferProcessing.Worker.Consumers;
using FinancialTransferProcessing.Worker.Consumers.RabbitMq;

namespace FinancialTransferProcessing.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddTransferConsumerDependencies(
        this IServiceCollection services)
    {
        services.AddSingleton<TransferRequestedEnvelopeReader>();
        services.AddSingleton<TransferRequestedDeliveryHandler>();
        services.AddSingleton<RabbitMqDeliveryOperations>();
        services.AddSingleton<TransferRequestedDeliveryDispatcher>();

        return services;
    }
}