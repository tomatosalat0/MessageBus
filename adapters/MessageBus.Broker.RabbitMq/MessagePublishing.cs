using System;
using System.Threading;
using System.Threading.Tasks;
using MessageBus.Messaging;
using RabbitMQ.Client;

namespace MessageBus.Broker.RabbitMq
{
    internal class MessagePublishing : IDisposable
    {
        private readonly SemaphoreSlim _channelLock = new SemaphoreSlim(1, 1);
        private readonly IModel _channel;
        private bool _disposedValue;

        public MessagePublishing(IConnection connection)
        {
            _channel = connection.CreateModel();
        }

        public async Task Publish<T>(string exchange, TopicName topic, T payload)
        {
            await _channelLock.WaitAsync();
            try
            {
                byte[] data = (byte[])(object)payload!;
                ReadOnlyMemory<byte> buffer = new ReadOnlyMemory<byte>(data);

                _channel.BasicPublish(
                    exchange: exchange,
                    routingKey: topic.ToString(),
                    basicProperties: null,
                    body: buffer
                );
            }
            finally
            {
                _channelLock.Release();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _channel.Dispose();
                    _channelLock.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
