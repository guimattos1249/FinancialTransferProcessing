using FinancialTransferProcessing.Worker.Consumers;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IValidateOptions<TransferConsumerOptions>, TransferConsumerOptionsValidator>();

builder.Services.AddOptions<TransferConsumerOptions>()
    .Bind(builder.Configuration.GetSection(TransferConsumerOptions.SectionName))
    .ValidateOnStart();

var host = builder.Build();
await host.RunAsync();
