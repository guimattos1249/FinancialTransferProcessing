using FinancialTransferProcessing.Application.Contracts;
using FinancialTransferProcessing.Application.Contracts.Messaging;
using FinancialTransferProcessing.Application.Contracts.Repositories.Accounts;
using FinancialTransferProcessing.Application.Contracts.Repositories.OutboxMessages;
using FinancialTransferProcessing.Application.Contracts.Repositories.Transfers;
using FinancialTransferProcessing.Infrastructure.Messaging;
using FinancialTransferProcessing.Infrastructure.Persistence;
using FinancialTransferProcessing.Infrastructure.Repositories;
using FinancialTransferProcessing.Infrastructure.Repositories.Accounts;
using FinancialTransferProcessing.Infrastructure.Repositories.OutboxMessages;
using FinancialTransferProcessing.Infrastructure.Repositories.Transfers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransferProcessing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAccountReadOnlyRepository, AccountRepository>();
        services.AddScoped<IAccountWriteOnlyRepository, AccountRepository>();
        services.AddScoped<ITransferReadOnlyRepository, TransferRepository>();
        services.AddScoped<ITransferWriteOnlyRepository, TransferRepository>();
        services.AddScoped<IOutboxMessageReadOnlyRepository, OutboxMessageRepository>();
        services.AddScoped<IOutboxMessageWriteOnlyRepository, OutboxMessageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IMessageSerializer, SystemTextJsonMessageSerializer>();

        return services;
    }
}
