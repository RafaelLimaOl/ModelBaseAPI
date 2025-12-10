using RabbitMQ.Client;
using System.Text;

namespace ModelBaseAPI.Services
{
    public class RabbitMQService : IDisposable
    {
        private readonly string _user = "guest";
        private readonly string _pass = "guest";
        private readonly string _hostName = "localhost";
        private readonly string _queueName = "messages_queue";
        private readonly string _exchangeName = "";

        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMQService()
        {
            var factory = new ConnectionFactory
            {
                UserName = _user,
                Password = _pass,
                VirtualHost = "/",
                HostName = _hostName
            };

            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;

            _channel.QueueDeclareAsync(_queueName, false, false, false, null).Wait();
        }

        public async Task SendMessageAsync(string message)
        {
            byte[] messageBodyBytes = Encoding.UTF8.GetBytes(message);
            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: _queueName,
                mandatory: false,
                basicProperties: new BasicProperties(),
                body: messageBodyBytes
            );
        }

        public async Task<string?> ReceiveMessageAsync()
        {
            var result = await _channel.BasicGetAsync(_queueName, autoAck: true);
            return result == null ? null : Encoding.UTF8.GetString(result.Body.ToArray());
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
