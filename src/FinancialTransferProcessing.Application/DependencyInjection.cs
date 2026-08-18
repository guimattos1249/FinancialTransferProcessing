using FinancialTransferProcessing.Application.UseCases.Accounts.CreateAccount;
using FinancialTransferProcessing.Application.UseCases.Accounts.GetAccountById;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransferProcessing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateAccountUseCase, CreateAccountUseCase>();
        services.AddScoped<IGetAccountByIdUseCase, GetAccountByIdUseCase>();

        return services;
    }
}
