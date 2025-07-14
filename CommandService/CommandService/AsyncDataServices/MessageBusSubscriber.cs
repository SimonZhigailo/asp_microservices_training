
using System.Text;
using CommandService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService, IAsyncDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private RabbitMQ.Client.IConnection _connection;
        private RabbitMQ.Client.IChannel _channel;
        private string _queueName;

        public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
        {
            _configuration = configuration;
            _eventProcessor = eventProcessor;
        }


        private async Task InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory() { HostName = _configuration["RabbitMqHost"], Port = int.Parse(_configuration["RabbitMqPort"]) };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);

            var queueDeclareResult = await _channel.QueueDeclareAsync();
            _queueName = queueDeclareResult.QueueName;

            _channel.QueueBindAsync(queue: _queueName, exchange: "trigger", routingKey: "");

            Console.WriteLine("--> Слушаю Message Bus");

            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;
        }

        private Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> Закрыто соединение с RabbitMq");
            return Task.CompletedTask;
        }
        
        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                if (!_channel.IsClosed)
                {
                    await _channel.CloseAsync();
                }
                _channel.Dispose();
                _channel = null;
            }

            if (_connection != null)
            {
                if (_connection.IsOpen)
                {
                    await _connection.CloseAsync();
                }
                _connection.Dispose();
                _connection = null;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            await InitializeRabbitMQ();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (ModuleHandle, ea) =>
            {
                Console.WriteLine("--> Событие получено");

                var body = ea.Body;
                var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

                _eventProcessor.ProcessEvent(notificationMessage);
                await Task.CompletedTask;
            };

            await _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: true,
                consumer: consumer);
                
            await Task.CompletedTask;
        }
    }
}