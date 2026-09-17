using FinancialTransferProcessing.Worker.Consumers.Configuration;
using FinancialTransferProcessing.Application;
using FinancialTransferProcessing.Infrastructure;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IValidateOptions<TransferConsumerOptions>, TransferConsumerOptionsValidator>();

builder.Services.AddOptions<TransferConsumerOptions>()
    .Bind(builder.Configuration.GetSection(TransferConsumerOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
await host.RunAsync();
