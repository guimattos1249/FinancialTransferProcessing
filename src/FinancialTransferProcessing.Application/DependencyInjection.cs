using FinancialTransferProcessing.Application.UseCases.Accounts.CreateAccount;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransferProcessing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateAccountUseCase, CreateAccountUseCase>();

        return services;
    }
}
