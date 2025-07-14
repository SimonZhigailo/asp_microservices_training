using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PlatformService.Dto;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient, IAsyncDisposable
    {
        private readonly IConfiguration _configuration;
        private RabbitMQ.Client.IConnection _connection;
        private RabbitMQ.Client.IChannel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMqHost"],
                Port = int.Parse(_configuration["RabbitMqPort"])
            };

            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
                
                Console.WriteLine("Connected and channel opened successfully.");

                await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);

                _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Не могу подключиться или открыть канал: {ex.Message}");
            }
        }

        public async Task PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {

            if (_connection == null || !_connection.IsOpen || _channel == null || _channel.IsClosed)
                {
                    await InitializeAsync();
                }
            
            var message = JsonSerializer.Serialize(platformPublishedDto);

            if (_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ Соединение открыто, посылаю сообщение");
                await sendMessage(message);
            }
            else
            {
                Console.WriteLine("--> RabbitMQ Соединение закрыто, не посылаю сообщение");
            }
        }

        private async Task sendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(
                exchange: "trigger",
                routingKey: "",
                mandatory: true,
                body: body);

            Console.WriteLine($"--> Послал {message}");
            
        }

        private Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMq соединение закрыто");
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
    }
}