using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FinancialTransferProcessing.Infrastructure.Messaging;

internal sealed class RabbitMqConnectionProvider : IAsyncDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private IConnection? _connection;

    public RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options)
    {
        var rabbitMqOptions = options.Value;

        _connectionFactory = new ConnectionFactory
        {
            HostName = rabbitMqOptions.HostName,
            Port = rabbitMqOptions.Port,
            VirtualHost = rabbitMqOptions.VirtualHost,
            UserName = rabbitMqOptions.UserName,
            Password = rabbitMqOptions.Password,
            ClientProvidedName = rabbitMqOptions.ClientProvidedName,
        };
    }

    public async Task<IConnection> GetConnectionAsync(
        CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            if (_connection is not null)
                await _connection.DisposeAsync();

            _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();

        _connectionLock.Dispose();
    }
}
