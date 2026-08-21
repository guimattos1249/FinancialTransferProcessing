using FinancialTransferProcessing.Application.UseCases.Accounts.CreateAccount;
using FinancialTransferProcessing.Application.UseCases.Accounts.GetAccountById;
using FinancialTransferProcessing.Application.UseCases.Transfers.CreateTransfer;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransferProcessing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateAccountUseCase, CreateAccountUseCase>();
        services.AddScoped<IGetAccountByIdUseCase, GetAccountByIdUseCase>();
        services.AddScoped<ICreateTransferUseCase, CreateTransferUseCase>();

        return services;
    }
}
